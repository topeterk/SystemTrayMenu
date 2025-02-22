﻿// <copyright file="GenerateDriveShortcuts.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
#if WINDOWS
    using IWshRuntimeLibrary;
#endif

    internal class GenerateDriveShortcuts
    {
        public static void Start()
        {
            List<char> driveNamesToRemove = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().ToList();

            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in driveInfos)
            {
                driveNamesToRemove.Remove(driveInfo.Name[0]);
                string linkPath = GetLinkPathFromDriveName(driveInfo.Name[..1]);
                if (!System.IO.File.Exists(linkPath))
                {
                    CreateShortcut(linkPath, driveInfo.Name);
                }
            }

            foreach (char driveName in driveNamesToRemove)
            {
                string possibleShortcut = GetLinkPathFromDriveName(driveName.ToString());

                try
                {
                    System.IO.File.Delete(possibleShortcut);
                }
                catch (Exception ex)
                {
                    Log.Warn($"Could not delete shortcut at path:'{possibleShortcut}'", ex);
                }
            }
        }

        private static void CreateShortcut(string linkPath, string targetPath)
        {
#if TODO_LINUX
            WshShell shell = new();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(linkPath);
            shortcut.Description = "Generated by SystemTrayMenu";
            shortcut.TargetPath = targetPath;

            try
            {
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not create shortcut at path:'{linkPath}'", ex);
            }
#endif
        }

        private static string GetLinkPathFromDriveName(string driveName)
        {
            return Path.Combine(Config.Path, $" {driveName}       .lnk");
        }
    }
}
