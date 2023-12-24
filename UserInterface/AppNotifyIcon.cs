﻿// <copyright file="AppNotifyIcon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Drawing;
#if !AVALONIA
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
        private Icon? loadingIcon;

        public AppNotifyIcon()
        {
            notifyIcon.ToolTip = "SystemTrayMenu";
#if TODO_AVALONIA
            notifyIcon.Icon = Config.GetAppIcon().Handle;
#else
            // Icon (more specific: System.Drawing) is no longer available, so it must be replaced
            // As quick fix just load the static Icon here only.
            notifyIcon.Icon = new WindowIcon(new LocalResourceBitmap("/Resources/SystemTrayMenu.ico"));
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
            loadingIcon?.Dispose();
        }

        public void LoadingStart()
        {
            loadingIcon ??= App.LoadIconFromResource("Resources/Loading.ico");
            notifyIcon.UpdateIcon(loadingIcon.Handle);
        }

        public void LoadingStop()
        {
            notifyIcon.UpdateIcon(Config.GetAppIcon().Handle);
        }
    }
}