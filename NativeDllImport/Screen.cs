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
    using SystemTrayMenu.Utilities;
    using Rect = System.Drawing.Rectangle;
    using Window = Avalonia.Controls.Window;
#endif

    /// <summary>
    /// wraps the methodcalls to native windows dll's.
    /// </summary>
    internal static partial class NativeMethods
    {
#if AVALONIA
        internal static bool Contains(this Rect rect, Point pt) => rect.Contains((int)pt.X, (int)pt.Y);
#endif

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

#if !AVALONIA
            private static Point LastCursorPosition = default(Point);
#else
            private static Window? screensWrapperWindow;

            internal static Screens DesktopScreens
            {
                get
                {
                    screensWrapperWindow ??= new Window();
                    return screensWrapperWindow.Screens;
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
                        FetchScreens();
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

            internal static Point CursorPosition
            {
                get
                {
#if !AVALONIA
#if TODO // Maybe use Windows.Desktop instead of Win32 API?
         // See: https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.mouse.getposition?view=windowsdesktop-8.0
                    if (Mouse.Capture(menu))
                    {
                        LastCursorPosition = Mouse.GetPosition(menu);
                        Mouse.Capture(null);
                    }
#else
                    NativeMethods.POINT lpPoint;
                    if (NativeMethods.GetCursorPos(out lpPoint))
                    {
                        LastCursorPosition = new(lpPoint.X, lpPoint.Y);
                    }
#endif
                    return LastCursorPosition;
#else
                    // TODO: Based on another window as mainWindow is no longer set?
                    //  ((IClassicDesktopStyleApplicationLifetime?)Application.Current!.ApplicationLifetime)!.MainWindow!
                    return Mouse.GetPosition(new Window());
#endif
                }
            }

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
#endif
        }
    }
}
