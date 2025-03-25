using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Extensions;
using ScreenToGif.Util.InterProcessChannel;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using ScreenToGif.Windows.Other;

namespace ScreenToGif;

public partial class App : IDisposable
{
    #region Properties

    internal static NotifyIcon NotifyIcon { get; private set; }
    internal static ApplicationViewModel MainViewModel { get; private set; }

    private Mutex _singleInstanceMutex;
    private bool _isMutexOwned;
    private readonly List<Exception> _handledExceptions = new();
    private readonly object _exceptionLock = new();
        
    #endregion

    #region Event Handlers

    private void App_Startup(object sender, StartupEventArgs e)
    {
        InitializeApplication();
        HandleStartupArguments(e.Args);
        InitializeUiComponents();
        StartBackgroundTasks();
        LaunchStartupWindow();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
            HandleUnhandledException(exception);
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General) 
            return;

        UpdateApplicationTheme();
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        CleanupResources();
    }

    #endregion

    #region Initialization Methods

    private void InitializeApplication()
    {
        Global.StartupDateTime = DateTime.Now;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), 
            new FrameworkPropertyMetadata(int.MaxValue));
        
        ConfigureSecurityProtocol();
        LocalizationHelper.SelectCulture(UserSettings.All.LanguageCode);
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    private void ConfigureSecurityProtocol()
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | 
                                                 SecurityProtocolType.Tls12 | 
                                                 SecurityProtocolType.Tls13;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to set network security protocol");
        }
    }

    private void HandleStartupArguments(string[] args)
    {
        Arguments.Prepare(args);

        if (Arguments.IsInDownloadMode)
        {
            HandleDownloadMode();
            return;
        }

        if (Arguments.IsInSettingsMode)
        {
            SettingsPersistenceChannel.RegisterServer();
            return;
        }

        if (UserSettings.All.SingleInstance && !Arguments.NewInstance)
        {
            InitializeSingleInstance();
        }
    }

    private void HandleDownloadMode()
    {
        var downloader = new Downloader
        {
            DownloadMode = Arguments.DownloadMode,
            DestinationPath = Arguments.DownloadPath
        };
        downloader.ShowDialog();
        Environment.Exit(90);
    }

    private void InitializeSingleInstance()
    {
        try
        {
            using var currentProcess = Process.GetCurrentProcess();
            var userIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().User;
            var executablePath = currentProcess.MainModule?.FileName ?? 
                               Assembly.GetEntryAssembly()?.Location ?? "ScreenToGif";
            var locationHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(executablePath));
            var mutexName = (userIdentity?.Value ?? Environment.UserName) + "_" + locationHash;

            _singleInstanceMutex = new Mutex(true, mutexName, out _isMutexOwned);

            if (!_isMutexOwned)
            {
                SwitchToExistingInstance(currentProcess);
                Environment.Exit(0);
            }

            InstanceSwitcherChannel.RegisterServer(InstanceSwitch_Received);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to initialize single instance mode");
        }
    }

    private void SwitchToExistingInstance(Process currentProcess)
    {
        var warningShown = true;

        using var existingProcess = Process.GetProcessesByName(currentProcess.ProcessName)
            .FirstOrDefault(p => p.MainWindowHandle != currentProcess.MainWindowHandle);

        if (existingProcess != null)
        {
            var windowHandles = Native.Helpers.Windows.GetWindowHandlesFromProcess(existingProcess);
            var mainHandle = windowHandles.Count > 0 ? windowHandles[0] : existingProcess.Handle;

            Native.External.User32.ShowWindow(mainHandle, Domain.Enums.Native.ShowWindowCommands.Show);
            Native.External.User32.SetForegroundWindow(mainHandle);
            warningShown = false;

            InstanceSwitcherChannel.SendMessage(existingProcess.Id, Environment.GetCommandLineArgs());
        }

        if (warningShown)
        {
            Dialog.Ok(
                LocalizationHelper.Get("S.Warning.Single.Title"), 
                LocalizationHelper.Get("S.Warning.Single.Header"), 
                LocalizationHelper.Get("S.Warning.Single.Message"), 
                Icons.Info);
        }
    }

    private void InitializeUiComponents()
    {
        RenderOptions.ProcessRenderMode = UserSettings.All.DisableHardwareAcceleration 
            ? RenderMode.SoftwareOnly 
            : RenderMode.Default;

        ConfigureDispatcherWorkaround();
        InitializeSystemTray();
        RegisterGlobalShortcuts();
    }

    private void ConfigureDispatcherWorkaround()
    {
        try
        {
            if (UserSettings.All.WorkaroundQuota)
            {
                BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure = 
                    BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Reset;
            }

#if DEBUG
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
            BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure = 
                BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Throw;
#endif
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to configure dispatcher workaround");
        }
    }

    private void InitializeSystemTray()
    {
        NotifyIcon = (NotifyIcon)FindResource("NotifyIcon");

        if (NotifyIcon != null)
        {
            NotifyIcon.Visibility = ShouldShowNotificationIcon() 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        MainViewModel = (ApplicationViewModel)FindResource("AppViewModel") ?? new ApplicationViewModel();
    }

    private bool ShouldShowNotificationIcon()
    {
        // Migrate old setting to new format
        if (UserSettings.All.StartUp == 5)
        {
            UserSettings.All.StartMinimized = true;
            UserSettings.All.ShowNotificationIcon = true;
            UserSettings.All.StartUp = 0;
        }

        return UserSettings.All.ShowNotificationIcon || 
               UserSettings.All.StartMinimized;
    }

    #endregion

    #region Runtime Methods

    private void StartBackgroundTasks()
    {
        Task.Factory.StartNew(MainViewModel.ClearTemporaryFiles, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(async () => await MainViewModel.CheckForUpdates(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(MainViewModel.SendFeedback, TaskCreationOptions.LongRunning);
    }

    private void LaunchStartupWindow()
    {
        MainViewModel.Open.Execute(Arguments.Open 
            ? Arguments.WindownToOpen 
            : UserSettings.All.StartUp, 
            Arguments.Open);
    }

    internal static void InstanceSwitch_Received(object sender, InstanceSwitcherMessage message)
    {
        try
        {
            if (message.Args?.Length > 0)
                Arguments.Prepare(message.Args);

            MainViewModel.Open.Execute(Arguments.Open 
                ? Arguments.WindownToOpen 
                : UserSettings.All.StartUp, 
                Arguments.Open);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Failed to process instance switch message");
        }
    }

    private void UpdateApplicationTheme()
    {
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);

        if (UserSettings.All.GridColorsFollowSystem)
        {
            var isDarkTheme = ThemeHelper.IsSystemUsingDarkTheme();
            UserSettings.All.GridColor1 = isDarkTheme ? Constants.DarkEven : Constants.VeryLightEven;
            UserSettings.All.GridColor2 = isDarkTheme ? Constants.DarkOdd : Constants.VeryLightOdd;
        }
    }

    private void HandleUnhandledException(Exception exception)
    {
        LogWriter.Log(exception, "Unhandled exception occurred");

        lock (_exceptionLock)
        {
            if (_handledExceptions.Any(ex => ex.Message == exception.Message))
                return;

            _handledExceptions.Add(exception);

            Current.Dispatcher.Invoke(() =>
            {
                var isXamlParseException = exception is XamlParseException && 
                                         exception.InnerException is TargetInvocationException;

                ExceptionDialog.Ok(exception, "ScreenToGif", 
                    isXamlParseException && Global.IsHotFix4055002Installed 
                        ? "Error while rendering visuals" 
                        : "Unhandled exception", 
                    exception.Message);
            });

            _handledExceptions.Remove(exception);
        }
    }

    #endregion

    #region Cleanup Methods

    private void CleanupResources()
    {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        ReleaseMutexes();
        DisposeSystemTrayIcon();
        StopAllEncodings();
        SaveUserSettings();
        ReleaseSingleInstanceMutex();
        DisposeHotkeys();
    }

    private void ReleaseMutexes()
    {
        try
        {
            MutexList.RemoveAll();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to remove project mutexes");
        }
    }

    private void DisposeSystemTrayIcon()
    {
        try
        {
            NotifyIcon?.Dispose();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to dispose system tray icon");
        }
    }

    private void StopAllEncodings()
    {
        try
        {
            EncodingManager.StopAllEncodings();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to stop encodings");
        }
    }

    private void SaveUserSettings()
    {
        try
        {
            SettingsExtension.ForceSave();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to save user settings");
        }
    }

    private void ReleaseSingleInstanceMutex()
    {
        try
        {
            if (_singleInstanceMutex != null && _isMutexOwned)
            {
                _singleInstanceMutex.ReleaseMutex();
                _isMutexOwned = false;
            }
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to release single instance mutex");
        }
    }

    private void DisposeHotkeys()
    {
        try
        {
            HotKeyCollection.Default.Dispose();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to dispose hotkeys");
        }
    }

    #endregion

    #region Shortcut Methods

    private void RegisterGlobalShortcuts()
    {
        MainViewModel.RecorderGesture = RegisterShortcut(
            UserSettings.All.RecorderModifiers, 
            UserSettings.All.RecorderShortcut, 
            () => MainViewModel.OpenRecorder.Execute(null));

        MainViewModel.WebcamRecorderGesture = RegisterShortcut(
            UserSettings.All.WebcamRecorderModifiers, 
            UserSettings.All.WebcamRecorderShortcut, 
            () => MainViewModel.OpenWebcamRecorder.Execute(null));

        MainViewModel.BoardRecorderGesture = RegisterShortcut(
            UserSettings.All.BoardRecorderModifiers, 
            UserSettings.All.BoardRecorderShortcut, 
            () => MainViewModel.OpenBoardRecorder.Execute(null));

        MainViewModel.EditorGesture = RegisterShortcut(
            UserSettings.All.EditorModifiers, 
            UserSettings.All.EditorShortcut, 
            () => MainViewModel.OpenEditor.Execute(null));

        MainViewModel.OptionsGesture = RegisterShortcut(
            UserSettings.All.OptionsModifiers, 
            UserSettings.All.OptionsShortcut, 
            () => MainViewModel.OpenOptions.Execute(null));

        MainViewModel.ExitGesture = RegisterShortcut(
            UserSettings.All.ExitModifiers, 
            UserSettings.All.ExitShortcut, 
            () => MainViewModel.ExitApplication.Execute(null));
    }

    private string RegisterShortcut(ModifierKeys modifiers, Key shortcut, Action action)
    {
        var registered = HotKeyCollection.Default.TryRegisterHotKey(
            modifiers, 
            shortcut, 
            () => { if (!Global.IgnoreHotKeys) action(); }, 
            true);

        return registered 
            ? Native.Helpers.Other.GetSelectKeyText(shortcut, modifiers, true, true) 
            : string.Empty;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_singleInstanceMutex != null && _isMutexOwned)
        {
            _singleInstanceMutex.ReleaseMutex();
            _isMutexOwned = false;
        }

        _singleInstanceMutex?.Dispose();
    }

    #endregion
}