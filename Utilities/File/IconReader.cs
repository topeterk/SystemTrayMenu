// <copyright file="IconReader.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// see also: https://www.codeproject.com/Articles/2532/Obtaining-and-managing-file-and-folder-icons-using.

namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
#if WINDOWS
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using SystemTrayMenu.Helpers;
#endif
#if !AVALONIA
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;
#else
    using Application = Avalonia.Application;
#endif
    using SystemTrayMenu.DllImports;

    /// <summary>
    /// Provides static methods to read system icons for folders and files.
    /// </summary>
    internal static class IconReader
    {
        internal static readonly BitmapSource LoadingImage = (BitmapSource)Application.Current.Resources["LoadingIconImage"];
        internal static readonly BitmapSource NotFoundImage = (BitmapSource)Application.Current.Resources["NotFoundIconImage"];
        private static readonly BitmapSource OverlayImage = (BitmapSource)Application.Current.Resources["LinkArrowIconImage"];

        private static readonly ConcurrentDictionary<string, BitmapSource> IconDictPersistent = new();
        private static readonly ConcurrentDictionary<string, BitmapSource> IconDictCache = new();
        private static readonly BlockingCollection<Action> IconFactoryQueueSTA = new();
        private static readonly List<Thread> IconFactoryThreadPoolSTA = new(16);

        internal static void Startup()
        {
            for (int i = 0; i < IconFactoryThreadPoolSTA.Capacity; i++)
            {
                Thread thread = new(IconFactoryWorkerSTA)
                {
                    Name = "IconFactory STA #" + i.ToString(),
                };

                if (OperatingSystem.IsWindows())
                {
                    thread.SetApartmentState(ApartmentState.STA);
                }

                thread.Start();
                IconFactoryThreadPoolSTA.Add(thread);
            }

            static void IconFactoryWorkerSTA()
            {
                while (true)
                {
                    try
                    {
                        IconFactoryQueueSTA.Take()();
                    }
                    catch (ThreadInterruptedException)
                    {
                        // Shutdown
                        break;
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static void Shutdown()
        {
            foreach (Thread thread in IconFactoryThreadPoolSTA)
            {
                thread.Interrupt();
            }

            foreach (Thread thread in IconFactoryThreadPoolSTA)
            {
                thread.Join(400);
            }

            IconFactoryThreadPoolSTA.Clear();
        }

        internal static void ClearCacheWhenLimitReached()
        {
            if (IconDictCache.Count > Properties.Settings.Default.ClearCacheIfMoreThanThisNumberOfItems)
            {
                IconDictCache.Clear();
                GC.Collect();
            }
        }

        internal static void RemoveIconFromCache(string path) => IconDictPersistent.Remove(path, out _);

        /// <summary>
        /// Loads an Icon requested by parameters.
        /// </summary>
        /// <param name="isFolder">Icon is a folder or file icon.</param>
        /// <param name="path">Path to the file or directory entry.</param>
        /// <param name="resolvedPath">Path to the file or directory entry which 'path' is pointing at.</param>
        /// <param name="linkOverlay">Apply the link overlay to the icon.</param>
        /// <param name="persistentEntry">Load from or into persistent cache.</param>
        /// <param name="onIconLoaded">Callback called when icon got loaded.</param>
        /// <param name="synchronousLoading">Force synchronous loading. e.g. during preloading on startup.</param>
        /// <returns>True = Icon was loaded synchronously, False = Icon will be loaded in the background.</returns>
        internal static bool GetIconAsync(
            bool isFolder,
            string path,
            string resolvedPath,
            bool linkOverlay,
            bool persistentEntry,
            Action<BitmapSource> onIconLoaded,
            bool synchronousLoading)
        {
            string key = path;

            if (!isFolder)
            {
                string extension = Path.GetExtension(path);
                if (IsExtensionWithSameIcon(extension))
                {
                    // Generic file extension
                    key = extension + ":" + linkOverlay;
                    persistentEntry = true; // Always store them in persistent cache
                }
            }

            if (!DictIconCache(persistentEntry).TryGetValue(key, out BitmapSource? icon) &&
                !DictIconCache(!persistentEntry).TryGetValue(key, out icon))
            {
                if (synchronousLoading)
                {
                    icon = DictIconCache(persistentEntry).GetOrAdd(key, FactoryIconSTA);
                }
                else
                {
                    IconFactoryQueueSTA.Add(() =>
                    {
                        BitmapSource icon = DictIconCache(persistentEntry).GetOrAdd(key, FactoryIconSTA);
                        onIconLoaded(icon);
                    });

                    return false;
                }
            }

            onIconLoaded(icon);
            return true;

            BitmapSource FactoryIconSTA(string keyExtension)
            {
                return GetIconAsBitmapSourceSTA(path, resolvedPath, linkOverlay, isFolder);
            }
        }

#if WINDOWS
        [SupportedOSPlatform("Windows")]
        internal static Icon? GetRootFolderIcon(string path)
        {
            NativeMethods.SHFILEINFO shFileInfo = default;
            bool linkOverlay = false;
            bool largeIcon = false;
            uint flags = GetFlags(linkOverlay, largeIcon);
            uint attribute = NativeMethods.FileAttributeDirectory;
            IntPtr imageList = NativeMethods.Shell32SHGetFileInfo(path, attribute, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), flags);
            return TryGetIcon(path, linkOverlay, shFileInfo, imageList);
        }
#endif

        private static bool CacheGetOrAddIcon(
            string key,
            bool persistentEntry,
            Action<BitmapSource?> onIconLoaded,
            bool synchronousLoading,
            Func<string, BitmapSource> factory)
        {
            if (!DictIconCache(persistentEntry).TryGetValue(key, out BitmapSource? icon) &&
                !DictIconCache(!persistentEntry).TryGetValue(key, out icon))
            {
                if (synchronousLoading)
                {
                    icon = DictIconCache(persistentEntry).GetOrAdd(key, factory);
                }
                else
                {
                    IconFactoryQueueSTA.Add(() =>
                    {
                        BitmapSource icon = DictIconCache(persistentEntry).GetOrAdd(key, factory);
                        onIconLoaded(icon);
                    });

                    return false;
                }
            }

            onIconLoaded(icon);
            return true;
        }

#if !AVALONIA
        [SupportedOSPlatform("Windows")]
        private static BitmapSource? TryCreateBitmapSourceFromIcon(string path, Icon icon) => Application.Current.Dispatcher.Invoke(() =>
        {
            BitmapSource? bitmap = null;

            try
            {
                bitmap = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                bitmap.Freeze();
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to {nameof(TryCreateBitmapSourceFromIcon)}: {path} ", ex);
            }

            return bitmap;
        });
#endif

#if WINDOWS
        [SupportedOSPlatform("Windows")]
        private static BitmapSource? TryGetIconAsBitmapSourceSTA(string path, string resolvedPath, bool linkOverlay, bool isFolder)
        {
            BitmapSource? result = null;

            if (!isFolder &&
                Path.GetExtension(path).Equals(".ico", StringComparison.InvariantCultureIgnoreCase))
            {
                result = TryGetIconExtractAssociatedIcon(path);
            }
            else if (!isFolder && File.Exists(resolvedPath) &&
                Path.GetExtension(resolvedPath).Equals(".ico", StringComparison.InvariantCultureIgnoreCase))
            {
                result = TryGetIconExtractAssociatedIcon(resolvedPath);
                if (result != null && linkOverlay)
                {
                    result = ImagingHelper.CreateIconWithOverlay(result, OverlayImage);
                }
            }
            else
            {
                // This code block must run in an STA thread otherwise the results may be incorrectly loaded icons!
                NativeMethods.SHFILEINFO shFileInfo = default;
                bool largeIcon = // Note: large returns another folder icon than windows explorer
                    Scaling.Factor >= 1.25f ||
                    Scaling.FactorByDpi >= 1.25f ||
                    Properties.Settings.Default.IconSizeInPercent / 100f >= 1.25f;
                uint flags = GetFlags(linkOverlay, largeIcon);
                uint attribute = isFolder ? NativeMethods.FileAttributeDirectory : NativeMethods.FileAttributeNormal;
                IntPtr imageList = NativeMethods.Shell32SHGetFileInfo(path, attribute, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), flags);
                Icon? icon = TryGetIcon(path, linkOverlay, shFileInfo, imageList);
                if (icon != null)
                {
#if AVALONIA
                    result = icon;
#else
                    result = TryCreateBitmapSourceFromIcon(path, icon);
#endif
                    icon.Dispose();
                }
            }

            return result;
        }

        [SupportedOSPlatform("Windows")]
        private static BitmapSource? TryGetIconExtractAssociatedIcon(string path)
        {
            BitmapSource? result = null;
            Icon? iconOrNull = null;
            try
            {
                iconOrNull = Icon.ExtractAssociatedIcon(path);
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to {nameof(TryGetIconExtractAssociatedIcon)}: {path}", ex);
            }

            if (iconOrNull is not null)
            {
#if AVALONIA
                result = iconOrNull;
#else
                result = TryCreateBitmapSourceFromIcon(path, iconOrNull);
#endif
                iconOrNull.Dispose();
            }

            return result;
        }
#endif

        private static BitmapSource GetIconAsBitmapSourceSTA(string path, string resolvedPath, bool linkOverlay, bool isFolder)
        {
            BitmapSource? bitmapSource = null;

#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                bitmapSource = TryGetIconAsBitmapSourceSTA(path, resolvedPath, linkOverlay, isFolder);
            }
#else
            if (!OperatingSystem.IsWindows())
            {
                // TODO: Find mimetype from file and load associated icon
                //       See: https://unix.stackexchange.com/questions/123018/gtk-icons-for-special-files/200666#200666
                //            peter@ubuntu2204:/usr/share/icons$ gio info -a standard::icon ~/test.py
                //            uri: file:///home/peter/test.py
                //            local path: /home/peter/test.py
                //            unix mount: /dev/sda3 / ext4 rw,relatime,errors=remount-ro
                //            attributes:
                //            standard::icon: text-x-python, text-x-generic, text-x-python-symbolic, text-x-generic-symbolic
                //       Alternatively by looking up the type via /usr/share/mime/packages/*.xml

                // TODO: As intermediate solution, at least differentiate between folder/file/application
                //       /usr/share/icons/Yaru/32x32/status/image-loading.png
                //       /usr/share/icons/Yaru/32x32/mimetypes/text-x-generic.png
                //                                             application-x-executable.png
                //       /usr/share/icons/Yaru/32x32/places/folder.png
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    bitmapSource ??= FindThemeIcon("places", "folder");
                }
                else
                {
                    try
                    {
                        NativeMethods.UnixFileMode permissions = NativeMethods.GetUnixFileMode(path);
                        if (permissions.HasFlag(NativeMethods.UnixFileMode.UserExecute) ||
                            permissions.HasFlag(NativeMethods.UnixFileMode.GroupExecute) ||
                            permissions.HasFlag(NativeMethods.UnixFileMode.OtherExecute) )
                        {
                            bitmapSource ??= FindThemeIcon("mimetypes", "application-x-executable");
                        }
                    }
                    catch
                    {
                    }

                    bitmapSource ??= FindThemeIcon("mimetypes", "text-x-generic");
                }

                bitmapSource ??= FindThemeIcon("status", "image-loading");
            }
#endif

            bitmapSource ??= NotFoundImage;
#if !AVALONIA
            bitmapSource.Freeze(); // Make it accessible for any thread
#endif
            return bitmapSource;
        }

#if !WINDOWS
        private static BitmapSource? FindThemeIcon(string context, string iconName, int desiredSize = 48)
        {
            string? iconPath = FreeDesktop.FindThemeIcon(context, iconName, desiredSize);
            if (iconPath is not null)
            {
                try
                {
                    return new(iconPath);
                }
                catch
                {
                }
            }

            return null;
        }
#endif

        private static ConcurrentDictionary<string, BitmapSource> DictIconCache(bool checkPersistentFirst)
            => checkPersistentFirst ? IconDictPersistent : IconDictCache;

        private static bool IsExtensionWithSameIcon(string fileExtension)
        {
            bool isExtensionWithSameIcon = true;
            List<string> extensionsWithDiffIcons = new() { string.Empty, ".EXE", ".LNK", ".ICO", ".URL" };
            if (extensionsWithDiffIcons.Contains(fileExtension.ToUpperInvariant()))
            {
                isExtensionWithSameIcon = false;
            }

            return isExtensionWithSameIcon;
        }

        private static uint GetFlags(bool linkOverlay, bool largeIcon)
        {
            uint flags = NativeMethods.ShgfiIcon | NativeMethods.ShgfiSYSICONINDEX;
            if (linkOverlay)
            {
                flags += NativeMethods.ShgfiLINKOVERLAY;
            }

            if (!largeIcon)
            {
                flags += NativeMethods.ShgfiSMALLICON;
            }
            else
            {
                flags += NativeMethods.ShgfiLARGEICON;
            }

            return flags;
        }

#if TODO_LINUX
        [SupportedOSPlatform("Windows")]
        private static Icon? TryGetIcon(
            string path, bool linkOverlay, NativeMethods.SHFILEINFO shFileInfo, IntPtr imageList)
        {
            Icon? icon = null;
            if (imageList != IntPtr.Zero)
            {
                IntPtr hIcon;
                if (linkOverlay)
                {
                    hIcon = shFileInfo.hIcon;
                }
                else
                {
                    hIcon = NativeMethods.ImageList_GetIcon(imageList, shFileInfo.iIcon, NativeMethods.IldTransparent);
                }

                try
                {
                    // Note: Destroying hIcon after FromHandle will invalidate the Icon, so we do NOT destroy it, despite the request from documentation:
                    //       https://learn.microsoft.com/en-us/dotnet/api/system.drawing.icon.fromhandle?view=dotnet-plat-ext-7.0
                    //       Reason is https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Icon.cs,555 (FromHandle)
                    //       It is not taking over the ownership, data will be deleted upon destroying the original icon, so a clone is required.
                    //       With Clone we actually get a new handle, so we can free up the original handle without killing our copy.
                    //       Using Clone will also restore the ownership of the new icon handle, so we do not have to call DestroyIcon on it by ourself.
                    icon = (Icon)Icon.FromHandle(hIcon).Clone();
                }
                catch (Exception ex)
                {
                    Log.Warn($"path:'{path}'", ex);
                }

                if (!linkOverlay)
                {
                    _ = NativeMethods.User32DestroyIcon(hIcon);
                }

                _ = NativeMethods.User32DestroyIcon(shFileInfo.hIcon);
            }

            return icon;
        }
#endif
    }
}
