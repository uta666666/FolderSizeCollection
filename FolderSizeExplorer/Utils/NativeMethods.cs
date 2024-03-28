using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderSizeExplorer.Utils
{

    internal static class NativeMethods
    {
        /// <summary>
        /// 指定した名前と一致する名前と属性を持つファイルまたはサブディレクトリをディレクトリで検索します。
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="fInfoLevelId"></param>
        /// <param name="lpFindFileData"></param>
        /// <param name="fSearchOp"></param>
        /// <param name="lpSearchFilter"></param>
        /// <param name="dwAdditionalFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFindFileHandle FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            FIND_FIRST_EX dwAdditionalFlags);

        /// <summary>
        /// FindFirstFile 関数、FindFirstFileEx 関数、または FindFirstFileTransacted 関数への以前の呼び出しからファイル検索を続行します。
        /// </summary>
        /// <param name="hFindFile"></param>
        /// <param name="lpFindFileData"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, BestFitMapping = false)]
        internal static extern bool FindNextFile(SafeFindFileHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

        /// <summary>
        /// FindFirstFile、FindFirstFileEx、FindFirstFileNameW、FindFirstFileNameTransactedW、FindFirstFileTransacted、FindFirstStreamTransactedW、または FindFirstStreamW 関数によって開かれたファイル検索ハンドルを閉じます。
        /// </summary>
        /// <param name="handle"></param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        internal static extern bool FindClose(IntPtr handle);

        /// <summary>
        /// 返されるデータの情報レベルを指定するために、FindFirstFileEx 関数で使用する値を定義します。
        /// </summary>
        internal enum FINDEX_INFO_LEVELS
        {
            Standard = 0,
            Basic = 1,
            FindExInfo = 2
        }

        /// <summary>
        /// 実行するフィルター処理の種類を指定するために、FindFirstFileEx 関数で使用する値を定義します。
        /// </summary>
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

        /// <summary>
        /// FindFirstFile、FindFirstFileEx、または FindNextFile 関数で検出されたファイルに関する情報が含まれます。
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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



        /// <summary>
        /// ファイル、フォルダー、ディレクトリ、ドライブ ルートなど、ファイル システム内のオブジェクトに関する情報を取得します。
        /// </summary>
        /// <param name="pszPath"></param>
        /// <param name="dwFileAttributes"></param>
        /// <param name="psfi"></param>
        /// <param name="cbSizeFileInfo"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得

        public struct IconSize
        {
            public const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
            public const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン
        }

        /// <summary>
        /// ファイル オブジェクトに関する情報を格納します。
        /// </summary>
        public struct SHFILEINFO
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
