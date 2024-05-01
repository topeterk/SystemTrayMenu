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
    using System.Runtime.Versioning;
    using SystemTrayMenu.DllImports;
    using static SystemTrayMenu.DllImports.NativeMethods;

    [UnsupportedOSPlatform("windows")]
    internal static class FreeDesktop
    {
        private static readonly List<string> ThemeBaseDirs = new();

        private static readonly SortedDictionary<int /*weight*/, List<MimeGlobEntry>> MimeGlobs = new(
            Comparer<int>.Create((x, y) => -x.CompareTo(y))); // inverts default order to descending values

        private static readonly Dictionary<string /*MimeTypeName*/, string /*IconName*/> MimeIcons = new ();

        static FreeDesktop()
        {
            // Pre-build lookup folders
            // Mime base directories:
            // See: https://specifications.freedesktop.org/shared-mime-info-spec/latest/ar01s02.html#s2_layout
            //      Unclear what why these paths are mentioned when they are not mentioned to build the base: /usr, /usr/local and the user's home
            //      Further unclear why these examples arementioned, too: /usr/share/mime/text/html.xml, /usr/local/share/mime/text/html.xml, and ~/.local/share/mime/text/html.xml
            // Theme base directories:
            // See: https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout
            // See: https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html#variables
            string envPath;
            List<string> searchDirsMime = new();
            List<string> searchDirsTheme = new();
            string? envVar = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(envVar))
            {
                envPath = Path.Combine(envVar.Trim(), ".icons");
                if (Directory.Exists(envPath))
                {
                    searchDirsTheme.Add(envPath);
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
                            MimeGlobs.Add(weight, new () { mimeGlobEntry });
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
                        if (!MimeIcons.TryAdd(mimeTypeName, iconName))
                        {
                            MimeIcons[mimeTypeName] = iconName;
                        }
                    }
                }
            }

            // Prepare theme lookup paths
            // TODO: How to detect the currently loaded theme?
            //       https://unix.stackexchange.com/questions/419895/if-i-have-a-mime-type-how-do-i-get-its-associated-icon-from-the-current-appearan
            //          gsettings get org.gnome.desktop.interface icon-theme      >> 'Yaru'
            List<string> themeNames = new() { "Yaru", "hicolor" };
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

        // TODO: Find mimetype from file and load associated icon (return a list of matching icons instead of first best)
        //       Then caller can try to load alternative files when icon of better matches cannot be loaded
        //       See: https://unix.stackexchange.com/questions/123018/gtk-icons-for-special-files/200666#200666
        //            peter@ubuntu2204:/usr/share/icons$ gio info -a standard::icon ~/test.py
        //            uri: file:///home/peter/test.py
        //            local path: /home/peter/test.py
        //            unix mount: /dev/sda3 / ext4 rw,relatime,errors=remount-ro
        //            attributes:
        //            standard::icon: text-x-python, text-x-generic, text-x-python-symbolic, text-x-generic-symbolic
        //       Alternatively by looking up the type via /usr/share/mime/packages/*.xml
        internal static bool FindMimeTypeIcon(string path, out string mimeTypeIconName)
        {
            if (GetMimeTypeName(path, out string mimeTypeName))
            {
                if (MimeIcons.TryGetValue(mimeTypeName, out mimeTypeIconName))
                {
                    return true;
                }
            }

            mimeTypeIconName = string.Empty;
            return false;
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

                    string[] sizeValueStr = Path.GetFileName(sizePath).Split(new char[] { 'x', '@' });

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

        private static bool GetMimeTypeName(string path, out string mimeTypeName)
        {
            string? fileName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                foreach (var globEntries in MimeGlobs.Values)
                {
                    foreach (var mimeGlobEntry in globEntries)
                    {
                        FileNameMatchFlags flags = FileNameMatchFlags.FNM_PERIOD;
                        if (!mimeGlobEntry.IsCaseSensitive)
                        {
                            flags |= FileNameMatchFlags.FNM_CASEFOLD;
                        }

                        if (NativeMethods.fnmatch(mimeGlobEntry.Pattern, fileName, flags) == 0)
                        {
                            mimeTypeName = mimeGlobEntry.MimeTypeName;
                            return true;
                        }
                    }
                }
            }

            mimeTypeName = string.Empty;
            return false;
        }

        private struct MimeGlobEntry
        {
            internal string MimeTypeName;
            internal string Pattern;
            internal bool IsCaseSensitive;

            public override string ToString() => MimeTypeName + ":" + Pattern + (IsCaseSensitive ? ":cs" : string.Empty);
        }
    }
}
#endif
