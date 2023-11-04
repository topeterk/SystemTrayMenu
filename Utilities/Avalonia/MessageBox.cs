// <copyright file="MessageBox.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
using System;

namespace SystemTrayMenu.Utilities
{
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
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Asterisk = 64,
        Information = 64,
    }

    internal class MessageBox
    {
        private static MessageBoxResult DoLogOnly(string text, string? caption = null) // TODO: Messagebox
        {
            caption ??= "<null>";
            Console.WriteLine("MSGBOX(" + caption + "): " + text);
            return MessageBoxResult.None;
        }

        internal static MessageBoxResult Show(string messageBoxText) => DoLogOnly(messageBoxText);

        internal static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button) => DoLogOnly(messageBoxText, caption);

        internal static MessageBoxResult Show(string messageBoxText, string? caption, MessageBoxButton button, MessageBoxImage icon) => DoLogOnly(messageBoxText, caption);
    }
}
#endif
