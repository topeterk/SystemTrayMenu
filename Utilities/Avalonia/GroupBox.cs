// <copyright file="GroupBox.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    internal class GroupBox : Avalonia.Controls.Border
    {
        internal string Header { get; set; } = string.Empty;

        internal object Content { get; set; } = string.Empty; // TODO: Implement GroupBox rather just using a border
    }
}
#endif
