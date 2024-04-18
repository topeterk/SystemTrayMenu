// <copyright file="AppNotifyIcon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if !AVALONIA
namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Drawing;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Threading;
    using H.NotifyIcon.Core;
    using SystemTrayMenu.Utilities;

    [SupportedOSPlatform("windows5.1.2600")]
    internal class AppNotifyIcon : IDisposable
    {
        private readonly Dispatcher dispatchter = WPFExtensions.CurrentDispatcher;
        private readonly TrayIconWithContextMenu notifyIcon = new ();
        private Icon? loadingIcon;

        public AppNotifyIcon()
        {
            notifyIcon.ToolTip = "SystemTrayMenu";
            notifyIcon.Icon = Config.GetCustomAppIcon().Handle;
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
            // WPF/H.NotifyIcon Workaround: On application reset we get stuck here, so do this only on regular shutdown
            if (Application.Current is null)
            {
                notifyIcon.Dispose();
            }

            loadingIcon?.Dispose();
        }

        public void LoadingStart()
        {
            loadingIcon ??= App.LoadIconFromResource("Resources/Loading.ico");
            notifyIcon.UpdateIcon(loadingIcon.Handle);
        }

        public void LoadingStop()
        {
            notifyIcon.UpdateIcon(Config.GetCustomAppIcon().Handle);
        }
    }
}
#endif
