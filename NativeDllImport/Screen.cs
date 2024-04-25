// <copyright file="Screen.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2024 Peter Kirmeier

namespace SystemTrayMenu.DllImports
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
#if !AVALONIA
    using System.Linq;
    using System.Windows;
#else
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Window = Avalonia.Controls.Window;
#endif

    /// <summary>
    /// wraps the methodcalls to native windows dll's.
    /// </summary>
    internal static partial class NativeMethods
    {
        internal static bool IsTouchEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                const int SM_MAXIMUMTOUCHES = 95;
                int maxTouches = GetSystemMetrics(SM_MAXIMUMTOUCHES);
                return maxTouches > 0;
            }

            return false;
        }

        [SupportedOSPlatform("Windows")]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern int GetSystemMetrics(int nIndex);

        internal static class Screen
        {
            private static List<Rect>? screens;

            private static Point LastCursorPosition = default;

#if AVALONIA
            internal static Screens DesktopScreens
            {
                get
                {
                    // TODO: Is there a better way as creating a new Window just to get updated the Screens?
                    return new Window().Screens;
                }
            }
#endif

            internal static List<Rect> Screens
            {
                get
                {
#if !AVALONIA
                    if ((screens == null) || (screens.Count == 0))
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            FetchScreens();
                        }
                    }

                    if ((screens == null) || (screens.Count == 0))
                    {
                        return new()
                        {
                            new (0, 0, 800, 600),
                        };
                    }
#else
                    int ScreenCount = DesktopScreens.ScreenCount;
                    if (ScreenCount == 0)
                    {
                        return new()
                        {
                            new (0, 0, 800, 600),
                        };
                    }
                    else
                    {
                        screens = new(ScreenCount);
                        foreach (var screen in DesktopScreens.All)
                        {
                            screens.Add(ScreenToRect(screen));
                        }
                    }
#endif
                    return screens;
                }
            }

#if !AVALONIA
            // The primary screen will have x = 0, y = 0 coordinates
            internal static Rect PrimaryScreen => Screens.FirstOrDefault((screen) => screen.Left == 0 && screen.Top == 0, Screens[0]);
#else
            internal static Rect PrimaryScreen => ScreenToRect(DesktopScreens.Primary);
#endif

            [SupportedOSPlatform("Windows")]
            internal static Point CursorPosition
            {
                get
                {
                    if (OperatingSystem.IsWindows())
                    {
                        NativeMethods.POINT lpPoint;
                        if (NativeMethods.GetCursorPos(out lpPoint))
                        {
                            LastCursorPosition = new(lpPoint.X, lpPoint.Y);
                        }
                    }

                    return LastCursorPosition;
                }
            }

#if AVALONIA
            /// <summary>
            /// Get Screen position from a the pointer event data associated to the given window.
            /// </summary>
            /// <param name="window">Window the event occured.</param>
            /// <param name="e">Pointer event data.</param>
            /// <returns>Point with cursor screen coordinates.</returns>
            internal static Point GetCursorPosition(Window window, PointerEventArgs e)
            {
                // TODO: Is calulation correct as window position is PixelPoint and we convert it without taking Dpi into account?
                return e.GetPosition(window) + new Point(window.Position.X, window.Position.Y);
            }
#endif

            internal static Rect FromPoint(Point pt)
            {
                foreach (Rect screen in Screens)
                {
                    if (screen.Contains(pt))
                    {
                        return screen;
                    }
                }

                // Use primary screen as fallback
                return PrimaryScreen;
            }

#if AVALONIA
            private static Rect ScreenToRect(Avalonia.Platform.Screen screen) => new(screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width, screen.WorkingArea.Height);
#else
            [SupportedOSPlatform("windows")]
            internal static void FetchScreens()
            {
                var backup = screens;
                screens = new();
                if (!NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallback, IntPtr.Zero))
                {
                    screens = backup;
                }
            }

            private static bool MonitorEnumCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData)
            {
                try
                {
                    screens!.Add(new()
                    {
                        X = lprcMonitor.left,
                        Y = lprcMonitor.top,
                        Width = lprcMonitor.right - lprcMonitor.left,
                        Height = lprcMonitor.bottom - lprcMonitor.top,
                    });
                }
                catch
                {
                    // Catch everything as this callback runs within a native context
                }

                return true;
            }
#endif

            private class NativeMethods
            {
                public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

                /// <summary>
                /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors .
                /// </summary>
                [SupportedOSPlatform("Windows")]
                [DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
                [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
                public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

                /// <summary>
                /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos .
                /// </summary>
                [SupportedOSPlatform("Windows")]
                [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
                [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
                public static extern bool GetCursorPos(out POINT lpPoint);

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
                internal struct RECT
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                }

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
                internal struct POINT
                {
                    public int X;
                    public int Y;
                }
            }
        }
    }
}
