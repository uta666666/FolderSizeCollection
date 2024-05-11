using FolderSizeExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FolderSizeExplorer.Models
{
    public class DriveData : AbstractFileData, INotifyPropertyChanged
    {
        public DriveData(DriveInfo driveInfo)
        {
            FullName = driveInfo.Name;
            Name = driveInfo.Name;
            Image = FileIconUtil.GetIcon(driveInfo.Name, NativeMethods.IconSize.SHGFI_LARGEICON);
            FormatType = driveInfo.DriveFormat;
            FreeSpace = driveInfo.AvailableFreeSpace;
            TotalSize = driveInfo.TotalSize;
            DriveType = driveInfo.DriveType.ToString();

            SubDirectories = new ObservableCollection<FileData>();
            Files = new ObservableCollection<FileData>();
        }


        public override bool IsFile => false;

        public override bool IsDirectory => false;

        public override bool IsDrive => true;

        public override string FullName { get; }

        public override string Name { get; }

        public string FormatType { get; private set; }

        public long FreeSpace { get; private set; }

        public long FreeSpaceKB => FreeSpace / 1024;

        public long TotalSize { get; private set; }

        public long TotalSizeKB => TotalSize / 1024;

        public string DriveType { get; private set; }

        public BitmapSource Image { get; set; }

        public override ObservableCollection<FileData> SubDirectories { get; }

        public override ObservableCollection<FileData> Files { get; }

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



        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string? caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }




        public override string ToString() => FullName.TrimEnd('\\');


        public override async Task GetDirectoriesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength, IProgress<string> logger)
        {
            await Task.Run(async () =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                SubDirectories.Clear();
                MaxLengthDirectory = 0;

                try
                {
                    //表示のために先に追加しておく
                    foreach (var dir in DirectoryUtil.EnumerateDirectoriesData(FullName))
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                        SubDirectories.Add(dir);
                        progress.Report(dir);
                    }

                    //取得したディレクトリの中身を検索
                    foreach (var dir in SubDirectories)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                        dir.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(dir.Length))
                            {
                                if (dir.Length > MaxLengthDirectory)
                                {
                                    MaxLengthDirectory = dir.Length;
                                    progressMaxLength.Report(MaxLengthDirectory);
                                }
                            }
                        };
                        var progressSubDir = new Progress<FileData>(value => RaisePropertyChanged(nameof(SubDirectories)));
                        var progressMaxLengthSubDir = new Progress<long>(_ => { });
                        await dir.GetFilesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir, logger);
                        await dir.GetDirectoriesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir, logger);
                    }
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

                Files.Clear();
                MaxLengthFile = 0;

                try
                {
                    foreach (var file in DirectoryUtil.EnumerateFilesData(FullName))
                    {
                        Files.Add(file);
                        progress.Report(file);

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
