using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSizeExplorer.Models
{
    public class FileDataComparer : IComparer<FileData>
    {
        public FileDataComparer()
        {
        }

        public FileDataComparer(string propertyName, bool ascending)
        {
            PropertyName = propertyName;
            Ascending = ascending;
        }


        public bool Ascending { get; set; } = true;

        public string PropertyName { get; set; } = "Name";

        public int Compare(FileData? x, FileData? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return Ascending ? -1 : 1;
            }
            else if (y == null)
            {
                return Ascending ? 1 : -1;
            }

            switch (PropertyName)
            {
                case "Name":
                    return Ascending ? string.Compare(x.Name, y.Name) : string.Compare(y.Name, x.Name);
                case "Length":
                    return Ascending ? x.Length.CompareTo(y.Length) : y.Length.CompareTo(x.Length);
                case "CreationTime":
                    return Ascending ? x.CreationTime.CompareTo(y.CreationTime) : y.CreationTime.CompareTo(x.CreationTime);
                case "LastAccessTime":
                    return Ascending ? x.LastAccessTime.CompareTo(y.LastAccessTime) : y.LastAccessTime.CompareTo(x.LastAccessTime);
                case "LastWriteTime":
                    return Ascending ? x.LastWriteTime.CompareTo(y.LastWriteTime) : y.CreationTime.CompareTo(x.LastWriteTime);
                case "FilesCount":
                    return Ascending ? x.FilesCount.CompareTo(y.FilesCount) : y.FilesCount.CompareTo(x.FilesCount);
                case "SubDirectoriesCount":
                    return Ascending ? x.SubDirectoriesCount.CompareTo(y.SubDirectoriesCount) : y.SubDirectoriesCount.CompareTo(x.SubDirectoriesCount);
                case "FilesCountCurrent":
                    return Ascending ? x.FilesCountCurrent.CompareTo(y.FilesCountCurrent) : y.FilesCountCurrent.CompareTo(x.FilesCountCurrent);
                case "SubDirectoriesCountCurrent":
                    return Ascending ? x.SubDirectoriesCountCurrent.CompareTo(y.SubDirectoriesCountCurrent) : y.SubDirectoriesCountCurrent.CompareTo(x.SubDirectoriesCountCurrent);
                default:
                    throw new ArgumentException("Invalid PropertyName");
            }
        }
    }
}
