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

        private readonly object _lockLength = new object();
        private long _length = 0;
        public long Length
        {
            get { return _length; }
            private set
            {
                lock (_lockLength)
                {
                    if (_length == value)
                    {
                        return;
                    }
                    _length = value;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(LengthKB));
            }
        }

        public long LengthKB => Length / 1024;

        public override ObservableCollection<FileData> SubDirectories { get; }

        public override ObservableCollection<FileData> Files { get; }

        private readonly object _lockSubDirectoriesCount = new object();
        private int _subDirectoriesCount;
        public int SubDirectoriesCount
        {
            get { return _subDirectoriesCount; }
            private set
            {
                lock (_lockSubDirectoriesCount)
                {
                    if (_subDirectoriesCount == value)
                    {
                        return;
                    }
                    _subDirectoriesCount = value;
                }
                RaisePropertyChanged();
            }
        }

        private readonly object _lockFileCount = new object();
        private int _filesCount;
        public int FilesCount
        {
            get { return _filesCount; }
            private set
            {
                lock (_lockFileCount)
                {
                    if (_filesCount == value)
                    {
                        return;
                    }
                    _filesCount = value;
                }
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

        private long _maxLengthFile = 0;
        public override long MaxLengthFile
        {
            get
            {
                return _maxLengthFile;
            }
            set
            {
                if (_maxLengthFile == value)
                {
                    return;
                }
                _maxLengthFile = value;
                RaisePropertyChanged();
            }
        }

        private long _maxLengthDirectory = 0;
        public override long MaxLengthDirectory
        {
            get
            {
                return _maxLengthDirectory;
            }
            set
            {
                if (_maxLengthDirectory == value)
                {
                    return;
                }
                _maxLengthDirectory = value;
                RaisePropertyChanged();
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






        public override async Task GetDirectoriesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength, IProgress<string> logger)
        {
            await Task.Run(async () =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                long sizeDir = 0;
                SubDirectories.Clear();
                SubDirectoriesCount = 0;
                MaxLengthDirectory = 0;

                try
                {
                    foreach (var subDir in DirectoryUtil.EnumerateDirectoriesData(FullName))
                    {
                        SubDirectories.Add(subDir);
                        SubDirectoriesCount++;
                        progress.Report(subDir);

                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        subDir.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(subDir.Length))
                            {
                                if (subDir.Length > MaxLengthDirectory)
                                {
                                    MaxLengthDirectory = subDir.Length;
                                    progressMaxLength.Report(MaxLengthDirectory);
                                }
                            }
                        };

                        var progressSubDir = new Progress<FileData>(value => RaisePropertyChanged(nameof(SubDirectories)));
                        var progressMaxLengthSubDir = new Progress<long>(_ => { });
                        await subDir.GetFilesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir, logger);
                        await subDir.GetDirectoriesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir, logger);

                        SubDirectoriesCount += subDir.SubDirectoriesCount;
                        FilesCount += subDir.FilesCount;
                        sizeDir += subDir.Length;
                    }
                    Length += sizeDir;
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (DirectoryNotFoundException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (FileNotFoundException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
            });
        }

        public override async Task GetFilesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength, IProgress<string> logger)
        {
            await Task.Run(() =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                long sizeFile = 0;
                Files.Clear();
                FilesCount = 0;
                MaxLengthFile = 0;

                try
                {
                    foreach (var file in DirectoryUtil.EnumerateFilesData(FullName))
                    {
                        Files.Add(file);
                        sizeFile += file.Length;

                        if (file.Length > MaxLengthFile)
                        {
                            MaxLengthFile = file.Length;
                            progressMaxLength.Report(MaxLengthFile);
                        }

                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    Length += sizeFile;
                    FilesCount = Files.Count;
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (DirectoryNotFoundException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (FileNotFoundException ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Report(ex.Message);
                    return;
                }
            });
        }
    }
}
