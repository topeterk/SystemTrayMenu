// <copyright file="FreeDesktop.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// ===============================================================================
// MIT License
//
// Copyright (c) 2024-2024 Peter Kirmeier
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

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class FreeDesktop
    {
        private static readonly List<string> ThemeBaseDirs = new();

        static FreeDesktop()
        {
            // Pre-build lookup folders
            // See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout
            // See: https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html#variables
            string envPath;
            List<string> searchDirs = new();
            string? envVar = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(envVar))
            {
                envPath = Path.Combine(envVar.Trim(), ".icons");
                if (Directory.Exists(envPath))
                {
                    searchDirs.Add(envPath);
                }
            }

            envVar = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
            if (!string.IsNullOrEmpty(envVar))
            {
                foreach (string envEntry in envVar.Split(Path.PathSeparator))
                {
                    envPath = Path.Combine(envEntry.Trim(), "icons");
                    if (Directory.Exists(envPath))
                    {
                        searchDirs.Add(envPath);
                    }
                }
            }

            envPath = "/usr/share/pixmaps";
            if (Directory.Exists(envPath))
            {
                searchDirs.Add(envPath);
            }

            // Prepare all lookup paths
            // TODO: How to detect the currently loaded theme?
            //       https://unix.stackexchange.com/questions/419895/if-i-have-a-mime-type-how-do-i-get-its-associated-icon-from-the-current-appearan
            //          gsettings get org.gnome.desktop.interface icon-theme      >> 'Yaru'
            List<string> themeNames = new() { "Yaru", "hicolor" };
            foreach (string themeName in themeNames)
            {
                foreach (string searchDir in searchDirs)
                {
                    ThemeBaseDirs.Add(Path.Combine(searchDir, themeName));
                }
            }
        }

        internal enum DarkModePreference
        {
            Unkown,
            NoPreference,
            PreferDark,
            PreferLight,
        }

        internal static string FindThemeIcon(string context, string iconName)
        {
            // TODO: Lookup for specific sizes
            //       as of now we try to get 32x32 and 48x48 as fallback
            //       48x48 should be provided as minimum by specification
            //       See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#install_icons
            // See: https://specifications.freedesktop.org/icon-naming-spec/icon-naming-spec-latest.html
            List<string> iconSizes = new() { "32x32", "48x48" };

            bool isMimeType = context.Equals("mimetypes");

            foreach (string baseDir in ThemeBaseDirs)
            {
                foreach (string iconSize in iconSizes)
                {
                    string lookupDir = Path.Combine(baseDir, iconSize, context);
                    string baseName = iconName;
                    do
                    {
                        string iconPath = Path.Combine(lookupDir, baseName + ".png");
                        if (File.Exists(iconPath))
                        {
                            return iconPath;
                        }

                        // levels of specificity are not allowed for MimeTypes
                        if (isMimeType)
                        {
                            break;
                        }

                        int dashPos = baseName.LastIndexOf('-');
                        if (dashPos < 0)
                        {
                            // No matching file found
                            break;
                        }

                        // try again being less specific in the lookup name
                        baseName = baseName.Substring(0, dashPos);
                    }
                    while (true);
                }
            }

            return null;
        }

        /// <summary>
        /// Indicates the system's preferred color scheme.
        /// </summary>
        /// <returns>Prefered color scheme.</returns>
        internal static DarkModePreference GetDarkModePreference()
        {
            // TODO: Implement
            //       https://github.com/flatpak/xdg-desktop-portal/blob/main/data/org.freedesktop.portal.Settings.xml#L32-L39
            //       https://github.com/pbek/QOwnNotes/issues/2525
            //          dbus-send --session --print-reply=literal --reply-timeout=1000 --dest=org.freedesktop.portal.Desktop /org/freedesktop/portal/desktop org.freedesktop.portal.Settings.Read string:'org.freedesktop.appearance' string:'color-scheme'
            return DarkModePreference.Unkown;
        }
    }
}
#endif
