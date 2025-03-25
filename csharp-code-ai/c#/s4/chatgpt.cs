public partial class App : IDisposable
{
    #region Properties

    internal static NotifyIcon NotifyIcon { get; private set; }
    internal static ApplicationViewModel MainViewModel { get; private set; }

    private Mutex _mutex;
    private bool _accepted;
    private readonly List<Exception> _exceptionList = new();
    private readonly object _lock = new();

    #endregion

    #region Events

    private void App_Startup(object sender, StartupEventArgs e)
    {
        Global.StartupDateTime = DateTime.Now;

        RegisterGlobalExceptionHandlers();
        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

        SetSecurityProtocol();
        Arguments.Prepare(e.Args);
        LocalizationHelper.SelectCulture(UserSettings.All.LanguageCode);
        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        if (HandleSpecialModes(e.Args)) return;
        if (UserSettings.All.SingleInstance && !Arguments.NewInstance && !EnsureSingleInstance(e.Args)) return;

        RenderOptions.ProcessRenderMode = UserSettings.All.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;

        SetWorkaroundForDispatcher();
        InitializeTrayIconAndViewModel();
        LaunchBackgroundTasks();

        if (Arguments.Open)
            MainViewModel.Open.Execute(Arguments.WindownToOpen, true);
        else
            MainViewModel.Open.Execute(UserSettings.All.StartUp);
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

        SafeExecute(MutexList.RemoveAll, "remove all mutexes");
        SafeExecute(() => NotifyIcon?.Dispose(), "dispose the system tray icon");
        SafeExecute(EncodingManager.StopAllEncodings, "cancel all encodings");
        SafeExecute(SettingsExtension.ForceSave, "save the user settings");
        SafeExecute(() => _mutex?.ReleaseMutex(), "release the single instance mutex");
        SafeExecute(() => HotKeyCollection.Default.Dispose(), "dispose the hotkeys");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogWriter.Log(e.Exception, "Unhandled Dispatcher Exception");

        try
        {
            ShowException(e.Exception);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Error showing dispatcher exception.");
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception)
            return;

        LogWriter.Log(exception, "Unhandled Domain Exception");

        try
        {
            ShowException(exception);
        }
        catch { }
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
            return;

        ThemeHelper.SelectTheme(UserSettings.All.MainTheme);

        if (UserSettings.All.GridColorsFollowSystem)
        {
            var isDark = ThemeHelper.IsSystemUsingDarkTheme();
            UserSettings.All.GridColor1 = isDark ? Constants.DarkEven : Constants.VeryLightEven;
            UserSettings.All.GridColor2 = isDark ? Constants.DarkOdd : Constants.VeryLightOdd;
        }
    }

    #endregion

    #region Initialization Methods

    private void RegisterGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private bool HandleSpecialModes(string[] args)
    {
        if (Arguments.IsInDownloadMode)
        {
            new Downloader
            {
                DownloadMode = Arguments.DownloadMode,
                DestinationPath = Arguments.DownloadPath
            }.ShowDialog();

            Environment.Exit(90);
            return true;
        }

        if (Arguments.IsInSettingsMode)
        {
            SettingsPersistenceChannel.RegisterServer();
            return true;
        }

        return false;
    }

    private bool EnsureSingleInstance(string[] args)
    {
        try
        {
            using var thisProcess = Process.GetCurrentProcess();
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
            var filePath = thisProcess.MainModule?.FileName ?? Assembly.GetEntryAssembly()?.Location ?? "ScreenToGif";
            var mutexName = $"{user?.Value ?? Environment.UserName}_{Convert.ToBase64String(Encoding.UTF8.GetBytes(filePath))}";

            _mutex = new Mutex(true, mutexName, out _accepted);

            if (!_accepted)
            {
                if (!TrySwitchToExistingInstance(thisProcess, args))
                    ShowSingleInstanceWarning();

                Environment.Exit(0);
                return false;
            }

            InstanceSwitcherChannel.RegisterServer(InstanceSwitch_Received);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to enforce single instance.");
        }

        return true;
    }

    private bool TrySwitchToExistingInstance(Process thisProcess, string[] args)
    {
        var existing = Process.GetProcessesByName(thisProcess.ProcessName)
                              .FirstOrDefault(p => p.MainWindowHandle != thisProcess.MainWindowHandle);

        if (existing != null)
        {
            var handles = Native.Helpers.Windows.GetWindowHandlesFromProcess(existing);

            var handle = handles.Count > 0 ? handles[0] : existing.Handle;
            Native.External.User32.ShowWindow(handle, ShowWindowCommands.Show);
            Native.External.User32.SetForegroundWindow(handle);

            InstanceSwitcherChannel.SendMessage(existing.Id, args);
            return true;
        }

        return false;
    }

    private void ShowSingleInstanceWarning()
    {
        Dialog.Ok(
            LocalizationHelper.Get("S.Warning.Single.Title"),
            LocalizationHelper.Get("S.Warning.Single.Header"),
            LocalizationHelper.Get("S.Warning.Single.Message"),
            Icons.Info);
    }

    private void InitializeTrayIconAndViewModel()
    {
        NotifyIcon = (NotifyIcon)FindResource("NotifyIcon");

        if (NotifyIcon != null)
        {
            NotifyIcon.Visibility = UserSettings.All.ShowNotificationIcon || UserSettings.All.StartMinimized || UserSettings.All.StartUp == 5
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (UserSettings.All.StartUp == 5)
            {
                UserSettings.All.StartMinimized = true;
                UserSettings.All.ShowNotificationIcon = true;
                UserSettings.All.StartUp = 0;
            }
        }

        MainViewModel = (ApplicationViewModel)FindResource("AppViewModel") ?? new ApplicationViewModel();
        RegisterShortcuts();
    }

    private void LaunchBackgroundTasks()
    {
        Task.Factory.StartNew(MainViewModel.ClearTemporaryFiles, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(async () => await MainViewModel.CheckForUpdates(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(MainViewModel.SendFeedback, TaskCreationOptions.LongRunning);
    }

    #endregion

    #region Utility Methods

    private void SetSecurityProtocol()
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to set network security protocol.");
        }
    }

    private void SetWorkaroundForDispatcher()
    {
        try
        {
            if (UserSettings.All.WorkaroundQuota)
                BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure = BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Reset;

#if DEBUG
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
            BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure = BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Throw;
#endif
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Failed to apply dispatcher workaround.");
        }
    }

    internal static void RegisterShortcuts()
    {
        static bool Register(Func<bool> action) => action();

        var screen = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.RecorderModifiers, UserSettings.All.RecorderShortcut,
            () => ExecuteIfAllowed(MainViewModel.OpenRecorder)));

        var webcam = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.WebcamRecorderModifiers, UserSettings.All.WebcamRecorderShortcut,
            () => ExecuteIfAllowed(MainViewModel.OpenWebcamRecorder)));

        var board = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.BoardRecorderModifiers, UserSettings.All.BoardRecorderShortcut,
            () => ExecuteIfAllowed(MainViewModel.OpenBoardRecorder)));

        var editor = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.EditorModifiers, UserSettings.All.EditorShortcut,
            () => ExecuteIfAllowed(MainViewModel.OpenEditor)));

        var options = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.OptionsModifiers, UserSettings.All.OptionsShortcut,
            () => ExecuteIfAllowed(MainViewModel.OpenOptions)));

        var exit = Register(() => HotKeyCollection.Default.TryRegisterHotKey(UserSettings.All.ExitModifiers, UserSettings.All.ExitShortcut,
            () => ExecuteIfAllowed(MainViewModel.ExitApplication)));

        MainViewModel.RecorderGesture = screen ? GetGesture(UserSettings.All.RecorderShortcut, UserSettings.All.RecorderModifiers) : "";
        MainViewModel.WebcamRecorderGesture = webcam ? GetGesture(UserSettings.All.WebcamRecorderShortcut, UserSettings.All.WebcamRecorderModifiers) : "";
        MainViewModel.BoardRecorderGesture = board ? GetGesture(UserSettings.All.BoardRecorderShortcut, UserSettings.All.BoardRecorderModifiers) : "";
        MainViewModel.EditorGesture = editor ? GetGesture(UserSettings.All.EditorShortcut, UserSettings.All.EditorModifiers) : "";
        MainViewModel.OptionsGesture = options ? GetGesture(UserSettings.All.OptionsShortcut, UserSettings.All.OptionsModifiers) : "";
        MainViewModel.ExitGesture = exit ? GetGesture(UserSettings.All.ExitShortcut, UserSettings.All.ExitModifiers) : "";
    }

    private static void ExecuteIfAllowed(ICommand command)
    {
        if (!Global.IgnoreHotKeys && command.CanExecute(null))
            command.Execute(null);
    }

    private static string GetGesture(Key key, ModifierKeys modifiers)
    {
        return Native.Helpers.Other.GetSelectKeyText(key, modifiers, true, true);
    }

    private void ShowException(Exception exception)
    {
        lock (_lock)
        {
            if (_exceptionList.Any(e => e.Message == exception.Message))
                return;

            _exceptionList.Add(exception);

            Current.Dispatcher.Invoke(() =>
            {
                if (Global.IsHotFix4055002Installed && exception is XamlParseException && exception.InnerException is TargetInvocationException)
                    ExceptionDialog.Ok(exception, "ScreenToGif", "Error while rendering visuals", exception.Message);
                else
                    ExceptionDialog.Ok(exception, "ScreenToGif", "Unhandled exception", exception.Message);
            });

            _exceptionList.Remove(exception);
        }
    }

    private void SafeExecute(Action action, string context)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, $"Failed to {context}.");
        }
    }

    public void Dispose()
    {
        if (_mutex != null && _accepted)
        {
            _mutex.ReleaseMutex();
            _accepted = false;
        }

        _mutex?.Dispose();
    }

    #endregion

    #region IPC Callback

    internal static void InstanceSwitch_Received(object _, InstanceSwitcherMessage message)
    {
        try
        {
            Arguments.Prepare(message.Args);

            if (Arguments.Open)
                MainViewModel.Open.Execute(Arguments.WindownToOpen, true);
            else
                MainViewModel.Open.Execute(UserSettings.All.StartUp);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "IPC argument execution failed.");
        }
    }

    #endregion
}
