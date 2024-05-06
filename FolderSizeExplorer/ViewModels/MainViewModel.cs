using FolderSizeExplorer.Models;
using FolderSizeExplorer.Utils;
using Livet;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Linq.Dynamic.Core;
using MahApps.Metro.Controls.Dialogs;

namespace FolderSizeExplorer.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private IDialogCoordinator _dialogCordinator;

        public MainViewModel(IDialogCoordinator instance)
        {
            _dialogCordinator = instance;
            _cancelSource = new CancellationTokenSource();

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
            BreadCrumbDictionaries = new ReactiveCollection<AbstractFileData>();
            SelectedDirectoryPath = new ReactiveProperty<FileData>();
            SelectedFilePath = new ReactiveProperty<FileData>();
            DirectoryNameSortMark = new ReactiveProperty<string>();
            DirectoryLengthSortMark = new ReactiveProperty<string>();
            FileNameSortMark = new ReactiveProperty<string>();
            FileLengthSortMark = new ReactiveProperty<string>();
            StatusMessage = new ReactiveProperty<string>();


            ScanDriveCommand = new ReactiveCommand();
            ScanDriveCommand.Subscribe(async () => await ScanDriveAsync());

            StopScanDriveCommand = new ReactiveCommand();
            StopScanDriveCommand.Subscribe(StopScanDrive);

            FolderSelectCommand = new ReactiveCommand();
            FolderSelectCommand.Subscribe(SelectDirectory);

            MoveFolderCommand = new ReactiveCommand<string>();
            MoveFolderCommand.Subscribe(MoveDirectory);

            SortDirectoryCommand = new ReactiveCommand<string>();
            SortDirectoryCommand.Subscribe(SortDirectories);

            SortFileCommand = new ReactiveCommand<string>();
            SortFileCommand.Subscribe(SortFiles);

            OpenExplorerCommand = new ReactiveCommand<FileData>();
            OpenExplorerCommand.Subscribe(OpenExplorer);

            DeleteCommnad = new ReactiveCommand<FileData>();
            DeleteCommnad.Subscribe(DeleteFile);
        }


        private const string NamePropertyName = "Name";
        private const string LengthPropertyName = "Length";

        private string _sortDirectoryPropertyName = "";
        private bool _isSortDirectoryAsc = true;
        private string _sortFilePropertyName = "";
        private bool _isSortFileAsc = true;
        private CancellationTokenSource _cancelSource;



        /// <summary>
        /// スキャンを実行する
        /// </summary>
        /// <returns></returns>
        private async Task ScanDriveAsync()
        {
            IsScanning.Value = true;
            StatusMessage.Value = "解析中．．．";
            try
            {
                _cancelSource = new CancellationTokenSource();
                var token = _cancelSource.Token;
                var tasks = new List<Task>();

                BreadCrumbDictionaries.ClearOnScheduler();
                BreadCrumbDictionaries.AddOnScheduler(SelectedDrive.Value);
                Directories.ClearOnScheduler();
                Files.ClearOnScheduler();


                var progressGetFiles = new Progress<FileData>(file =>
                {
                    Files.AddOnScheduler(file);
                });
                var progressChangeMaxLengthFile = new Progress<long>(maxLength =>
                {
                    MaxLengthFile.Value = maxLength;
                });
                var taskFile = SelectedDrive.Value.GetFilesAsync(token, progressGetFiles, progressChangeMaxLengthFile);


                var progressGetDirectories = new Progress<FileData>(dir =>
                {
                    Directories.AddOnScheduler(dir);
                });
                var progressChangeMaxLengthDirectory = new Progress<long>(maxLength =>
                {
                    MaxLengthDirectory.Value = maxLength;
                });
                var taskDirectory = SelectedDrive.Value.GetDirectoriesAsync(token, progressGetDirectories, progressChangeMaxLengthDirectory);


                await Task.WhenAll(taskFile, taskDirectory);
            }
            finally
            {
                IsScanning.Value = false;
                StatusMessage.Value = "解析終了";
            }
        }


        /// <summary>
        /// スキャンを止める
        /// </summary>
        private void StopScanDrive()
        {
            if (IsScanning.Value)
            {
                _cancelSource?.Cancel();
            }
        }


        /// <summary>
        /// フォルダ選択
        /// </summary>
        private void SelectDirectory()
        {
            if (SelectedDirectoryPath.Value == null)
            {
                return;
            }
            var dir = Directories.FirstOrDefault(d => d.FullName == SelectedDirectoryPath.Value.FullName);
            if (dir == null)
            {
                return;
            }

            SearchDirectory.Value = SelectedDirectoryPath.Value.FullName;
            BreadCrumbDictionaries.Add(SelectedDirectoryPath.Value);

            MaxLengthDirectory.Value = dir.MaxLengthDirectory;
            MaxLengthFile.Value = dir.MaxLengthFile;

            SetCollection(Directories, dir.SubDirectories, _sortDirectoryPropertyName, _isSortDirectoryAsc);
            SetCollection(Files, dir.Files, _sortFilePropertyName, _isSortFileAsc);
        }


        /// <summary>
        /// フォルダ選択
        /// </summary>
        /// <param name="path"></param>
        private void MoveDirectory(string path)
        {
            (AbstractFileData dir, int index) = BreadCrumbDictionaries.Select((data, index) => (data, index)).FirstOrDefault(d => d.data.FullName == path);
            if (dir == null)
            {
                return;
            }

            for (int i = BreadCrumbDictionaries.Count - 1; i > index; i--)
            {
                BreadCrumbDictionaries.RemoveAt(i);
            }
            SearchDirectory.Value = dir.IsDrive ? dir.ToString() : dir.FullName;

            MaxLengthDirectory.Value = dir.MaxLengthDirectory;
            MaxLengthFile.Value = dir.MaxLengthFile;

            SetCollection(Directories, dir.SubDirectories, _sortDirectoryPropertyName, _isSortDirectoryAsc);
            SetCollection(Files, dir.Files, _sortFilePropertyName, _isSortFileAsc);
        }


        /// <summary>
        /// ディレクトリ一覧をソートする
        /// </summary>
        /// <param name="sortPropertyName"></param>
        private void SortDirectories(string sortPropertyName)
        {
            if (Directories.Count == 0)
            {
                return;
            }

            if (_sortDirectoryPropertyName == sortPropertyName)
            {
                _isSortDirectoryAsc = !_isSortDirectoryAsc;
            }
            else
            {
                _isSortDirectoryAsc = true;
                _sortDirectoryPropertyName = sortPropertyName;
            }

            SetCollection(Directories, Directories, _sortDirectoryPropertyName, _isSortDirectoryAsc);
            DirectoryNameSortMark.Value = _sortDirectoryPropertyName == NamePropertyName ? (_isSortDirectoryAsc ? "▲" : "▼") : "";
            DirectoryLengthSortMark.Value = _sortDirectoryPropertyName == LengthPropertyName ? (_isSortDirectoryAsc ? "▲" : "▼") : "";
        }


        /// <summary>
        /// ファイル一覧をソートする
        /// </summary>
        private void SortFiles(string sortPropertyName)
        {
            if (Files.Count == 0)
            {
                return;
            }

            if (_sortFilePropertyName == sortPropertyName)
            {
                _isSortFileAsc = !_isSortFileAsc;
            }
            else
            {
                _isSortFileAsc = true;
                _sortFilePropertyName = sortPropertyName;
            }

            SetCollection(Files, Files, _sortFilePropertyName, _isSortFileAsc);
            FileNameSortMark.Value = (_sortFilePropertyName == NamePropertyName ? (_isSortFileAsc ? "▲" : "▼") : "");
            FileLengthSortMark.Value = (_sortFilePropertyName == LengthPropertyName ? (_isSortFileAsc ? "▲" : "▼") : "");
        }


        /// <summary>
        /// Collectionをソートする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="isAscending"></param>
        private void SetCollection<T>(ReactiveCollection<T> collection, IEnumerable<T> items, string sortPropertyName, bool isAscending)
        {
            collection.ClearOnScheduler();
            if (string.IsNullOrEmpty(sortPropertyName))
            {
                collection.AddRangeOnScheduler(items);
            }
            else
            {
                collection.AddRangeOnScheduler(items.AsQueryable().OrderBy($"{sortPropertyName} {(isAscending ? "ascending" : "descending")}"));
            }
        }


        /// <summary>
        /// エクスプローラーで表示
        /// </summary>
        /// <param name="fileData"></param>
        private void OpenExplorer(FileData fileData)
        {
            string path = string.Empty;
            if (fileData.IsDirectory)
            {
                System.Diagnostics.Process.Start("EXPLORER.EXE", fileData.FullName);
            }
            else if (fileData.IsFile)
            {
                path = Path.GetDirectoryName(fileData.FullName) ?? string.Empty;
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (File.Exists(path) || Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("EXPLORER.EXE", path);
            }
        }


        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="fileData"></param>
        private async void DeleteFile(FileData fileData)
        {
            var result = await ShowConfirmMessage("確認", $"{fileData.Name}を削除しますか？");
            if (result == MessageDialogResult.Affirmative)
            {
                await DeleteFileAsync(fileData.FullName);
            }
        }

        /// <summary>
        /// 削除処理本体
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
            else if (Directory.Exists(filePath))
            {
                await Task.Run(() => Directory.Delete(filePath));
            }
        }


        /// <summary>
        /// メッセージを表示
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<MessageDialogResult> ShowConfirmMessage(string title, string message)
        {
            return await _dialogCordinator.ShowMessageAsync(this, title, message, MessageDialogStyle.AffirmativeAndNegative);
        }



        #region プロパティ
        public ReactiveCollection<DriveData> Drives { get; set; }

        public ReactiveProperty<DriveData> SelectedDrive { get; set; }

        public ReactiveProperty<string> SearchDirectory { get; set; }

        public ReactiveCollection<AbstractFileData> BreadCrumbDictionaries { get; set; }

        public ReactiveProperty<FileData> SelectedDirectoryPath { get; set; }

        public ReactiveProperty<FileData> SelectedFilePath { get; set; }

        public ReactiveCollection<FileData> Files { get; set; }

        public ReactiveCollection<FileData> Directories { get; set; }

        public ReactiveProperty<bool> IsScanning { get; set; }

        public ReactiveProperty<long> MaxLengthFile { get; set; }

        public ReactiveProperty<long> MaxLengthDirectory { get; set; }

        public ReactiveProperty<string> DirectoryNameSortMark { get; set; }

        public ReactiveProperty<string> DirectoryLengthSortMark { get; set; }

        public ReactiveProperty<string> FileNameSortMark { get; set; }

        public ReactiveProperty<string> FileLengthSortMark { get; set; }

        public ReactiveProperty<string> StatusMessage { get; set; }
        #endregion プロパティ


        #region コマンド
        public ReactiveCommand ScanDriveCommand { get; private set; }

        public ReactiveCommand StopScanDriveCommand { get; private set; }

        public ReactiveCommand FolderSelectCommand { get; private set; }

        public ReactiveCommand<string> MoveFolderCommand { get; private set; }

        public ReactiveCommand<FileData> DeleteCommnad { get; private set; }

        public ReactiveCommand<FileData> OpenExplorerCommand { get; private set; }

        public ReactiveCommand<string> SortDirectoryCommand { get; private set; }

        public ReactiveCommand<string> SortFileCommand { get; private set; }
        #endregion コマンド
    }
}
