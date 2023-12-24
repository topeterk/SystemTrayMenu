// <copyright file="FolderDialog.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface.FolderBrowseDialog
{
    using System;
    using System.Threading.Tasks;
#if !AVALONIA
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Interop;
    using SystemTrayMenu.DllImports;
    using SystemTrayMenu.Utilities;
#else
    using System.Collections.Generic;
    using Avalonia.Controls;
    using Avalonia.Platform.Storage;
    using Window = SystemTrayMenu.Utilities.Window;
#endif

    public class FolderDialog : IDisposable
    {
        private bool isDisposed;

        ~FolderDialog() // the finalizer
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets /sets folder in which dialog will be open.
        /// </summary>
        public string? InitialFolder { get; set; }

        /// <summary>
        /// Gets or sets /sets directory in which dialog will be open
        /// if there is no recent directory available.
        /// </summary>
        public string? DefaultFolder { get; set; }

        /// <summary>
        /// Gets or sets selected folder.
        /// </summary>
        public string? Folder { get; set; }

        /// <summary>
        /// Shows the file dialog and requests user interaction.
        /// </summary>
        /// <param name="owner">The window the dialog is assigned to.</param>
        /// <returns>True is returned on successful user interaction and when not cancelled by the user otherwise false is returned.</returns>
#if TODO_AVALONIA
        [SupportedOSPlatform("windows")]
#endif
        public async Task<bool> ShowDialog(Window owner)
        {
#if TODO_AVALONIA
            NativeMethods.IFileDialog frm = (NativeMethods.IFileDialog)new NativeMethods.FileOpenDialogRCW();
            frm.GetOptions(out uint options);
            options |= NativeMethods.FOS_PICKFOLDERS |
                       NativeMethods.FOS_FORCEFILESYSTEM |
                       NativeMethods.FOS_NOVALIDATE |
                       NativeMethods.FOS_NOTESTFILECREATE |
                       NativeMethods.FOS_DONTADDTORECENT;
            frm.SetOptions(options);
            if (InitialFolder != null)
            {
                Guid riid = new("43826D1E-E718-42EE-BC55-A1E261C37BFE"); // IShellItem
                if (NativeMethods.SHCreateItemFromParsingName(
                    InitialFolder,
                    IntPtr.Zero,
                    ref riid,
                    out NativeMethods.IShellItem directoryShellItem) == NativeMethods.S_OK)
                {
                    frm.SetFolder(directoryShellItem);
                }
            }

            if (DefaultFolder != null)
            {
                Guid riid = new("43826D1E-E718-42EE-BC55-A1E261C37BFE"); // IShellItem
                if (NativeMethods.SHCreateItemFromParsingName(
                    DefaultFolder,
                    IntPtr.Zero,
                    ref riid,
                    out NativeMethods.IShellItem directoryShellItem) == NativeMethods.S_OK)
                {
                    frm.SetDefaultFolder(directoryShellItem);
                }
            }

            if (frm.Show(new WindowInteropHelper(owner).Handle) == NativeMethods.S_OK)
            {
                try
                {
                    if (frm.GetResult(out NativeMethods.IShellItem shellItem) == NativeMethods.S_OK)
                    {
                        if (shellItem.GetDisplayName(
                            NativeMethods.SIGDN_FILESYSPATH,
                            out IntPtr pszString) == NativeMethods.S_OK)
                        {
                            if (pszString != IntPtr.Zero)
                            {
                                try
                                {
                                    Folder = Marshal.PtrToStringAuto(pszString);
                                    return await Task.FromResult(true);
                                }
                                finally
                                {
                                    Marshal.FreeCoTaskMem(pszString);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Folder Dialog failed", ex);
                }
            }
#else
            IStorageProvider? storageProvider = TopLevel.GetTopLevel(owner)?.StorageProvider;
            if (storageProvider is not null)
            {
                IStorageFolder? initialFolder = null;
                if (!string.IsNullOrEmpty(InitialFolder))
                {
                    // Use given inital folder when valid.
                    initialFolder = await storageProvider.TryGetFolderFromPathAsync(InitialFolder);
                }

                if (initialFolder is null)
                {
                    // Fallback to documents folder when no initial folder is given or valid.
                    // When this fails initial folder will be null and the dialogs opens on the storage provider's default location.
                    initialFolder = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
                }

                IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(new()
                {
                    Title = "Select Folder",
                    AllowMultiple = false,
                    SuggestedStartLocation = initialFolder,
                });

                if (folders.Count == 1)
                {
                    Folder = folders[0].Path.LocalPath;
                    return true;
                }
            }
#endif

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
            }

            isDisposed = true;
        }
    }
}
