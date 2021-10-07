using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static FolderSizeCollection.Models.DirectoryUtil;

namespace FolderSizeCollection.Models
{
    public static partial class DirectoryUtil
    {
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, true, false);
        }

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, false, true);
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, true, true);
        }

        public static IEnumerable<FileData> EnumerateFilesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, true, false);
        }

        public static IEnumerable<FileData> EnumerateDirectoriesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, false, true);
        }

        public static IEnumerable<FileData> EnumerateFileSystemEntriesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, true, true);
        }
    }

    public class FileData
    {
        public FileAttributes Attributes { get; }
        public bool IsFile => (Attributes & FileAttributes.Directory) == 0;
        public bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;
        public DateTime CreationTimeUtc { get; }
        public DateTime CreationTime => CreationTimeUtc.ToLocalTime();
        public DateTime LastAccessTimeUtc { get; }
        public DateTime LastAccesTime => LastAccessTimeUtc.ToLocalTime();
        public DateTime LastWriteTimeUtc { get; }
        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();
        public long Length { get; }
        public string Name { get; }
        public string FullName { get; }

        internal FileData(ref string fullName, ref NativeMethods.WIN32_FIND_DATA findData)
        {
            Attributes = findData.dwFileAttributes;
            CreationTimeUtc = findData.ToCreationTimeUtc;
            LastAccessTimeUtc = findData.ToLastAccessTimeUtc;
            LastWriteTimeUtc = findData.ToLastWriteTimeUtc;
            Length = ((long)findData.nFileSizeHigh << 32) + findData.nFileSizeLow;
            Name = findData.cFileName;
            FullName = fullName;
        }

        public override string ToString() => Name;
    }
}
