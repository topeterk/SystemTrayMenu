// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
    using System.IO;
#if !AVALONIA
    using System.Drawing;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Threading;
#else
    using System.Reflection;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using Avalonia.Platform;
    using Avalonia.Styling;
    using Avalonia.Threading;
    using SystemTrayMenu.UserInterface;
#endif
    using SystemTrayMenu.Business;
    using SystemTrayMenu.DllImports;
    using SystemTrayMenu.Helpers;
    using SystemTrayMenu.Helpers.Updater;
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.Utilities;

    /// <summary>
    /// App contains the notifyicon, the taskbarform and the menus.
    /// </summary>
    public partial class App : Application, IDisposable
    {
#if AVALONIA
        private static IClassicDesktopStyleApplicationLifetime? desktopLifetime;
#endif

        private Menus? menus;
        private JoystickHelper? joystickHelper;
#if AVALONIA
        private TrayIcon? trayIcon;
        private IDisposable? updateCheckTimer;
#endif
        private bool isDisposed;

        public App()
        {
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
            AppColors.Initialize(true);

#if !AVALONIA
            InitializeComponent();
#endif

            AppRestart.BeforeRestarting += Dispose;
#if !AVALONIA
            Activated += (_, _) => IsActiveApp = true;
            Deactivated += (_, _) => IsActiveApp = false;
            Startup += AppStartupHandler;
#endif
        }

#if AVALONIA
        internal static bool IsAppLoading
        {
            set
            {
                App app = (App)Current;
                if (value)
                {
                    app.trayIcon.Icon = (WindowIcon)app.Resources["ApplicationTrayIconLoading"];
                }
                else
                {
                    app.trayIcon.Icon = Config.GetCustomAppIcon() ?? (WindowIcon)app.Resources["ApplicationTrayIcon"];
                }
            }
        }
#endif

#if AVALONIA
        internal static bool IsActiveApp
        {
            get
            {
                if (desktopLifetime is not null)
                {
                    foreach (var window in desktopLifetime.Windows)
                    {
                        if (window.IsActive && window is not TaskbarLogo)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
#else
        internal static bool IsActiveApp { get; private set; }
#endif

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if AVALONIA
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            TrayIcons? trayIcons = TrayIcon.GetIcons(this);
            if (trayIcons is not null && trayIcons.Count > 0)
            {
                trayIcon = trayIcons[0]; // Remember first (and only) one
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            RequestedThemeVariant = Config.IsDarkMode() ? ThemeVariant.Dark : ThemeVariant.Light;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktopLifetime = desktop;

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.Exit += AppExitHandler;

                AppStartupHandler(this, desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }
#endif

        /// <summary>
        /// Loads an Icon from the application's Resources.
        /// Note: Only allowed to be called after App's Startup event.
        /// </summary>
        /// <param name="resourceName">Absolute file path from root directory.</param>
        /// <returns>New Icon object.</returns>
#if AVALONIA
        internal static WindowIcon LoadIconFromResource(string resourceName)
#else
        [SupportedOSPlatform("windows")]
        internal static Icon LoadIconFromResource(string resourceName)
#endif
        {
#if !AVALONIA
            using (Stream stream = GetResourceStream(new("pack://application:,,,/" + resourceName, UriKind.Absolute)).Stream)
#else
            using (Stream stream = AssetLoader.Open(new Uri($"avares://{Assembly.GetEntryAssembly()!.GetName().Name!}{"/" + resourceName}")))
#endif
            {
                return new(stream);
            }
        }

#if AVALONIA
        internal void TrayMenu_Clicked(object sender, EventArgs args) => menus?.UserSwitchOpenClose(true);

        internal void TrayMenu_OpenSettings(object sender, EventArgs args) => SettingsWindow.ShowSingleInstance();

        internal void TrayMenu_OpenLog(object sender, EventArgs args) => Log.OpenLogFile();

        internal void TrayMenu_OpenFAQ(object sender, EventArgs args) => Config.ShowHelpFAQ();

        internal void TrayMenu_OpenSupport(object sender, EventArgs args) => Config.ShowSupportSystemTrayMenu();

        internal void TrayMenu_OpenAbout(object sender, EventArgs args) => AboutBox.ShowSingleInstance();

        internal void TrayMenu_CheckUpdates(object sender, EventArgs args) => GitHubUpdate.ActivateNewVersionFormOrCheckForUpdates(showWhenUpToDate: true);

        internal void TrayMenu_Restart(object sender, EventArgs args) => AppRestart.ByAppContextMenu();

        internal void TrayMenu_Exit(object sender, EventArgs args) => this.Shutdown();
#endif

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
#if AVALONIA
                updateCheckTimer?.Dispose();
#endif
                IconReader.Shutdown();

                if (joystickHelper != null)
                {
                    if (menus != null)
                    {
                        joystickHelper.KeyPressed -= menus.KeyPressed;
                    }

                    joystickHelper.Dispose();
                }

                menus?.Dispose();

                isDisposed = true;
            }
        }

        private void AppStartupHandler(object sender, object e)
        {
#if AVALONIA
            // IcoWidth 100% = 21px, 175% is 33
            double icoWidth = 16 * Scaling.FactorByDpi;
            double factorIconSizeInPercent = Settings.Default.IconSizeInPercent / 100D;
            Resources["ColumnIconWidth"] = Math.Ceiling(icoWidth * factorIconSizeInPercent * Scaling.Factor);

            double factor;
            if (NativeMethods.IsTouchEnabled())
            {
                factor = Settings.Default.RowHeighteInPercentageTouch / 100f;
            }
            else
            {
                factor = Settings.Default.RowHeighteInPercentage / 100D;
            }

            double rowHeightDefault = 21.24d * Scaling.FactorByDpi;
            Resources["RowHeight"] = Math.Round(rowHeightDefault * factor * Scaling.Factor);
#endif

            IconReader.Startup();

            menus = new();
            menus.Startup();

            if (Settings.Default.SupportGamepad)
            {
                joystickHelper = new();
                joystickHelper.KeyPressed += menus.KeyPressed;
            }

            if (Settings.Default.CheckForUpdates)
            {
#if !AVALONIA
                _ = Dispatcher.InvokeAsync(
                    () => GitHubUpdate.ActivateNewVersionFormOrCheckForUpdates(showWhenUpToDate: false),
                    DispatcherPriority.ApplicationIdle);
#else
                // Run check later in the background, so offline startup can proceed faster
                updateCheckTimer = DispatcherTimer.RunOnce(
                    () => Dispatcher.UIThread.Post(() => GitHubUpdate.ActivateNewVersionFormOrCheckForUpdates(showWhenUpToDate: false)),
                    new(0),
                    DispatcherPriority.ApplicationIdle);
#endif
            }
        }

#if AVALONIA
        private void AppExitHandler(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            desktopLifetime = null;
            Dispose();
        }
#endif
    }
}
