// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
    using System.Reflection;
#if WINDOWS
    using System.Windows;
#else
    using Avalonia;
    using Avalonia.ReactiveUI;
#endif
    using SystemTrayMenu.Utilities;

    internal static class Program
    {
        private static bool isStartup = true;

        [STAThread]
        private static void Main(string[] args)
        {
#if !REMOTE_DEBBUGING_STARTUP_BREAK
#if WAIT_FOREVER
            bool waiting = true;
            while (waiting)
            {
                System.Threading.Thread.Sleep(100);
            }
#else
            System.Threading.Thread.Sleep(6000);
#endif
#endif

            try
            {
                Log.Initialize();
                Translator.Initialize();
                Config.SetFolderByWindowsContextMenu(args);
                Config.LoadOrSetByUser();
                Config.Initialize();
#if WINDOWS
                PrivilegeChecker.Initialize();
#endif

                // Without a valid path we cannot do anything, just close application
                if (string.IsNullOrEmpty(Config.Path))
                {
                    MessageBox.Show(
                        Translator.GetText("Your root directory for the app does not exist or is empty! Change the root directory or put some files, directories or shortcuts into the root directory."),
                        "SystemTrayMenu",
                        MessageBoxButton.OK);
                    return;
                }

                if (SingleAppInstance.Initialize())
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.UnhandledException += (sender, args)
                        => AskUserSendError((Exception)args.ExceptionObject);

                    Scaling.Initialize();
                    FolderOptions.Initialize();
#if WINDOWS
                    using App app = new ();
                    isStartup = false;
                    Log.WriteApplicationRuns();
                    app.Run();
#else
                    // Initialization code. Don't use any Avalonia, third-party APIs or any
                    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
                    // yet and stuff might break.
                    AppBuilder app = AppBuilder.Configure<App>();
                    isStartup = false;
                    app.UsePlatformDetect();
                    app.LogToTrace();
                    app.UseReactiveUI();
                    Log.WriteApplicationRuns();
                    app.StartWithClassicDesktopLifetime(args);
#endif
                }
            }
            catch (Exception ex)
            {
                AskUserSendError(ex);
            }
            finally
            {
                SingleAppInstance.Unload();
                Log.Close();
            }

            static void AskUserSendError(Exception ex)
            {
                Log.Error("Application Crashed", ex);

                MessageBoxResult dialogResult = MessageBox.Show(
                    "A problem has been encountered and SystemTrayMenu needs to restart. " +
                    "Reporting this error will help us making our product better." +
                    Environment.NewLine + Environment.NewLine +
                    "We kindly ask you to press 'Yes' and send us the crash report before restarting the application. " +
                    "This will open your standard email app and prepare a mail that you can directly send off. " +
                    "Alternatively, you can also create an issue manually here: https://github.com/Hofknecht/SystemTrayMenu/issues" +
                    Environment.NewLine + Environment.NewLine +
                    "Pressing 'No' will only restart the application." +
                    Environment.NewLine +
                    "Pressing 'Cancel' will quit the application.",
                    "SystemTrayMenu Crashed",
                    MessageBoxButton.YesNoCancel);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
                    Log.ProcessStart("mailto:" + "markus@hofknecht.eu" +
                        "?subject=SystemTrayMenu Bug reported " +
                        (version != null ? version.ToString() : string.Empty) +
                        "&body=" + ex.ToString());
                }

                if (!isStartup && dialogResult != MessageBoxResult.Cancel)
                {
                    AppRestart.ByThreadException();
                }
            }
        }
    }
}
