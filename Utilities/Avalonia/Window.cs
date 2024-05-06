// <copyright file="Window.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Threading.Tasks;
    using Avalonia.Input;
    using Avalonia.Media;
    using Avalonia.Threading;

    internal enum Visibility : byte
    {
        Visible = 0,
        Hidden = 1,
        Collapsed = 2,
    }

    public class Window : Avalonia.Controls.Window
    {
        public Window()
        {
            RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias); // = ClearType

            Opened += (sender, e) => ContentRendered?.Invoke(sender, e);
        }

        internal event EventHandler? ContentRendered;

        internal event EventHandler<PointerEventArgs>? MouseEnter
        {
            add { AddHandler(PointerEnteredEvent, value); }
            remove { RemoveHandler(PointerEnteredEvent, value); }
        }

        internal event EventHandler<PointerEventArgs>? MouseLeave
        {
            add { AddHandler(PointerExitedEvent, value); }
            remove { RemoveHandler(PointerExitedEvent, value); }
        }

        internal event EventHandler<PointerEventArgs>? MouseMove
        {
            add { AddHandler(PointerMovedEvent, value); }
            remove { RemoveHandler(PointerMovedEvent, value); }
        }

        internal event EventHandler<PointerPressedEventArgs>? MouseDown
        {
            add { AddHandler(PointerPressedEvent, value); }
            remove { RemoveHandler(PointerPressedEvent, value); }
        }

        internal event EventHandler<PointerReleasedEventArgs>? MouseUp
        {
            add { AddHandler(PointerReleasedEvent, value); }
            remove { RemoveHandler(PointerReleasedEvent, value); }
        }

        internal Dispatcher Dispatcher => Dispatcher.UIThread;

        internal bool? ShowDialog()
        {
            Task<bool?> dialog = ShowDialog<bool?>(this);
            dialog.Wait(); // TODO: Most likely we get stuck here as we have to await?
            return dialog.Result;
        }
    }
}
#endif
