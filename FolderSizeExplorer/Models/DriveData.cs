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
        public DriveData(string drivePath)
        {
            FullName = drivePath;
            Name = Path.GetFileName(drivePath);
            Image = FileIconUtil.GetIcon(drivePath, NativeMethods.IconSize.SHGFI_LARGEICON);

            SubDirectories = new ObservableCollection<FileData>();
            Files = new ObservableCollection<FileData>();
        }


        public override bool IsFile => false;

        public override bool IsDirectory => false;

        public override bool IsDrive => true;

        public override string FullName { get; }

        public override string Name { get; }

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


        public override async Task GetDirectoriesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength)
        {
            await Task.Run(async () =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    foreach (var dir in DirectoryUtil.EnumerateDirectoriesData(FullName))
                    {
                        SubDirectories.Add(dir);
                        progress.Report(dir);

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
                        await dir.GetFilesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir);
                        await dir.GetDirectoriesAsync(cancelToken, progressSubDir, progressMaxLengthSubDir);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    return;
                }
                catch (DirectoryNotFoundException ex)
                {
                    return;
                }
                catch (FileNotFoundException ex)
                {
                    return;
                }
                catch
                {
                    return;
                }
            });
        }


        public override async Task GetFilesAsync(CancellationToken cancelToken, IProgress<FileData> progress, IProgress<long> progressMaxLength)
        {
            await Task.Run(() =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
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
                    return;
                }
                catch (DirectoryNotFoundException ex)
                {
                    return;
                }
                catch (FileNotFoundException ex)
                {
                    return;
                }
                catch
                {
                    return;
                }
            });
        }
    }
}
