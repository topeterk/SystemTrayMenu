// <copyright file="Unix.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// ===============================================================================
// MIT License
//
// Copyright (c) 2024-2024 Peter Kirmeier
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable CA2101 // False positive as Linux P/Invokes are most likely UTF8 encoded which is Ansi, but warning "forces" to set Unicode instead (whichs is wrong)

#if !WINDOWS
namespace SystemTrayMenu.DllImports
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal static partial class NativeMethods
    {
        internal const int FNM_NOMATCH = 1; /* Value returned by fnmatch if STRING does not match PATTERN. */

        // Origin: https://github.com/sebastinas/yafc/blob/2a4bbd7f3ed445a18de2006a0b780234448ec713/lib/fnmatch_.h#L36-L43
        [Flags]
        internal enum FileNameMatchFlags : int
        {
            /// <summary>
            /// No wildcard can ever match `/'.
            /// </summary>
            FNM_PATHNAME = 1,

            /// <summary>
            /// Backslashes don't quote special chars.
            /// </summary>
            FNM_NOESCAPE = 2,

            /// <summary>
            /// Leading `.' is matched only explicitly.
            /// </summary>
            FNM_PERIOD = 4,

            /// <summary>
            /// Ignore `/...' after a match.
            /// </summary>
            FNM_LEADING_DIR = 8,

            /// <summary>
            /// Compare without regard to case.
            /// </summary>
            FNM_CASEFOLD = 16,

            /// <summary>
            /// Use ksh-like extended matching.
            /// </summary>
            FNM_EXTMATCH = 32,
        }

        /// <summary>
        /// https://man7.org/linux/man-pages/man3/fnmatch.3.html .
        /// </summary>
        /// <param name="pattern">Shell wildcard pattern (see glob(7)).</param>
        /// <param name="str">The string to be checked for matches of pattern.</param>
        /// <param name="flags">Behavior modifiers.</param>
        /// <returns>Zero if string matches pattern, FNM_NOMATCH if there is no match or another nonzero value if there is an error.</returns>
        [UnsupportedOSPlatform("windows")]
        [DllImport("libc", EntryPoint = "fnmatch", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = false)]
        internal static extern int fnmatch(string pattern, string str, FileNameMatchFlags flags);
    }
}
#endif
