using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderSizeCollection.Models
{

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFindFileHandle FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            FIND_FIRST_EX dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, BestFitMapping = false)]
        internal static extern bool FindNextFile(SafeFindFileHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        internal static extern bool FindClose(IntPtr handle);

        internal enum FINDEX_INFO_LEVELS
        {
            Standard = 0,
            Basic = 1
        }

        internal enum FINDEX_SEARCH_OPS
        {
            SearchNameMatch = 0,
            SearchLimitToDirectories = 1,
            SearchLimitToDevices = 2
        }

        internal enum FIND_FIRST_EX
        {
            CaseSensitive = 1,
            LargeFetch = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        [BestFitMapping(false)]
        internal struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILE_TIME ftCreationTime;
            public FILE_TIME ftLastAccessTime;
            public FILE_TIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;

            internal bool IsRelative => cFileName == "." || cFileName == "..";
            internal bool IsFile => (dwFileAttributes & FileAttributes.Directory) == 0;
            internal bool IsDirectory => (dwFileAttributes & FileAttributes.Directory) != 0;

            internal DateTime ToCreationTimeUtc => DateTime.FromFileTimeUtc(ftCreationTime.ToTicks());
            internal DateTime ToLastAccessTimeUtc => DateTime.FromFileTimeUtc(ftLastAccessTime.ToTicks());
            internal DateTime ToLastWriteTimeUtc => DateTime.FromFileTimeUtc(ftLastWriteTime.ToTicks());

            public override string ToString() => cFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME
        {
            public FILE_TIME(long fileTime)
            {
                ftTimeLow = (uint)fileTime;
                ftTimeHigh = (uint)(fileTime >> 32);
            }

            public long ToTicks()
            {
                return ((long)ftTimeHigh << 32) + ftTimeLow;
            }

            internal uint ftTimeLow;
            internal uint ftTimeHigh;
        }

        internal sealed class SafeFindFileHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal SafeFindFileHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return FindClose(handle);
            }
        }



        // SHGetFileInfo関数
        [DllImport("shell32.dll")]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // SHGetFileInfo関数で使用するフラグ
        internal const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
        internal const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
        internal const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン

        // SHGetFileInfo関数で使用する構造体
        internal struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }
}
