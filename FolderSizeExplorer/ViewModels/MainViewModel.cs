using FolderSizeExplorer.Models;
using FolderSizeExplorer.Utils;
using Livet;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FolderSizeExplorer.ViewModels
{
    public class MainViewModel : ViewModel
    {

        public MainViewModel()
        {
            //BindingOperations.EnableCollectionSynchronization(Dictionaries., new object());

            Drives = new ReactiveCollection<DriveData>();
            foreach (var drive in Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.FullName))
            {
                Drives.Add(drive);
            }
            SelectedDrive = new ReactiveProperty<DriveData>(Drives.FirstOrDefault());
            Directories = new ReactiveCollection<FileData>();
            Files = new ReactiveCollection<FileData>();
            IsScanning = new ReactiveProperty<bool>();
            MaxLengthFile = new ReactiveProperty<long>(0);
            MaxLengthDirectory = new ReactiveProperty<long>(0);
            SearchDirectory = new ReactiveProperty<string>(SelectedDrive.Value.FullName);
            SearchDirectoryCollection = new ReactiveCollection<AbstractFileData>();
            SelectedPath = new ReactiveProperty<FileData>();

            ScanDriveCommand = new ReactiveCommand();
            ScanDriveCommand.Subscribe(async () =>
            {
                await Task.Run(() =>
                {
                    IsScanning.Value = true;
                    try
                    {
                        _cancelSource = new CancellationTokenSource();
                        var token = _cancelSource.Token;
                        var tasks = new List<Task>();

                        SearchDirectoryCollection.ClearOnScheduler();
                        SearchDirectoryCollection.AddOnScheduler(SelectedDrive.Value);

                        foreach (var dir in DirectoryUtil.EnumerateDirectoriesData(SelectedDrive.Value.FullName))
                        {
                            Directories.AddOnScheduler(dir);
                            tasks.Add(Task.Run(async () => { await GetDirectoriesAsync(dir, token); }));
                        }

                        foreach (var file in DirectoryUtil.EnumerateFilesData(SelectedDrive.Value.FullName))
                        {
                            Files.AddOnScheduler(file);
                            if (file.Length > MaxLengthFile.Value)
                            {
                                MaxLengthFile.Value = file.Length;
                            }
                        }

                        Task.WaitAll(tasks.ToArray());

                        MaxLengthDirectory.Value = Directories.Max(d => d.Length);
                    }
                    finally
                    {
                        IsScanning.Value = false;
                    }
                });

                SelectedDrive.Value.SubDirectories.Clear();
                foreach (var dir in Directories)
                {
                    SelectedDrive.Value.SubDirectories.Add(dir);
                }
                foreach(var file in Files)
                {
                    SelectedDrive.Value.Files.Add(file);
                }
            });

            FolderSelectCommand = new ReactiveCommand();
            FolderSelectCommand.Subscribe(() =>
            {
                var dir = Directories.FirstOrDefault(d => d.FullName == SelectedPath.Value.FullName);
                if (dir == null)
                {
                    return;
                }

                SearchDirectory.Value = SelectedPath.Value.FullName;
                SearchDirectoryCollection.Add(SelectedPath.Value);

                Directories.ClearOnScheduler();
                Directories.AddRangeOnScheduler(dir.SubDirectories);
                Files.ClearOnScheduler();
                Files.AddRangeOnScheduler(dir.Files);
            });

            MoveFolderCommand = new ReactiveCommand<string>();
            MoveFolderCommand.Subscribe(path =>
            {
                var dir = Directories.FirstOrDefault(d => d.FullName == path);
                if (dir == null)
                {
                    return;
                }

                Directories.ClearOnScheduler();
                Directories.AddRangeOnScheduler(dir.SubDirectories);
                Files.ClearOnScheduler();
                Files.AddRangeOnScheduler(dir.Files);

            });
        }



        private async Task GetDirectoriesAsync(FileData dirData, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                return;
            }

            try
            {
                long sizeFile = 0;
                dirData.Files.Clear();
                //await DirectoryUtil.EnumerateFilesData(dirData.FullName).ForEachAsync((Func<FileData, Task>)(async file =>
                foreach (var file in DirectoryUtil.EnumerateFilesData(dirData.FullName))
                {
                    dirData.Files.Add(file);

                    sizeFile += file.Length;
                }
                //}), 200, token);
                dirData.Length += sizeFile;
                dirData.FilesCount = dirData.Files.Count;


                long sizeDir = 0;
                dirData.SubDirectories.Clear();
                dirData.SubDirectoriesCount = 0;
                //await DirectoryUtil.EnumerateDirectoriesData(dirData.FullName).ForEachAsync((Func<FileData, Task>)(async subDir =>
                foreach (var subDir in DirectoryUtil.EnumerateDirectoriesData(dirData.FullName))
                {
                    await GetDirectoriesAsync(subDir, token);

                    dirData.SubDirectories.Add(subDir);
                    dirData.SubDirectoriesCount++;
                    dirData.SubDirectoriesCount += subDir.SubDirectoriesCount;
                    dirData.FilesCount += subDir.FilesCount;
                    sizeDir += subDir.Length;
                }
                //}), 200, token);
                dirData.Length += sizeDir;
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
        }


        private CancellationTokenSource _cancelSource;


        public ReactiveCollection<DriveData> Drives { get; set; }
        public ReactiveProperty<DriveData> SelectedDrive { get; set; }

        public ReactiveProperty<string> SearchDirectory { get; set; }

        public ReactiveCollection<AbstractFileData> SearchDirectoryCollection { get; set; }

        public ReactiveProperty<FileData> SelectedPath { get; set; }

        public ReactiveCollection<FileData> Files { get; set; }

        public ReactiveCollection<FileData> Directories { get; set; }

        public ReactiveProperty<bool> IsScanning { get; set; }

        public ReactiveProperty<long> MaxLengthFile { get; set; }

        public ReactiveProperty<long> MaxLengthDirectory { get; set; }


        public ReactiveCommand ScanDriveCommand { get; private set; }

        public ReactiveCommand StopScanDriveCommand { get; private set; }

        public ReactiveCommand FolderSelectCommand { get; private set; }

        public ReactiveCommand<string> MoveFolderCommand { get; private set; }
    }
}
