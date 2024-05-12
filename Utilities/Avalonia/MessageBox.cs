// <copyright file="MessageBox.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2024 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using System;

    internal enum MessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }

    internal enum MessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
    }

    internal enum MessageBoxImage
    {
#if TODO_REMOVE
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
#endif
        Warning = 48,
#if TODO_REMOVE
        Asterisk = 64,
        Information = 64,
#endif
    }

    internal class MessageBox
    {
        internal static MessageBoxResult Show(string messageBoxText) => DoLogOnly(messageBoxText);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060", Justification = "Unused parameter kept for API Compatibility")]
        internal static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button) => DoLogOnly(messageBoxText, caption);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060", Justification = "Unused parameter kept for API Compatibility")]
        internal static MessageBoxResult Show(string messageBoxText, string? caption, MessageBoxButton button, MessageBoxImage icon) => DoLogOnly(messageBoxText, caption);

        private static MessageBoxResult DoLogOnly(string text, string? caption = null) // TODO: Messagebox
        {
            caption ??= "<null>";
            Console.WriteLine("MSGBOX(" + caption + "): " + text);
            return MessageBoxResult.None;
        }
    }
}
#endif
