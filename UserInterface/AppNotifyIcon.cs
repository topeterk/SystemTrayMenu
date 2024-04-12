// <copyright file="AppNotifyIcon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
#if !AVALONIA
    using System.Drawing;
    using System.Windows.Threading;
    using H.NotifyIcon.Core;
#else
    using Avalonia.Controls;
    using Avalonia.Threading;
#endif
    using SystemTrayMenu.Utilities;

    internal class AppNotifyIcon : IDisposable
    {
        private readonly Dispatcher dispatchter = WPFExtensions.CurrentDispatcher;
        private readonly TrayIconWithContextMenu notifyIcon = new ();
#if AVALONIA
        private WindowIcon? loadingIcon;
#else
        private Icon? loadingIcon;
#endif

        public AppNotifyIcon()
        {
            notifyIcon.ToolTip = "SystemTrayMenu";

#if AVALONIA
            notifyIcon.Icon = Config.GetAppIcon();
#else
            notifyIcon.Icon = Config.GetAppIcon().Handle;
#endif
            notifyIcon.ContextMenu = new AppContextMenu().Create();
            notifyIcon.MessageWindow.MouseEventReceived += (sender, e) =>
            {
                if (e.MouseEvent == MouseEvent.IconLeftMouseUp ||
                    e.MouseEvent == MouseEvent.IconLeftDoubleClick)
                {
                    dispatchter.Invoke(() => Click?.Invoke());
                }
            };
            notifyIcon.Create();
        }

        public event Action? Click;

        public void Dispose()
        {
            notifyIcon.Dispose();
#if !AVALONIA
            loadingIcon?.Dispose();
#endif
        }

        public void LoadingStart()
        {
            loadingIcon ??= App.LoadIconFromResource("Resources/Loading.ico");
#if AVALONIA
            notifyIcon.Icon = loadingIcon; // TODO: Loading/Unloading icons will not update the icon, maybe try using bindings?

            // TODO: Think about XAML tray icon: https://docs.avaloniaui.net/docs/reference/controls/detailed-reference/tray-icon
#else
            notifyIcon.UpdateIcon(loadingIcon.Handle);
#endif
        }

        public void LoadingStop()
        {
#if AVALONIA
            notifyIcon.Icon = Config.GetAppIcon();
#else
            notifyIcon.UpdateIcon(Config.GetAppIcon().Handle);
#endif
        }
    }
}