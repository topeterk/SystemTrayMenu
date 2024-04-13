// <copyright file="WindowsExplorerSort.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Helpers
{
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using SystemTrayMenu.DllImports;

    internal class WindowsExplorerSort : IComparer<string?>
    {
        [SupportedOSPlatform("Windows")]
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
