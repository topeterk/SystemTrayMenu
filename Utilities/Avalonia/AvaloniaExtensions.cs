﻿// <copyright file="AvaloniaExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// ===============================================================================
// MIT License
//
// Copyright (c) 2021-2023 Peter Kirmeier
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable SA1402 // File may only contain a single type

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Reflection;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Platform;
#if !TODO_LINUX
    using Avalonia.Threading;
#endif

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    internal static class AvaloniaExtensions
    {
        internal static void Shutdown(this Application application)
        {
            ((IClassicDesktopStyleApplicationLifetime?)application.ApplicationLifetime)?.Shutdown();
        }

        internal static object GetData(this IDataObject obj, string dataFormat) => obj.Get(dataFormat) ?? new (); // TODO

        internal static void Offset(this Point point, double x, double y) => point += new Vector(x, y);
    }

    /// <summary>
    /// Loads an image (Bitmap) from local resources (avares://).
    /// </summary>
    internal class LocalResourceBitmap : BitmapSource
    {
        public LocalResourceBitmap(string path)
            : base(
            AvaloniaLocator.Current.GetService<IAssetLoader>()!.Open(
                new Uri($"avares://{Assembly.GetEntryAssembly()!.GetName().Name!}{path}")))
        {
        }
    }

#pragma warning disable SA1201 // ElementsMustAppearInTheCorrectOrder
    internal enum MouseEvent
    {
        IconLeftMouseUp,
        IconLeftDoubleClick,
    }
#pragma warning restore SA1201 // ElementsMustAppearInTheCorrectOrder

    internal class PopupMenu : NativeMenu
    {
    }

    internal class PopupMenuItem : NativeMenuItem
    {
        public PopupMenuItem(string text, EventHandler<EventArgs> onClick)
            : base(text)
        {
            Command = new ActionCommand((_) => onClick(null, new ()));
        }
    }

    internal class PopupMenuSeparator : NativeMenuItemSeparator
    {
    }

    internal class MouseEventReceivedEventArgs
    {
#pragma warning disable SA1401 // FieldShouldBePrivate
        internal MouseEvent MouseEvent;
#pragma warning restore SA1401 // FieldShouldBePrivate

        public MouseEventReceivedEventArgs(MouseEvent mouseEvent)
        {
            MouseEvent = mouseEvent;
        }
    }

    internal class MessageWindow
    {
        internal event EventHandler<MouseEventReceivedEventArgs>? MouseEventReceived;

        internal void RaiseEvent(object? sender, EventArgs e)
            => MouseEventReceived?.Invoke(sender, new(MouseEvent.IconLeftMouseUp));
    }

    internal class TrayIconWithContextMenu
    {
        private static MessageWindow msgWindow = new();
        private static TrayIcon? trayIcon;

        internal string ToolTip { get; set; } = string.Empty;

        internal WindowIcon Icon { get; set; }

        internal PopupMenu? ContextMenu { get; set; }

        internal MessageWindow MessageWindow => msgWindow;

        internal void Create()
        {
            trayIcon = new();
            trayIcon.Icon = Icon;
            trayIcon.ToolTipText = ToolTip;
            trayIcon.Menu = ContextMenu;
            trayIcon.Clicked += MessageWindow.RaiseEvent;
#if !TODO_LINUX
            // It seems that on Ubuntu 22.04 the Clicked event on the icon does not fire?
            // Find a workaround or fix! As a workaround add menu item to open the main menu instead:
            Dispatcher dispatcher = WPFExtensions.CurrentDispatcher;
            ContextMenu.Items.Insert(0, new PopupMenuItem(
                Translator.GetText("Open/Close Menu"), new((sender, e) => dispatcher.Invoke(() => MessageWindow.RaiseEvent(sender, e)))));
#endif
        }

        internal void Dispose()
        {
            trayIcon?.Dispose();
            trayIcon = null;
        }

        internal void UpdateIcon(nint handle)
        {
            // TODO: Avalonia icon is static as of now
        }
    }

#if TODO_AVALONIA // TODO: Delete?
    /// <summary>
    /// Should be replaced with DynamicResource.
    /// Workaround: Avalonia DataTriggerBehavior does not always work when page is loaded.
    ///             This class seems to work as kind of a proxy.
    ///             During first pass the DataTriggerBehavior is created and this object is instantiated,
    ///             however, the trigger will somehow not be fired.
    ///             During second pass the markup extension (ProvideValue) gets executed,
    ///             but this time the trigger was/gets fired as well.
    /// See: https://github.com/wieslawsoltes/AvaloniaBehaviors/issues/56
    /// See: https://stackoverflow.com/questions/68979876/how-to-simulate-datatrigger-with-avalonia .
    /// </summary>
    internal class LazyResource : MarkupExtension
    {
        internal LazyResource(string key) => Key = key;

        private string Key { get; init; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Application.Current?.Resources.ContainsKey(Key) ?? false)
            {
                return Application.Current.Resources[Key]!;
            }

            throw
        }
    }
#endif

    // TODO TODO TODO:
#pragma warning disable SA1201 // ElementsMustAppearInTheCorrectOrder
    internal enum MouseButtonState
    {
        Released = 0,
        Pressed = 1,
    }

    internal class MouseButtonEventArgs
    {
        internal bool Handled { get; set; }

        internal MouseButtonState LeftButton { get; }
    }

    internal class TextCompositionEventArgs
    {
        internal bool Handled { get; set; }

        internal string Text => "TODO: TextCompositionEventArgs.Text";
    }

    internal class StartupTaskState
    {
    }
#pragma warning restore SA1201 // ElementsMustAppearInTheCorrectOrder
}
#endif
