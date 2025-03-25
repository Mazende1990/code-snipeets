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
    private const int EXIT_CODE_DOWNLOAD_MODE = 90;

    #region Properties and Fields
    internal static NotifyIcon NotifyIcon { get; private set; }
    internal static ApplicationViewModel MainViewModel { get; private set; }

    private Mutex _singleInstanceMutex;
    private bool _mutexAcquired;
    private readonly List<Exception> _exceptionList = new();
    private readonly object _exceptionLock = new();
    #endregion

    #region Startup and Initialization
    private void App_Startup(object sender, StartupEventArgs e)
    {
        InitializeStartup(e);
        ConfigureUIAndSettings();
        HandleStartupModes(e);
        SetupBackgroundTasks();
        HandleStartupAction();
    }

    private void InitializeStartup(StartupEventArgs e)
    {
        Global.StartupDateTime = DateTime.Now;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
        SetSecurityProtocol();
        Arguments.Prepare(e.Args);
    }

    private void ConfigureUIAndSettings()
    {
        LocalizationHelper.SelectCulture(UserSettings.All.LanguageCode);
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        RenderOptions.ProcessRenderMode = UserSettings.All.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;
        SetWorkaroundForDispatcher();
    }

    private void HandleStartupModes(StartupEventArgs e)
    {
        if (Arguments.IsInDownloadMode)
            HandleDownloadMode(e);

        if (Arguments.IsInSettingsMode)
            HandleSettingsMode();

        HandleSingleInstanceMode(e);
    }

    private void HandleDownloadMode(StartupEventArgs e)
    {
        var downloader = new Downloader
        {
            DownloadMode = Arguments.DownloadMode,
            DestinationPath = Arguments.DownloadPath
        };
        downloader.ShowDialog();
        Environment.Exit(EXIT_CODE_DOWNLOAD_MODE);
    }

    private void HandleSettingsMode()
    {
        SettingsPersistenceChannel.RegisterServer();
    }

    private void HandleSingleInstanceMode(StartupEventArgs e)
    {
        if (UserSettings.All.SingleInstance && !Arguments.NewInstance)
        {
            TryAcquireSingleInstanceMutex(e);
        }
    }

    private void TryAcquireSingleInstanceMutex(StartupEventArgs e)
    {
        try
        {
            using var thisProcess = Process.GetCurrentProcess();
            var mutexName = CreateUniqueMutexName(thisProcess);

            _singleInstanceMutex = new Mutex(true, mutexName, out _mutexAcquired);

            if (!_mutexAcquired)
            {
                HandleExistingInstance(thisProcess, e);
            }
            else
            {
                InstanceSwitcherChannel.RegisterServer(InstanceSwitch_Received);
            }
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to check if another instance is running");
        }
    }

    private string CreateUniqueMutexName(Process thisProcess)
    {
        var name = thisProcess.MainModule?.FileName ?? Assembly.GetEntryAssembly()?.Location ?? "ScreenToGif";
        var location = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
        var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
        return (user?.Value ?? Environment.UserName) + "_" + location;
    }

    private void HandleExistingInstance(Process thisProcess, StartupEventArgs e)
    {
        var warning = HandleExistingProcessWindow(thisProcess, e);

        if (warning)
        {
            Dialog.Ok(
                LocalizationHelper.Get("S.Warning.Single.Title"), 
                LocalizationHelper.Get("S.Warning.Single.Header"), 
                LocalizationHelper.Get("S.Warning.Single.Message"), 
                Icons.Info
            );
        }

        Environment.Exit(0);
    }

    private bool HandleExistingProcessWindow(Process thisProcess, StartupEventArgs e)
    {
        var warning = true;
        var process = Process.GetProcessesByName(thisProcess.ProcessName)
            .FirstOrDefault(p => p.MainWindowHandle != thisProcess.MainWindowHandle);

        if (process != null)
        {
            var handles = Native.Helpers.Windows.GetWindowHandlesFromProcess(process);
            var windowHandle = handles.Count > 0 ? handles[0] : process.Handle;

            Native.External.User32.ShowWindow(windowHandle, Domain.Enums.Native.ShowWindowCommands.Show);
            Native.External.User32.SetForegroundWindow(windowHandle);
            
            InstanceSwitcherChannel.SendMessage(process.Id, e.Args);
            warning = false;
        }

        return warning;
    }

    private void SetupBackgroundTasks()
    {
        Task.Factory.StartNew(MainViewModel.ClearTemporaryFiles, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(async () => await MainViewModel.CheckForUpdates(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(MainViewModel.SendFeedback, TaskCreationOptions.LongRunning);
    }

    private void HandleStartupAction()
    {
        if (Arguments.Open)
            MainViewModel.Open.Execute(Arguments.WindownToOpen, true);
        else
            MainViewModel.Open.Execute(UserSettings.All.StartUp);
    }
    #endregion

    // Other methods remain mostly the same...

    public void Dispose()
    {
        ReleaseResources();
    }

    private void ReleaseResources()
    {
        if (_singleInstanceMutex != null && _mutexAcquired)
        {
            _singleInstanceMutex.ReleaseMutex();
            _mutexAcquired = false;
        }

        _singleInstanceMutex?.Dispose();
    }
}