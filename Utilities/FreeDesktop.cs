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
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Versioning;
    using SystemTrayMenu.DllImports;
    using static SystemTrayMenu.DllImports.NativeMethods;

    [UnsupportedOSPlatform("windows")]
    internal static class FreeDesktop
    {
        private static readonly List<string> ThemeBaseDirs = [];

        private static readonly Dictionary<string /*subclass*/, string /*base*/> MimeSubtypes = [];
        private static readonly Dictionary<string /*MimeTypeName*/, string /*IconName*/> MimeIcons = [];
        private static readonly SortedDictionary<int /*weight*/, List<MimeGlobEntry>> MimeGlobs = new(
            Comparer<int>.Create((x, y) => -x.CompareTo(y))); // inverts default order to descending values

        static FreeDesktop()
        {
            // Pre-build lookup folders
            // Mime base directories:
            // See: https://specifications.freedesktop.org/shared-mime-info-spec/latest/ar01s02.html#s2_layout
            // Theme base directories:
            // See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout
            // See: https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html#variables
            string envPath;
            List<string> searchDirsMime = [];
            List<string> searchDirsTheme = [];
            string? envVar = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(envVar))
            {
                envPath = Path.Combine(envVar.Trim(), ".icons");
                if (Directory.Exists(envPath))
                {
                    searchDirsTheme.Add(envPath);
                }

                // Even when specification does not mention this path it seems to be some default path
                // as "mimetype -a -D  test.cpp" shows the home path even when not present in environment variables
                //    Data dirs are: /home/peter/.local/share, /usr/share/ubuntu, /usr/local/share, /usr/share, /var/lib/snapd/desktop
                //    XDG_DATA_HOME is ""
                //    XDG_DATA_DIRS is "/usr/share/ubuntu:/usr/local/share/:/usr/share/:/var/lib/snapd/desktop"
                envPath = Path.Combine(envVar.Trim(), ".local", "share");
                if (Directory.Exists(envPath))
                {
                    searchDirsMime.Add(envPath);
                }
            }

            envVar = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (!string.IsNullOrEmpty(envVar))
            {
                foreach (string envEntry in envVar.Split(Path.PathSeparator))
                {
                    string envEntryTrimmed = envEntry.Trim();
                    envPath = Path.Combine(envEntryTrimmed, "mime");
                    if (Directory.Exists(envPath))
                    {
                        searchDirsMime.Add(envPath);
                    }
                }
            }

            envVar = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
            if (!string.IsNullOrEmpty(envVar))
            {
                foreach (string envEntry in envVar.Split(Path.PathSeparator))
                {
                    string envEntryTrimmed = envEntry.Trim();
                    envPath = Path.Combine(envEntryTrimmed, "mime");
                    if (Directory.Exists(envPath))
                    {
                        searchDirsMime.Add(envPath);
                    }

                    envPath = Path.Combine(envEntryTrimmed, "icons");
                    if (Directory.Exists(envPath))
                    {
                        searchDirsTheme.Add(envPath);
                    }
                }
            }

            envPath = "/usr/share/pixmaps";
            if (Directory.Exists(envPath))
            {
                searchDirsTheme.Add(envPath);
            }

            // Load mime type database (mapping of filename to mimetype)
            foreach (string searchDir in searchDirsMime)
            {
                // We ignore deprecated globs and only use globs2
                string fileNameGlob = Path.Combine(searchDir, "globs2");
                if (!File.Exists(fileNameGlob))
                {
                    continue;
                }

                IEnumerable<string> lines;
                try
                {
                    lines = File.ReadLines(fileNameGlob, System.Text.Encoding.UTF8);
                }
                catch
                {
                    continue;
                }

                foreach (string line in lines)
                {
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }

                    string[] fields = line.Split(':');
                    if (fields.Length < 3)
                    {
                        continue;
                    }

                    string mimeTypeName = fields[1];
                    string pattern = fields[2];

                    // Remove all previous definitions when requested
                    if (pattern.Equals("__NOGLOBS__"))
                    {
                        foreach (var globEntries in MimeGlobs.Values)
                        {
                            globEntries.RemoveAll((item) => item.MimeTypeName.Equals(mimeTypeName));
                        }
                    }
                    else if (int.TryParse(fields[0], out int weight))
                    {
                        // Add new mime type
                        MimeGlobEntry mimeGlobEntry = new()
                        {
                            MimeTypeName = mimeTypeName,
                            Pattern = pattern,
                        };

                        // Parse flags, when available
                        if (fields.Length > 3)
                        {
                            string[] flags = fields[3].Split(',');
                            foreach (string flag in flags)
                            {
                                if (flag.Equals("cs"))
                                {
                                    mimeGlobEntry.IsCaseSensitive = true;
                                }
                            }
                        }

                        if (MimeGlobs.TryGetValue(weight, out List<MimeGlobEntry> mimes))
                        {
                            mimes.Add(mimeGlobEntry);
                        }
                        else
                        {
                            MimeGlobs.Add(weight, [mimeGlobEntry]);
                        }
                    }
                }
            }

            // Load mime type database (mapping of mimetypes to icons)
            foreach (string searchDir in searchDirsMime)
            {
                foreach (string fileNameIcons in new string[] { Path.Combine(searchDir, "generic-icons"), Path.Combine(searchDir, "icons") })
                {
                    if (!File.Exists(fileNameIcons))
                    {
                        continue;
                    }

                    IEnumerable<string> lines;
                    try
                    {
                        lines = File.ReadLines(fileNameIcons);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (string line in lines)
                    {
                        string[] fields = line.Split(':');
                        if (fields.Length != 2)
                        {
                            continue;
                        }

                        string mimeTypeName = fields[0];
                        string iconName = fields[1];

                        // Add definition when not already existing
                        MimeIcons.TryAdd(mimeTypeName, iconName);
                    }
                }
            }

            // Load mime type database (mapping of subclasses to mimetypes)
            foreach (string searchDir in searchDirsMime)
            {
                string fileNameSubclasses = Path.Combine(searchDir, "subclasses");
                if (!File.Exists(fileNameSubclasses))
                {
                    continue;
                }

                IEnumerable<string> lines;
                try
                {
                    lines = File.ReadLines(fileNameSubclasses);
                }
                catch
                {
                    continue;
                }

                foreach (string line in lines)
                {
                    string[] fields = line.Split(' ');
                    if (fields.Length != 2)
                    {
                        continue;
                    }

                    string subclassName = fields[0];
                    string mimeTypeName = fields[1];

                    // Add definition when not already existing
                    MimeSubtypes.TryAdd(subclassName, mimeTypeName);
                }
            }

            // Prepare theme lookup paths
            List<string> themeNames = FindThemes();
            foreach (string themeName in themeNames)
            {
                foreach (string searchDir in searchDirsTheme)
                {
                    string baseDir = Path.Combine(searchDir, themeName);
                    if (Directory.Exists(baseDir))
                    {
                        ThemeBaseDirs.Add(baseDir);
                    }
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

        private enum ThemeDetectionMethod
        {
            GSETTINGS,
            KDE_CONFIG,
            GTK_CONFIG,
        }

        // Similar to:  gio info -a standard::icon ~/test.py
        //              gio info -a standard::symbolic-icon ~/test.cpp
        internal static bool FindMimeTypeIcons(string path, out List<string> mimeTypeIconNames)
        {
            mimeTypeIconNames = [];
            if (GetMimeTypeNames(path, out List<string> mimeTypeNames))
            {
                foreach (string mimeTypeName in mimeTypeNames)
                {
                    if (MimeIcons.TryGetValue(mimeTypeName, out string mimeTypeIconName))
                    {
                        if (!mimeTypeIconNames.Contains(mimeTypeIconName))
                        {
                            mimeTypeIconNames.Add(mimeTypeIconName);
                        }
                    }
                }
            }

            if (mimeTypeIconNames.Count == 0)
            {
                // When no icon has been found, try to guess icon names based on mime type name
                // This is not by specification but should help with inproper/incomplete/out-of-spec installations
                // It should also help agains forbidden levels of specificity during lookup for mimetype icons
                foreach (string mimeTypeName in mimeTypeNames)
                {
                    mimeTypeIconNames.Add(mimeTypeName.Replace('/', '-')); // e.g. "image/png" -> "image-png"
                }
            }

            return mimeTypeIconNames.Count > 0;
        }

        internal static string? FindThemeIcon(string context, string iconName, int desiredSize)
        {
            bool isMimeType = context.Equals("mimetypes");

            // loop over all levels of specificity
            // See: https://specifications.freedesktop.org/icon-naming-spec/icon-naming-spec-latest.html#guidelines
            do
            {
                string? iconPath = FindThemeIconExactName(context, iconName, desiredSize);

                // return found image or exit early as levels of specificity are not allowed for MimeTypes
                if (!string.IsNullOrEmpty(iconPath) || isMimeType)
                {
                    return iconPath;
                }

                int dashPos = iconName.LastIndexOf('-');
                if (dashPos < 0)
                {
                    // No matching file found
                    return null;
                }

                // try again being less specific in the lookup name
                iconName = iconName[0..dashPos];
            }
            while (true);
        }

        internal static string? FindThemeIconExactName(string context, string iconName, int desiredSize)
        {
            string desiredSizeDir = $"{desiredSize}x{desiredSize}";

            // Lookup a given icon with best fitting size with best fitting theme in most preferred install location
            // See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#icon_lookup
            // However, we skip looking into index.theme as current solutions as good enough for now
            // TODO: Respect index.theme accordingly to specification
            //       See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout
            foreach (string baseDir in ThemeBaseDirs)
            {
                // Try exact match right away
                string iconPath = Path.Combine(baseDir, desiredSizeDir, context, iconName + ".png");
                if (File.Exists(iconPath))
                {
                    return iconPath;
                }

                string[] sizePaths = Directory.GetDirectories(baseDir);

                int closestSmallerDiff = int.MinValue;
                int closestBiggerDiff = int.MaxValue;
                string? closestPathSmaller = null;
                string? closestPathBigger = null;

                foreach (string sizePath in sizePaths)
                {
                    if (sizePath.Equals(desiredSizeDir))
                    {
                        // Exact match already checked at the beginning
                        continue;
                    }

                    string[] sizeValueStr = Path.GetFileName(sizePath).Split(['x', '@']);

                    // We do not want scaled variations and we only look for equilateral dimensions
                    if ((sizeValueStr.Length == 2) && sizeValueStr[0].Equals(sizeValueStr[1]))
                    {
                        if (int.TryParse(sizeValueStr[0], out int sizeValue))
                        {
                            iconPath = Path.Combine(sizePath, context, iconName + ".png");
                            if (File.Exists(iconPath))
                            {
                                int diff = sizeValue - desiredSize;
                                if ((diff < 0) && (closestSmallerDiff < diff))
                                {
                                    closestPathSmaller = iconPath;
                                    closestSmallerDiff = diff;
                                }
                                else if (diff < closestBiggerDiff)
                                {
                                    closestPathBigger = iconPath;
                                    closestBiggerDiff = diff;
                                }
                            }
                        }
                    }
                }

                // Prefer higher resolution image before falling back to lower resolution
                if (!string.IsNullOrEmpty(closestPathBigger))
                {
                    return closestPathBigger;
                }
                else if (!string.IsNullOrEmpty(closestPathSmaller))
                {
                    return closestPathSmaller;
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

        /// <summary>
        /// Fetches multiple matching mime types for the given file.
        ///    Should be more detailed than using xdg-mime (See https://man.archlinux.org/man/xdg-mime.1)
        ///    xdg-mime query filetype test.cpp
        ///    This may not give good enough results therefore we fetch data from FreeDesktop databases directly.
        /// </summary>
        /// <param name="path">Path to the file which mime type should be found.</param>
        /// <param name="mimeTypeNames">List of matching types (ordered by significance: first in list fits best).</param>
        /// <returns>true: One or more mime types found, false: No mime type found.</returns>
        private static bool GetMimeTypeNames(string path, out List<string> mimeTypeNames)
        {
            mimeTypeNames = [];

            int longesPattern = 0;
            string? fileName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                // Lookup globs
                // See [1]: https://specifications.freedesktop.org/shared-mime-info-spec/latest/ar01s02.html
                foreach (var globEntries in MimeGlobs.Values)
                {
                    foreach (var mimeGlobEntry in globEntries)
                    {
                        // [1] start by doing a glob match of the filename.
                        FileNameMatchFlags flags = FileNameMatchFlags.FNM_PERIOD;
                        if (!mimeGlobEntry.IsCaseSensitive)
                        {
                            flags |= FileNameMatchFlags.FNM_CASEFOLD;
                        }

                        // [1] The format of the glob pattern is as for fnmatch(3)
                        if (NativeMethods.fnmatch(mimeGlobEntry.Pattern, fileName, flags) == 0)
                        {
                            // [1] If the patterns are different, keep only globs with the longest pattern
                            if (longesPattern < mimeGlobEntry.Pattern.Length)
                            {
                                longesPattern = mimeGlobEntry.Pattern.Length;
                                mimeTypeNames.Clear();
                            }

                            // Add it when not already existing
                            if (!mimeTypeNames.Contains(mimeGlobEntry.MimeTypeName))
                            {
                                mimeTypeNames.Add(mimeGlobEntry.MimeTypeName);
                            }
                        }
                    }

                    // list is sorted by weight, so if we found matches, we can stop here
                    // [1] Keep only globs with the biggest weight.
                    if (mimeTypeNames.Count > 0)
                    {
                        // [1] there is one or more matching glob, and all the matching globs result in the same mimetype, use that mimetype as the result.
                        //     If the glob matching fails or results in multiple conflicting mimetypes, read the contents of the file and do magic sniffing on it
                        // As we do want to do magic sniffing on files, we just return our best guess or return no mimetype instead.

                        // However, fill in fallbacks by adding base types of subclasses as well
                        // The inherited types will be added to the end and then next subclass is evaluated first.
                        // This ensures that when looking up the types the top level base classe(s) come(s) last rather in between.
                        for (int index = 0; index < mimeTypeNames.Count; index++)
                        {
                            if (MimeSubtypes.TryGetValue(mimeTypeNames[index], out string baseTypeName))
                            {
                                if (!mimeTypeNames.Contains(baseTypeName))
                                {
                                    mimeTypeNames.Add(baseTypeName);
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static List<string> FindThemes()
        {
            List<Tuple<string, ThemeDetectionMethod>> detectionMethodsPattern =
            [
                new ("Cinnamon", ThemeDetectionMethod.GSETTINGS),
                new ("GNOME", ThemeDetectionMethod.GSETTINGS ),
                new ("LXDE", ThemeDetectionMethod.GSETTINGS ),
                new ("MATE", ThemeDetectionMethod.GSETTINGS ),
                new ("XFCE", ThemeDetectionMethod.GSETTINGS ),
                new ("KDE", ThemeDetectionMethod.KDE_CONFIG ),
                new ("LXQt", ThemeDetectionMethod.GTK_CONFIG ),
            ];
            List<ThemeDetectionMethod> detectionMethods = [];

            // Detect preferred environment and add detection methods
            // Based on table from archlinux' xdg-utils documentation
            // See: https://wiki.archlinux.org/title/Xdg-utils#xdg-open
            string? envVar = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            if (!string.IsNullOrEmpty(envVar))
            {
                string[] envDesktopNames = envVar.Split(':');
                foreach (string desktopName in envDesktopNames)
                {
                    foreach (var (desktopPattern, detectionMethod) in detectionMethodsPattern)
                    {
                        if (desktopName.Equals(desktopPattern))
                        {
                            if (!detectionMethods.Contains(detectionMethod))
                            {
                                detectionMethods.Add(detectionMethod);
                            }
                        }
                    }
                }
            }

            // Add other methods as well as fallback
            foreach (ThemeDetectionMethod detectionMethod in Enum.GetValues<ThemeDetectionMethod>())
            {
                if (!detectionMethods.Contains(detectionMethod))
                {
                    detectionMethods.Add(detectionMethod);
                }
            }

            // Try each method to find a selected theme
            string? themeName;
            List<string> themeNames = [];
            foreach (ThemeDetectionMethod detectionMethod in detectionMethods)
            {
                // See: https://unix.stackexchange.com/questions/419895/if-i-have-a-mime-type-how-do-i-get-its-associated-icon-from-the-current-appearan
                switch (detectionMethod)
                {
                    case ThemeDetectionMethod.GSETTINGS:
                        {
                            themeName = ReadStringFromProcessNoThrow("gsettings", "get org.gnome.desktop.interface icon-theme");
                            if (!string.IsNullOrEmpty(themeName))
                            {
                                themeName = themeName.Trim().Trim(['"', '\'']);
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }
                            }

                            themeName = ReadStringFromProcessNoThrow("gsettings", "get org.gnome.desktop.interface gtk-theme");
                            if (!string.IsNullOrEmpty(themeName))
                            {
                                themeName = themeName.Trim().Trim(['"', '\'']);
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }
                            }

                            break;
                        }

                    case ThemeDetectionMethod.KDE_CONFIG:
                        {
                            // TODO: Maybe try something like this before doing it manually?
                            //       kreadconfig5--file kdeglobals --group General--key Name

                            // Get search path and load configuration file
                            // TODO: Make sure kf5-config returns a single path in proper format
                            // See: https://www.giovanniceribella.eu/fuere/?p=1641
                            string? output = ReadStringFromProcessNoThrow("kf5-config", "–path config");
                            if (!string.IsNullOrEmpty(output))
                            {
                                output = output.Trim().Trim(['"', '\'']);
                            }

                            if (string.IsNullOrEmpty(output))
                            {
                                output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                            }

                            string kdeGlobalsFile = Path.Combine(output, "kdeglobals");
                            if (File.Exists(kdeGlobalsFile))
                            {
                                IEnumerable<string> lines;
                                try
                                {
                                    lines = File.ReadLines(kdeGlobalsFile);
                                    themeName = ReadValueFromIniFile(lines, "Icons", "Theme");
                                    if (!string.IsNullOrEmpty(themeName))
                                    {
                                        if (!themeNames.Contains(themeName))
                                        {
                                            themeNames.Add(themeName);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }

                            break;
                        }

                    case ThemeDetectionMethod.GTK_CONFIG:
                        {
                            string envPath;

                            // Try GTK3.0 config ...

                            // Look for configurations in all search paths
                            // See: https://docs.gtk.org/gtk3/class.Settings.html
                            List<string> settingFiles = [];

                            // This path is not mentioned in documentation but whole world is talking about it, so we assume it exists as local override and add it first
                            envPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "gtk-3.0", "settings.ini");
                            if (File.Exists(envPath))
                            {
                                settingFiles.Add(envPath);
                            }

                            envVar = Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS");
                            if (!string.IsNullOrEmpty(envVar))
                            {
                                foreach (string envEntry in envVar.Split(Path.PathSeparator))
                                {
                                    string envEntryTrimmed = envEntry.Trim();
                                    envPath = Path.Combine(envEntryTrimmed, "gtk-3.0", "settings.ini");
                                    if (File.Exists(envPath))
                                    {
                                        settingFiles.Add(envPath);
                                    }
                                }
                            }

                            envVar = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                            if (!string.IsNullOrEmpty(envVar))
                            {
                                foreach (string envEntry in envVar.Split(Path.PathSeparator))
                                {
                                    string envEntryTrimmed = envEntry.Trim();
                                    envPath = Path.Combine(envEntryTrimmed, "gtk-3.0", "settings.ini");
                                    if (File.Exists(envPath))
                                    {
                                        settingFiles.Add(envPath);
                                    }
                                }
                            }

                            // Example file found here: https://github.com/MaskRay/Config/blob/master/home/.config/gtk-3.0/settings.ini
                            // Maybe one could look for other or better keys instead, but based on current knowledge, these should be sufficient
                            foreach (string settingFile in settingFiles)
                            {
                                IEnumerable<string> lines;
                                try
                                {
                                    lines = File.ReadLines(settingFile);
                                }
                                catch
                                {
                                    continue;
                                }

                                themeName = ReadValueFromIniFile(lines, "Settings", "gtk-icon-theme-name");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }

                                themeName = ReadValueFromIniFile(lines, "Settings", "gtk-fallback-icon-theme");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }

                                themeName = ReadValueFromIniFile(lines, "Settings", "gtk-theme-name");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }
                            }

                            // Try GTK2.0 config ...

                            // Look for configurations in all search paths
                            // Note: No specification found yet, all paths are assumptions
                            settingFiles.Clear();

                            // A automatically created file at ~/.gtkrc-2.0 mentioned another file to look at:
                            //     # Any customization should be done in ~/.gtkrc-2.0.mine instead.
                            envPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gtkrc-2.0.mine");
                            if (File.Exists(envPath))
                            {
                                settingFiles.Add(envPath);
                            }

                            // ~/.gtkrc-2.0 is mentioned at https://wiki.archlinux.org/title/Cursor_themes
                            envPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gtkrc-2.0");
                            if (File.Exists(envPath))
                            {
                                settingFiles.Add(envPath);
                            }

                            // No specification found yet, but someone states at https://www.gnome-look.org/p/1015532:
                            //     This file must either be: $HOME/.gtkrc-2.0 or: /etc/gtk-2.0/gtkrc
                            envVar = Environment.GetEnvironmentVariable("HOME");
                            if (!string.IsNullOrEmpty(envVar))
                            {
                                envPath = Path.Combine(envVar.Trim(), ".gtkrc-2.0");
                                if (File.Exists(envPath))
                                {
                                    settingFiles.Add(envPath);
                                }
                            }

                            envPath = "/etc/gtk-2.0/gtkrc";
                            if (File.Exists(envPath))
                            {
                                settingFiles.Add(envPath);
                            }

                            // Example file found here: https://github.com/drinkcat/config/blob/master/.gtkrc-2.0
                            //                          https://github.com/ssokolow/profile/blob/master/home/.gtkrc-2.0.mine
                            // No specification found yet about file content, implementation is based on the examples only
                            foreach (string settingFile in settingFiles)
                            {
                                IEnumerable<string> lines;
                                try
                                {
                                    lines = File.ReadLines(settingFile);
                                }
                                catch
                                {
                                    continue;
                                }

                                themeName = ReadValueFromGtk2CfgFile(lines, "gtk-icon-theme-name");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }

                                themeName = ReadValueFromGtk2CfgFile(lines, "gnome-theme-icon-name");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }

                                themeName = ReadValueFromGtk2CfgFile(lines, "gtk-theme-name");
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    if (!themeNames.Contains(themeName))
                                    {
                                        themeNames.Add(themeName);
                                    }
                                }
                            }

                            break;
                        }

                    default:
                        throw new NotImplementedException("Theme detection method not implemented for " + detectionMethod.ToString());
                }
            }

            // Always add default search path
            // https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout
            //     Implementations are required to look in the "hicolor" theme if an icon was not found in the current theme.
            themeNames.Add("hicolor");

            return themeNames;
        }

        private static string? ReadStringFromProcessNoThrow(string filename, string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ()
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        // TODO: Check and improve parsing as syntax is not 100 percent know yet
        private static string? ReadValueFromIniFile(IEnumerable<string> lines, string section, string key)
        {
            string lineTrimmed;
            string sectionLine = $"[{section}]";
            bool inIconsSection = false;
            foreach (string line in lines)
            {
                lineTrimmed = line.Trim();

                // not sure, if there are comments, so ignore anything after
                if (lineTrimmed.StartsWith(sectionLine))
                {
                    inIconsSection = true;
                }

                // ok it is nasty but good enough
                else if (lineTrimmed.StartsWith('[') && lineTrimmed.Contains(']'))
                {
                    inIconsSection = false;
                }
                else if (inIconsSection)
                {
                    string[] fields = lineTrimmed.Split('=');
                    if (fields.Length == 2)
                    {
                        if (fields[0].Trim().Equals(key))
                        {
                            return fields[1].Trim();
                        }
                    }
                }
            }

            return null;
        }

        private static string? ReadValueFromGtk2CfgFile(IEnumerable<string> lines, string key)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                string[] fields = line.Split('=');
                if (fields.Length == 2)
                {
                    continue;
                }

                if (fields[0].Trim().Equals(key))
                {
                    return fields[1].Trim().Trim(['"', '\'' ]);
                }
            }

            return null;
        }

        private struct MimeGlobEntry
        {
            internal string MimeTypeName;
            internal string Pattern;
            internal bool IsCaseSensitive;

            public override readonly string ToString() => MimeTypeName + ":" + Pattern + (IsCaseSensitive ? ":cs" : string.Empty);
        }
    }
}
#endif
