using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FolderSizeExplorer.Utils;

namespace FolderSizeExplorer.Models
{
    public class FileData : AbstractFileData, INotifyPropertyChanged
    {
        public FileAttributes Attributes { get; }
        public override bool IsFile => (Attributes & FileAttributes.Directory) == 0;
        public override bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;
        public override bool IsDrive => false;
        public DateTime CreationTimeUtc { get; }
        public DateTime CreationTime => CreationTimeUtc.ToLocalTime();
        public DateTime LastAccessTimeUtc { get; }
        public DateTime LastAccesTime => LastAccessTimeUtc.ToLocalTime();
        public DateTime LastWriteTimeUtc { get; }
        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        private long _length = 0;
        public long Length
        {
            get { return _length; }
            set
            {
                if (_length == value)
                {
                    return;
                }
                _length = value;
                RaisePropertyChanged();
            }
        }

        public override ObservableCollection<FileData> SubDirectories { get; }

        public override ObservableCollection<FileData> Files { get; }

        private int _subDirectoriesCount;
        public int SubDirectoriesCount
        {
            get { return _subDirectoriesCount; }
            set
            {
                if (_subDirectoriesCount == value)
                {
                    return;
                }
                _subDirectoriesCount = value;
                RaisePropertyChanged();
            }
        }

        private int _filesCount;
        public int FilesCount
        {
            get { return _filesCount; }
            set
            {
                if (_filesCount == value)
                {
                    return;
                }
                _filesCount = value;
                RaisePropertyChanged();
            }
        }

        public override string Name { get; }

        public override string FullName { get; }

        private Lazy<BitmapSource> _image;
        public BitmapSource Image
        {
            get
            {
                return _image.Value;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="findData"></param>
        internal FileData(ref string fullName, ref NativeMethods.WIN32_FIND_DATA findData)
        {
            Attributes = findData.dwFileAttributes;
            CreationTimeUtc = findData.ToCreationTimeUtc;
            LastAccessTimeUtc = findData.ToLastAccessTimeUtc;
            LastWriteTimeUtc = findData.ToLastWriteTimeUtc;
            Length = ((long)findData.nFileSizeHigh << 32) + findData.nFileSizeLow;
            Name = findData.cFileName;
            FullName = fullName;
            _image = new Lazy<BitmapSource>(() => { return FileIconUtil.GetIcon(FullName, NativeMethods.IconSize.SHGFI_SMALLICON); });

            SubDirectories = new ObservableCollection<FileData>();
            Files = new ObservableCollection<FileData>();
        }




        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string? caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        public override string ToString() => Name;
    }
}
