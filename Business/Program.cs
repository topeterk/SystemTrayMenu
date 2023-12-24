// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if !AVALONIA || !DEBUG
#define GLOBAL_TRY_CATCH
#endif

namespace SystemTrayMenu
{
    using System;
    using System.Reflection;
    using System.Threading;
#if WINDOWS
    using System.Windows;
#endif
#if AVALONIA
    using Avalonia;
    using Avalonia.ReactiveUI;
#endif
    using SystemTrayMenu.Utilities;

    internal static class Program
    {
#if GLOBAL_TRY_CATCH
        private static bool isStartup = true;
#endif

        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "MainThread";
#if !WINDOWS
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
#endif

#if GLOBAL_TRY_CATCH
            try
#endif
            {
                Log.Initialize();
#if WINDOWS
                PrivilegeChecker.Initialize();
#endif
                Config.Initialize();
                Translator.Initialize();

                if (SingleAppInstance.Initialize())
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
#if GLOBAL_TRY_CATCH
                    currentDomain.UnhandledException += (sender, args)
                        => AskUserSendError((Exception)args.ExceptionObject);
#endif
                    Scaling.Initialize();
                    FolderOptions.Initialize();

                    Config.ParseCommandline(args);
#if !AVALONIA
                    using App app = new ();
#if GLOBAL_TRY_CATCH
                    isStartup = false;
#endif
                    Log.WriteApplicationRuns();
                    app.Run();
#else
                    // Initialization code. Don't use any Avalonia, third-party APIs or any
                    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
                    // yet and stuff might break.
                    AppBuilder app = AppBuilder.Configure<App>();
#if GLOBAL_TRY_CATCH
                    isStartup = false;
#endif
                    app.UsePlatformDetect();
                    app.LogToTrace();
                    app.UseReactiveUI();
                    Log.WriteApplicationRuns();
                    app.StartWithClassicDesktopLifetime(args);
#endif
                }
            }
#if GLOBAL_TRY_CATCH
            catch (Exception ex)
            {
                AskUserSendError(ex);
            }
            finally
#endif
            {
                SingleAppInstance.Unload();
                Log.Close();
            }

#if GLOBAL_TRY_CATCH
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
#endif
        }
    }
}
