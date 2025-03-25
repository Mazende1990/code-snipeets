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
    #region Fields

    private Mutex _mutex;
    private bool _accepted;
    private readonly List<Exception> _exceptionList = new();
    private readonly object _lock = new();

    #endregion

    #region Properties

    internal static NotifyIcon NotifyIcon { get; private set; }
    internal static ApplicationViewModel MainViewModel { get; private set; }

    #endregion

    #region Events

    private void App_Startup(object sender, StartupEventArgs e)
    {
        InitializeApplication(e.Args);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleDispatcherException(e.Exception);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            HandleDomainException(exception);
        }
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        HandleUserPreferenceChange(e);
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        CleanupApplication();
    }

    #endregion

    #region Methods

    private void InitializeApplication(string[] args)
    {
        Global.StartupDateTime = DateTime.Now;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

        SetSecurityProtocol();
        Arguments.Prepare(args);
        LocalizationHelper.SelectCulture(UserSettings.All.LanguageCode);
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        if (Arguments.IsInDownloadMode)
        {
            StartDownloadMode();
            return;
        }

        if (Arguments.IsInSettingsMode)
        {
            StartSettingsMode();
            return;
        }

        if (UserSettings.All.SingleInstance && !Arguments.NewInstance)
        {
            if (HandleSingleInstance(args)) return;
        }

        RenderOptions.ProcessRenderMode = UserSettings.All.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;
        SetWorkaroundForDispatcher();

        NotifyIcon = (NotifyIcon)FindResource("NotifyIcon");
        ConfigureNotifyIcon();

        MainViewModel = (ApplicationViewModel)FindResource("AppViewModel") ?? new ApplicationViewModel();
        RegisterShortcuts();

        StartBackgroundTasks();
        StartApplication(args);
    }

    private void StartDownloadMode()
    {
        var downloader = new Downloader
        {
            DownloadMode = Arguments.DownloadMode,
            DestinationPath = Arguments.DownloadPath
        };
        downloader.ShowDialog();
        Environment.Exit(90);
    }

    private void StartSettingsMode()
    {
        SettingsPersistenceChannel.RegisterServer();
    }

    private bool HandleSingleInstance(string[] args)
    {
        try
        {
            using var thisProcess = Process.GetCurrentProcess();
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
            var name = thisProcess.MainModule?.FileName ?? Assembly.GetEntryAssembly()?.Location ?? "ScreenToGif";
            var location = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
            var mutexName = (user?.Value ?? Environment.UserName) + "_" + location;
            _mutex = new Mutex(true, mutexName, out _accepted);

            if (!_accepted)
            {
                HandleExistingInstance(thisProcess, args);
                return true;
            }

            InstanceSwitcherChannel.RegisterServer(InstanceSwitch_Received);
            return false;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to check if another instance is running");
            return false;
        }
    }

    private void HandleExistingInstance(Process thisProcess, string[] args)
    {
        var warning = true;
        using var process = Process.GetProcessesByName(thisProcess.ProcessName).FirstOrDefault(f => f.MainWindowHandle != thisProcess.MainWindowHandle);
        if (process != null)
        {
            var handles = Native.Helpers.Windows.GetWindowHandlesFromProcess(process);
            Native.External.User32.ShowWindow(handles.Count > 0 ? handles[0] : process.Handle, ShowWindowCommands.Show);
            Native.External.User32.SetForegroundWindow(handles.Count > 0 ? handles[0] : process.Handle);
            warning = false;
            InstanceSwitcherChannel.SendMessage(process.Id, args);
        }

        if (warning)
        {
            Dialog.Ok(LocalizationHelper.Get("S.Warning.Single.Title"), LocalizationHelper.Get("S.Warning.Single.Header"), LocalizationHelper.Get("S.Warning.Single.Message"), Icons.Info);
        }
        Environment.Exit(0);
    }

    internal static void InstanceSwitch_Received(object _, InstanceSwitcherMessage message)
    {
        try
        {
            var args = message.Args;
            if (args?.Length > 0) Arguments.Prepare(args);
            StartApplication(args);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Unable to execute arguments from IPC.");
        }
    }

    private void ConfigureNotifyIcon()
    {
        if (NotifyIcon != null)
        {
            NotifyIcon.Visibility = UserSettings.All.ShowNotificationIcon || UserSettings.All.StartMinimized || UserSettings.All.StartUp == 5 ? Visibility.Visible : Visibility.Collapsed;
            if (UserSettings.All.StartUp == 5)
            {
                UserSettings.All.StartMinimized = true;
                UserSettings.All.ShowNotificationIcon = true;
                UserSettings.All.StartUp = 0;
            }
        }
    }

    private void StartBackgroundTasks()
    {
        Task.Factory.StartNew(MainViewModel.ClearTemporaryFiles, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(async () => await MainViewModel.CheckForUpdates(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(MainViewModel.SendFeedback, TaskCreationOptions.LongRunning);
    }

    private static void StartApplication(string[] args)
    {
        if (Arguments.Open)
            MainViewModel.Open.Execute(Arguments.WindownToOpen, true);
        else
            MainViewModel.Open.Execute(UserSettings.All.StartUp);
    }

    private void HandleDispatcherException(Exception exception)
    {
        LogWriter.Log(exception, "On dispacher unhandled exception - Unknown");
        try
        {
            ShowException(exception);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Error while displaying the error.");
        }
    }

    private void HandleDomainException(Exception exception)
    {
        LogWriter.Log(exception, "Current domain unhandled exception - Unknown");
        try
        {
            ShowException(exception);
        }
        catch (Exception)
        {
        }
    }

    private void HandleUserPreferenceChange(UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General) return;
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        if (UserSettings.All.GridColorsFollowSystem)
        {
            var isSystemUsingDark = ThemeHelper.IsSystemUsingDarkTheme();
            UserSettings.All.GridColor1 = isSystemUsingDark ? Constants.DarkEven : Constants.VeryLightEven;
            UserSettings.All.GridColor2 = isSystemUsingDark ? Constants.DarkOdd : Constants.VeryLightOdd;
        }
    }

    private void CleanupApplication()
    {
        SystemEvents