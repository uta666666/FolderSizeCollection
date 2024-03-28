using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FolderSizeExplorer.Models
{
    public static partial class DirectoryUtil
    {
        /// <summary>
        /// ファイル名を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesName(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, true, false);
        }

        /// <summary>
        /// フォルダ名を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateDirectoriesName(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, false, true);
        }

        /// <summary>
        /// ファイル名、フォルダ名を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFileSystemEntriesName(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFullName(path, searchPattern, searchOption, true, true);
        }

        /// <summary>
        /// ファイル情報を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<FileData> EnumerateFilesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, true, false);
        }

        /// <summary>
        /// フォルダ情報を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<FileData> EnumerateDirectoriesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, false, true);
        }

        /// <summary>
        /// ファイル情報、フォルダ情報を列挙する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<FileData> EnumerateFileSystemEntriesData(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFileData(path, searchPattern, searchOption, true, true);
        }
    }

}
