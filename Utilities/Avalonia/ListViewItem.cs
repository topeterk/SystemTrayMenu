// <copyright file="ListViewItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using SystemTrayMenu.DataClasses;

    internal class ListViewItem : RowData
    {
        internal ListViewItem(bool isFolder, bool isAdditionalItem, int level, string path)
            : base(isFolder, isAdditionalItem, level, path)
        {
        }

        internal double ActualHeight => 60D; // TODO: Replace dummy value

        internal RowData Content => this;
    }
}
#endif
