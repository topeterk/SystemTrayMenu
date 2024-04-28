// <copyright file="Config.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;
    using Microsoft.Win32;
#if AVALONIA
    using Avalonia.Controls;
    using Window = SystemTrayMenu.Utilities.Window;
#else
    using System.Windows;
    using Icon = System.Drawing.Icon;
#endif
#if !WINDOWS
    using static SystemTrayMenu.Utilities.FreeDesktop;
#endif
    using SystemTrayMenu.DllImports;
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.UserInterface.FolderBrowseDialog;
    using SystemTrayMenu.Utilities;

    public static class Config
    {
#if AVALONIA
#if WINDOWS
        private static WindowIcon? iconRootFolder;
#endif
#else
        private static Icon? iconRootFolder;
        private static Icon? applicationIcon;
#endif

        private static bool readDarkModeDone;
        private static bool isDarkMode;
        private static bool readHideFileExtdone;
        private static bool isHideFileExtension;

        public static string Path => Settings.Default.PathDirectory;

        public static string SearchPattern => Settings.Default.SearchPattern;

        public static bool ShowDirectoryTitleAtTop => Settings.Default.ShowDirectoryTitleAtTop;

        public static bool ShowSearchBar => Settings.Default.ShowSearchBar;

        public static bool ShowCountOfElementsBelow => Settings.Default.ShowCountOfElementsBelow;

        public static bool ShowFunctionKeyOpenFolder => Settings.Default.ShowFunctionKeyOpenFolder;

        public static bool ShowFunctionKeyPinMenu => Settings.Default.ShowFunctionKeyPinMenu;

        public static bool ShowFunctionKeySettings => Settings.Default.ShowFunctionKeySettings;

        public static bool ShowFunctionKeyRestart => Settings.Default.ShowFunctionKeyRestart;

        public static bool AlwaysOpenByPin { get; internal set; }

        public static void Initialize()
        {
            UpgradeIfNotUpgraded();
            if (string.IsNullOrEmpty(Settings.Default.PathIcoDirectory))
            {
                Settings.Default.PathIcoDirectory = System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        $"SystemTrayMenu"),
                    "ico");
                if (!Directory.Exists(Settings.Default.PathIcoDirectory))
                {
                    Directory.CreateDirectory(Settings.Default.PathIcoDirectory);
                }
            }
        }

#if AVALONIA
        public static WindowIcon? GetCustomAppIcon()
#else
        [SupportedOSPlatform("windows")]
        public static Icon GetCustomAppIcon()
#endif
        {
#if AVALONIA
#if WINDOWS
            if (Settings.Default.UseIconFromRootFolder && OperatingSystem.IsWindows())
            {
                if (iconRootFolder is null)
                {
                    var icon = IconReader.GetRootFolderIcon(Path);
                    if (icon is not null)
                    {
                        BitmapSource? bitmap = icon;
                        icon.Dispose();
                        iconRootFolder ??= new(bitmap);
                        bitmap.Dispose();
                    }
                }

                return iconRootFolder;
            }
#endif

            return null;
#else
            if (Settings.Default.UseIconFromRootFolder && iconRootFolder is null)
            {
                // Load icon only once
                if (OperatingSystem.IsWindows())
                {
                    iconRootFolder = IconReader.GetRootFolderIcon(Path);
                }
            }

            if (Settings.Default.UseIconFromRootFolder && iconRootFolder is not null)
            {
                return iconRootFolder;
            }
            else
            {
                if (applicationIcon == null)
                {
                    Icon icon = App.LoadIconFromResource("Resources/SystemTrayMenu.ico");
                    applicationIcon = new(icon, (int)SystemParameters.SmallIconWidth, (int)SystemParameters.SmallIconHeight);
                    icon.Dispose();
                }

                return applicationIcon;
            }
#endif
        }

        public static void ParseCommandline(string[] args)
        {
            // When given by command line take path from there
            if (args.Length > 0 && args[0] != "-r")
            {
                string path = args[0];
                Log.Info($"SetFolderByCommandLine() path: {path}");
                Settings.Default.PathDirectory = path;
                Settings.Default.Save();
            }
        }

        public static async Task<bool> SetFolderByUser(Window owner, bool save = true)
        {
            using FolderDialog dialog = new();
            dialog.InitialFolder = Path;

            if (await dialog.ShowDialog(owner))
            {
                Settings.Default.PathDirectory = dialog.Folder;
                if (save)
                {
                    Settings.Default.Save();
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> SetFolderIcoByUser(Window owner)
        {
            using FolderDialog dialog = new();
            dialog.InitialFolder = Settings.Default.PathIcoDirectory;

            if (await dialog.ShowDialog(owner))
            {
                Settings.Default.PathIcoDirectory = dialog.Folder;
                return true;
            }

            return false;
        }

        internal static void ShowHelpFAQ()
        {
            Log.ProcessStart("https://github.com/Hofknecht/SystemTrayMenu#FAQ");
        }

        internal static void ShowSupportSystemTrayMenu()
        {
            Log.ProcessStart("https://github.com/Hofknecht/SystemTrayMenu#donations");
        }

        /// <summary>
        /// Read the OS setting whether dark mode is enabled.
        /// </summary>
        /// <returns>true = Dark mode; false = Light mode.</returns>
        internal static bool IsDarkMode()
        {
            if (!readDarkModeDone)
            {
                if (Settings.Default.IsDarkModeAlwaysOn)
                {
                    isDarkMode = true;
                }

                if (OperatingSystem.IsWindows())
                {
                    // 0 = Dark mode, 1 = Light mode
                    if (!isDarkMode && IsRegistryValueThisValue(
                                            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                                            "AppsUseLightTheme",
                                            "0"))
                    {
                        isDarkMode = true;
                    }

                    // Required for native UI rendering like the ShellContextMenu
                    NativeMethods.SetPreferredAppMode(isDarkMode ? NativeMethods.PreferredAppMode.ForceDark : NativeMethods.PreferredAppMode.ForceLight);
                    NativeMethods.FlushMenuThemes();
                }
#if !WINDOWS
                else
                {
                    // TODO: Allow user to set DarkMode app specific or system specific
                    if (!isDarkMode)
                    {
                        DarkModePreference preference = FreeDesktop.GetDarkModePreference();
                        if (preference == DarkModePreference.PreferDark)
                        {
                            isDarkMode = true;
                        }
                    }
                }
#endif

                readDarkModeDone = true;
            }

            return isDarkMode;
        }

        internal static void ResetReadDarkModeDone()
        {
            isDarkMode = false;
            readDarkModeDone = false;
        }

        /// <summary>
        /// Read the OS setting whether HideFileExt enabled.
        /// </summary>
        /// <returns>isHideFileExtension.</returns>
        internal static bool IsHideFileExtension()
        {
            if (!readHideFileExtdone)
            {
                if (OperatingSystem.IsWindows())
                {
                    // 0 = To show extensions, 1 = To hide extensions
                    if (IsRegistryValueThisValue(
                            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                            "HideFileExt",
                            "1"))
                    {
                        isHideFileExtension = true;
                    }
                }

                readHideFileExtdone = true;
            }

            return isHideFileExtension;
        }

        [SupportedOSPlatform("Windows")]
        private static bool IsRegistryValueThisValue(string keyName, string valueName, string value)
        {
            bool isRegistryValueThisValue = false;

            try
            {
                object? registryHideFileExt = Registry.GetValue(keyName, valueName, 1);

                if (registryHideFileExt == null)
                {
                    Log.Info($"Could not read registry keyName:{keyName} valueName:{valueName}");
                }
                else if (registryHideFileExt.ToString() == value)
                {
                    isRegistryValueThisValue = true;
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Security.SecurityException ||
                    ex is IOException)
                {
                    Log.Warn($"Could not read registry keyName:{keyName} valueName:{valueName}", ex);
                }
                else
                {
                    throw;
                }
            }

            return isRegistryValueThisValue;
        }

        private static void UpgradeIfNotUpgraded()
        {
            if (!Settings.Default.IsUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.IsUpgraded = true;
                Settings.Default.Save();
                Log.Info($"Settings upgraded from {CustomSettingsProvider.UserConfigPath}");
            }
        }
    }
}
