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

        private const int R_OK = 4; /* Test for read permission. */
        private const int W_OK = 2; /* Test for write permission. */
        private const int X_OK = 1; /* Test for execute permission. */
        private const int F_OK = 0; /* Test for existence. */
#if TODO_REMOVE
        // user permissions
        private const int S_IRUSR = 0x100;
        private const int S_IWUSR = 0x80;
        private const int S_IXUSR = 0x40;

        // group permission
        private const int S_IRGRP = 0x20;
        private const int S_IWGRP = 0x10;
        private const int S_IXGRP = 0x8;

        // other permissions
        private const int S_IROTH = 0x4;
        private const int S_IWOTH = 0x2;
        private const int S_IXOTH = 0x1;
#endif

        // Temporary copy until Upgrade to .Net 7 or higher
        // Origin: https://github.com/dotnet/runtime/blob/c3e5ce97a29317d3dab98d09c310daf11a2260c7/src/libraries/System.Private.CoreLib/src/System/IO/UnixFileMode.cs
        [Flags]
        internal enum UnixFileMode
        {
            /// <summary>
            /// No permissions.
            /// </summary>
            None = 0,

            /// <summary>
            /// Execute permission for others.
            /// </summary>
            OtherExecute = 1,

            /// <summary>
            /// Write permission for others.
            /// </summary>
            OtherWrite = 2,

            /// <summary>
            /// Read permission for others.
            /// </summary>
            OtherRead = 4,

            /// <summary>
            /// Execute permission for group.
            /// </summary>
            GroupExecute = 8,

            /// <summary>
            /// Write permission for group.
            /// </summary>
            GroupWrite = 16,

            /// <summary>
            /// Read permission for group.
            /// </summary>
            GroupRead = 32,

            /// <summary>
            /// Execute permission for owner.
            /// </summary>
            UserExecute = 64,

            /// <summary>
            /// Write permission for owner.
            /// </summary>
            UserWrite = 128,

            /// <summary>
            /// Read permission for owner.
            /// </summary>
            UserRead = 256,

            /// <summary>
            /// Sticky bit permission.
            /// </summary>
            StickyBit = 512,

            /// <summary>
            /// Set Group permission.
            /// </summary>
            SetGroup = 1024,

            /// <summary>
            /// Set User permission.
            /// </summary>
            SetUser = 2048,
        }

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

#if TODO_REMOVE
        [Flags]
        private enum FileStatusFlags
        {
            None = 0,
            HasBirthTime = 1,
        }
#endif

        // TODO: .Net7 and above use File.GetUnixFileMode istead

        /// <summary>
        /// Temporary wrapper pre .Net 7 for https://learn.microsoft.com/de-de/dotnet/api/system.io.file.getunixfilemode .
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The UnixFileMode of the file on the path.</returns>
        [UnsupportedOSPlatform("windows")]
        internal static UnixFileMode GetUnixFileMode(string path)
        {
#if TODO_REMOVE
            stat fileinfo;
            if (LStat(path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }), out fileinfo) == 0)
            {
                return (UnixFileMode)fileinfo.st_mode;
            }

            throw new PlatformNotSupportedException($"LStat for {path} failed.");
#else
            UnixFileMode result = UnixFileMode.None;
            if (access(path, X_OK) == 0)
            {
                result |= UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            }

            return result;
#endif
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

        /// <summary>
        /// https://man7.org/linux/man-pages/man2/access.2.html .
        /// </summary>
        /// <param name="pathname">Path to the file or directory.</param>
        /// <param name="mode">Mode to test access with.</param>
        /// <returns>On success zero is returned. On error -1 is returned.</returns>
        [UnsupportedOSPlatform("windows")]
        [DllImport("libc", EntryPoint = "access", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int access(string pathname, int mode);

#if TODO_REMOVE
        /// <summary>
        /// https://linux.die.net/man/2/lstat .
        /// </summary>
        /// <param name="path">Path to the file or directory.</param>
        /// <param name="buf">Buffer that is filled with information.</param>
        /// <returns>On success, zero is returned. On error, -1 is returned, and errno is set appropriately.</returns>
        [UnsupportedOSPlatform("windows")]
        [DllImport("libc", EntryPoint = "lstat", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int LStat(string path, out stat buf);

        private struct timespec
        {
            internal IntPtr tv_sec;
            internal IntPtr tv_nsec;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct stat
        { /* when _DARWIN_FEATURE_64_BIT_INODE is defined */
            public uint st_dev;
            public ushort st_mode;
            public ushort st_nlink;
            public ulong st_ino;
            public uint st_uid;
            public uint st_gid;
            public uint st_rdev;
            public timespec st_atimespec;
            public timespec st_mtimespec;
            public timespec st_ctimespec;
            public timespec st_birthtimespec;
            public ulong st_size;
            public ulong st_blocks;
            public uint st_blksize;
            public uint st_flags;
            public uint st_gen;
            public uint st_lspare;
            public ulong st_qspare_1;
            public ulong st_qspare_2;
        }

        private static class FileTypes
        {
            internal const int S_IFMT = 0xF000;
            internal const int S_IFIFO = 0x1000;
            internal const int S_IFCHR = 0x2000;
            internal const int S_IFDIR = 0x4000;
            internal const int S_IFBLK = 0x6000;
            internal const int S_IFREG = 0x8000;
            internal const int S_IFLNK = 0xA000;
            internal const int S_IFSOCK = 0xC000;
        }
#endif
    }
}
#endif
