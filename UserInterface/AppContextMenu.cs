// <copyright file="AppContextMenu.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if !AVALONIA
namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Threading;
    using H.NotifyIcon.Core;
    using SystemTrayMenu.Helpers.Updater;
    using SystemTrayMenu.Utilities;

    [SupportedOSPlatform("windows5.0")]
    internal class AppContextMenu
    {
        public PopupMenu Create()
        {
            PopupMenu menu = new();

            AddItem(menu, "Settings", () => SettingsWindow.ShowSingleInstance());
            AddSeperator(menu);
            AddItem(menu, "Log File", Log.OpenLogFile);
            AddSeperator(menu);
            AddItem(menu, "Frequently Asked Questions", Config.ShowHelpFAQ);
            AddItem(menu, "Support SystemTrayMenu", Config.ShowSupportSystemTrayMenu);
            AddItem(menu, "About SystemTrayMenu", AboutBox.ShowSingleInstance);
            AddItem(menu, "Check for updates", () => GitHubUpdate.ActivateNewVersionFormOrCheckForUpdates(showWhenUpToDate: true));
            AddSeperator(menu);
            AddItem(menu, "Restart", AppRestart.ByAppContextMenu);
            AddItem(menu, "Exit app", () => Application.Current.Shutdown());

            return menu;
        }

        private static void AddSeperator(PopupMenu menu)
        {
            menu.Items.Add(new PopupMenuSeparator());
        }

        private static void AddItem(
                PopupMenu menu,
                string text,
                Action actionClick)
        {
            Dispatcher dispatcher = WPFExtensions.CurrentDispatcher;
            menu.Items.Add(new PopupMenuItem(
                text, new ((_, _) => dispatcher.Invoke(actionClick))));
        }
    }
}
#endif
