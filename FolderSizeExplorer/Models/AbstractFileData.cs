using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSizeExplorer.Models
{
    public abstract class AbstractFileData
    {
        public abstract bool IsFile {  get; }

        public abstract bool IsDirectory { get; }

        public abstract bool IsDrive { get; }

        public abstract string Name { get; }

        public abstract string FullName { get; }

        public abstract ObservableCollection<FileData> SubDirectories { get; }

        public abstract ObservableCollection<FileData> Files { get; }

        public abstract long MaxLengthDirectory { get; set; }

        public abstract long MaxLengthFile { get; set; }

        public abstract Task GetDirectoriesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength);

        public abstract Task GetFilesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength);
    }
}
