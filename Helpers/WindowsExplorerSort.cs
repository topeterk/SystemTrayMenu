﻿// <copyright file="WindowsExplorerSort.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if TODO_LINUX
namespace SystemTrayMenu.Helpers
{
    using System.Collections.Generic;
    using SystemTrayMenu.DllImports;

    internal class WindowsExplorerSort : IComparer<string?>
    {
        public int Compare(string? x, string? y)
        {
            if (x != null && y != null)
            {
                return NativeMethods.ShlwapiStrCmpLogicalW(x, y);
            }

            return 0;
        }
    }
}
#endif
