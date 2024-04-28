// <copyright file="FolderOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Utilities
{
    using System;
    using System.IO;
#if WINDOWS
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Shell32;
#endif

    internal static class FolderOptions
    {
        private static bool hideHiddenEntries = false;
        private static bool hideSystemEntries = false;
#if WINDOWS
        private static IShellDispatch4? iShellDispatch4;
#endif

        internal static void Initialize()
        {
#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    iShellDispatch4 = (IShellDispatch4?)Activator.CreateInstance(
                        Type.GetTypeFromProgID("Shell.Application")!);

                    // Using SHGetSetSettings would be much better in performance but the results are not accurate.
                    // We have to go for the shell interface in order to receive the correct settings:
                    // https://docs.microsoft.com/en-us/windows/win32/shell/ishelldispatch4-getsetting
                    const int SSF_SHOWALLOBJECTS = 0x00000001;
                    hideHiddenEntries = !(iShellDispatch4?.GetSetting(SSF_SHOWALLOBJECTS) ?? false);

                    const int SSF_SHOWSUPERHIDDEN = 0x00040000;
                    hideSystemEntries = !(iShellDispatch4?.GetSetting(SSF_SHOWSUPERHIDDEN) ?? false);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException ||
                        ex is NotSupportedException ||
                        ex is TargetInvocationException ||
                        ex is MethodAccessException ||
                        ex is MemberAccessException ||
                        ex is InvalidComObjectException ||
                        ex is MissingMethodException ||
                        ex is COMException ||
                        ex is TypeLoadException)
                    {
                        Log.Warn("Get Shell COM instance failed", ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Read the flags if given path points to a file or directory that is marked as hidden.
        /// </summary>
        /// <param name="path">Path to file or directory.</param>
        /// <param name="hasHiddenFlag">Returns if the entry is marked as hidden.</param>
        /// <param name="hasOmittedFlag">Returns if the entry shall be ommitted.</param>
        internal static void ReadHiddenAttributes(string path, out bool hasHiddenFlag, out bool hasOmittedFlag)
        {
            hasOmittedFlag = false;
            hasHiddenFlag = false;

            try
            {
                FileAttributes attributes = File.GetAttributes(path);
                hasHiddenFlag = attributes.HasFlag(FileAttributes.Hidden);
                if (Properties.Settings.Default.SystemSettingsShowHiddenFiles)
                {
                    bool hasSystemFlag = attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System);

                    if ((hideHiddenEntries && hasHiddenFlag) ||
                        (hideSystemEntries && hasSystemFlag))
                    {
                        hasOmittedFlag = true;
                    }
                }
                else if (hasHiddenFlag && Properties.Settings.Default.NeverShowHiddenFiles)
                {
                    hasOmittedFlag = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"path:'{path}'", ex);
            }
        }
    }
}
