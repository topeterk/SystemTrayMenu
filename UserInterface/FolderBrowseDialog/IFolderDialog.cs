// <copyright file="IFolderDialog.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface.FolderBrowseDialog
{
#if WINDOWS
    using System.Windows;
#else
    using SystemTrayMenu.Utilities;
#endif

    public interface IFolderDialog
    {
        string? InitialFolder { get; set; }

        string? DefaultFolder { get; set; }

        string? Folder { get; set; }

        bool ShowDialog(Window owner);
    }
}
