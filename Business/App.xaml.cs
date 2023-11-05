// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
    using System.Drawing;
    using System.IO;
#if !AVALONIA
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
    using Avalonia.Themes.Fluent;
    using Avalonia.Threading;
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
        private Menus? menus;
        private JoystickHelper? joystickHelper;
#if AVALONIA
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
#if TODO_AVALONIA
            Activated += (_, _) => IsActiveApp = true;
            Deactivated += (_, _) => IsActiveApp = false;
#endif
#if !AVALONIA
            Startup += AppStartupHandler;
#endif
        }

        internal static bool IsActiveApp { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if AVALONIA
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (IStyle style in Styles)
                {
                    if (style is FluentTheme theme)
                    {
                        // TODO: theme.Mode = Settings.DarkMode ? FluentThemeMode.Dark : FluentThemeMode.Light;
                    }
                }

                UserInterface.TaskbarLogo main = new (); // TODO

                desktop.MainWindow = main;
                NativeMethods.Screen.DesktopScreens = desktop.MainWindow.Screens;
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
        internal static Icon LoadIconFromResource(string resourceName)
        {
#if !AVALONIA
            using (Stream stream = GetResourceStream(new("pack://application:,,,/" + resourceName, UriKind.Absolute)).Stream)
#else
            using (Stream stream = AvaloniaLocator.Current.GetService<IAssetLoader>()!.Open(
                new Uri($"avares://{Assembly.GetEntryAssembly()!.GetName().Name!}{"/" + resourceName}")))
#endif
            {
                return new(stream);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                updateCheckTimer?.Dispose();
            }
        }
#endif
    }
}
