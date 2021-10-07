using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderSizeCollection.Models
{
    public static partial class DirectoryUtil
    {

        private interface ISelector<T>
        {
            T Create(ref string fullName, ref NativeMethods.WIN32_FIND_DATA findData);
        }

        private class FullNameSelector : ISelector<string>
        {
            public string Create(ref string fullName, ref NativeMethods.WIN32_FIND_DATA findData) => fullName;
        }

        private class FileDataSelector : ISelector<FileData>
        {
            public FileData Create(ref string fullName, ref NativeMethods.WIN32_FIND_DATA findData) => new FileData(ref fullName, ref findData);
        }

        private static IEnumerable<string> EnumerateFullName(string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs)
        {
            return Enumerate(path, searchPattern, searchOption, includeFiles, includeDirs, new FullNameSelector());
        }

        private static IEnumerable<FileData> EnumerateFileData(string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs)
        {
            return Enumerate(path, searchPattern, searchOption, includeFiles, includeDirs, new FileDataSelector());
        }

        private static IEnumerable<T> Enumerate<T>(string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs, ISelector<T> selector)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (searchPattern == null)
                throw new ArgumentNullException(nameof(searchPattern));
            if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
                throw new ArgumentOutOfRangeException(nameof(searchOption));

            return EnumerateCore(Path.GetFullPath(path).TrimEnd('\\'), searchPattern, searchOption, includeFiles, includeDirs, selector);
        }

        private static IEnumerable<T> EnumerateCore<T>(string dir, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs, ISelector<T> selector)
        {
            // extend MAX_PATH
            var search = (dir.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
                                ? @"\\?\UNC\" + dir.Substring(2)
                                : @"\\?\" + dir) + @"\" + searchPattern;

            Queue<string> subDirs = null;

            using (var fileHandle = NativeMethods.FindFirstFileEx(search,
                                                                  NativeMethods.FINDEX_INFO_LEVELS.Basic,
                                                                  out var findData,
                                                                  NativeMethods.FINDEX_SEARCH_OPS.SearchNameMatch,
                                                                  IntPtr.Zero,
                                                                  NativeMethods.FIND_FIRST_EX.LargeFetch))
            {
                if (fileHandle.IsInvalid) yield break;

                do
                {
                    if (findData.IsRelative) continue;

                    var path = dir + @"\" + findData.cFileName;

                    if (findData.IsFile)
                    {
                        if (includeFiles)
                            yield return selector.Create(ref path, ref findData);
                    }
                    else if (findData.IsDirectory)
                    {
                        if (includeDirs)
                            yield return selector.Create(ref path, ref findData);

                        if (searchOption == SearchOption.AllDirectories)
                        {
                            subDirs = subDirs ?? new Queue<string>();
                            subDirs.Enqueue(path);
                        }
                    }

                } while (NativeMethods.FindNextFile(fileHandle, out findData));
            }

            if (subDirs == null) yield break;

            while (subDirs.Count > 0)
            {
                foreach (var path in EnumerateCore(subDirs.Dequeue(), searchPattern, searchOption, includeFiles, includeDirs, selector))
                    yield return path;
            }
        }
    }
}
