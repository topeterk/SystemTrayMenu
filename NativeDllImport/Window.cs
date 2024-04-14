// <copyright file="Window.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2024 Peter Kirmeier

namespace SystemTrayMenu.DllImports
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
#if !AVALONIA
    using System.Windows;
    using System.Windows.Interop;
#else
    using Avalonia.Controls;
    using Avalonia.Platform;
#endif

    /// <summary>
    /// wraps the methodcalls to native windows dll's.
    /// </summary>
    internal static partial class NativeMethods
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = -20;

        internal static void HideFromAltTab(Window window)
        {
            if (OperatingSystem.IsWindows())
            {
#if AVALONIA
                IPlatformHandle? wndHelper = window.TryGetPlatformHandle();
                if (wndHelper is null)
                {
                    return;
                }
#else
                WindowInteropHelper wndHelper = new WindowInteropHelper(window);
#endif

                if (Environment.Is64BitProcess)
                {
                    long exStyle = (long)GetWindowLongPtr(wndHelper.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_TOOLWINDOW; // do not show when user presses alt + tab
                    SetWindowLongPtr(wndHelper.Handle, GWL_EXSTYLE, (IntPtr)exStyle);
                }
                else
                {
                    int exStyle = (int)GetWindowLong(wndHelper.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_TOOLWINDOW; // do not show when user presses alt + tab
                    SetWindowLong(wndHelper.Handle, GWL_EXSTYLE, (IntPtr)exStyle);
                }
            }
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-findwindoww .
        /// </summary>
        /// <param name="lpClassName">The class name or a class atom.</param>
        /// <param name="lpWindowName">The window name.</param>
        /// <returns>Handle to the window or NULL on failure.</returns>
        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        internal static extern IntPtr User32FindWindow(string? lpClassName, string? lpWindowName);

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw .
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongptrw .
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongw .
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongptrw .
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
