﻿// <copyright file="FileLnk.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Threading;
#if WINDOWS
    using Shell32;
#endif

    internal class FileLnk
    {
        public static string GetResolvedFileName(string shortcutFilename, out bool isFolder)
        {
            bool isFolderByShell = false;
            string resolvedFilename = string.Empty;
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                resolvedFilename = GetShortcutFileNamePath(shortcutFilename, out isFolderByShell);
            }
            else
            {
                Thread staThread = new(new ParameterizedThreadStart(StaThreadMethod));
                void StaThreadMethod(object? obj)
                {
                    resolvedFilename = GetShortcutFileNamePath(shortcutFilename, out isFolderByShell);
                }

                if (OperatingSystem.IsWindows())
                {
                    staThread.SetApartmentState(ApartmentState.STA);
                }

                staThread.Start(shortcutFilename);
                staThread.Join();
            }

            // If path cannot be resolved, write log message and give back original path
            if (string.IsNullOrEmpty(resolvedFilename))
            {
                Log.Info($"Resolved path is empty: '{shortcutFilename}'");
                resolvedFilename = shortcutFilename;
            }

            isFolder = isFolderByShell;

            return resolvedFilename;
        }

        [SupportedOSPlatformGuard("Windows")]
        public static bool IsNetworkRoot(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                // This is a windows specific path starting with "\\"
                return path.StartsWith($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", StringComparison.InvariantCulture) &&
                !path[2..].Contains(Path.DirectorySeparatorChar, StringComparison.InvariantCulture);
            }
            else
            {
                // Other systems do not have this as they are mounted into the one and only file system tree
                return false;
            }
        }

        private static string GetShortcutFileNamePath(object shortcutFilename, out bool isFolder)
        {
            string resolvedFilename = string.Empty;
            isFolder = false;
#if TODO_LINUX
            try
            {
                string? pathOnly = Path.GetDirectoryName((string)shortcutFilename);
                string? filenameOnly = Path.GetFileName((string)shortcutFilename);

                Shell shell = new();
                Folder folder = shell.NameSpace(pathOnly);
                if (folder == null)
                {
                    Log.Info($"{nameof(GetShortcutFileNamePath)} folder == null for path:'{shortcutFilename}'");
                    return resolvedFilename;
                }

                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem == null)
                {
                    Log.Info($"{nameof(GetShortcutFileNamePath)} folderItem == null for path:'{shortcutFilename}'");
                    return resolvedFilename;
                }

                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                isFolder = link.Target.IsFolder;
                if (string.IsNullOrEmpty(link.Path))
                {
                    // https://github.com/Hofknecht/SystemTrayMenu/issues/242
                    // do not set CLSID key (GUID) shortcuts as resolvedFilename
                    if (!link.Target.Path.Contains("::{"))
                    {
                        resolvedFilename = link.Target.Path;
                    }
                }
                else
                {
                    resolvedFilename = link.Path;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // https://stackoverflow.com/questions/2934420/why-do-i-get-e-accessdenied-when-reading-public-shortcuts-through-shell32
                // e.g. Administrative Tools\Component Services.lnk which can not be resolved, do not spam the logfile in this case
            }
            catch (Exception ex)
            {
                Log.Warn($"shortcutFilename:'{shortcutFilename}'", ex);
            }
#endif
            return resolvedFilename;
        }
    }
}