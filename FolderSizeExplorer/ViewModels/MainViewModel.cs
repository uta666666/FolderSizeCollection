using FolderSizeExplorer.Models;
using Livet;
using Reactive.Bindings;
using System.Data;
using System.IO;
using System.Reactive.Linq;
using System.Linq.Dynamic.Core;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using System.Collections.Specialized;
using System.Text;
using Reactive.Bindings.Extensions;

namespace FolderSizeExplorer.ViewModels
{
    /// <summary>
    /// MainViewModel
    /// </summary>
    public class MainViewModel : ViewModel
    {
        private IDialogCoordinator _dialogCordinator;

        private CancellationTokenSource _cancelSource;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="instance"></param>
        public MainViewModel(IDialogCoordinator instance)
        {
            _dialogCordinator = instance;
            _cancelSource = new CancellationTokenSource();

            Drives = new ReactiveCollection<DriveData>();
            foreach (var drive in DriveUtil.GetDives().OrderBy(n => n.FullName))
            {
                Drives.Add(drive);
            }
            SelectedDrive = new ReactiveProperty<DriveData>(Drives.First());
            Directories = new ReactiveCollection<FileData>();
            Files = new ReactiveCollection<FileData>();
            IsScanning = new ReactiveProperty<bool>();
            MaxLengthFile = new ReactiveProperty<long>(0);
            MaxLengthDirectory = new ReactiveProperty<long>(0);
            SearchDirectory = new ReactiveProperty<string>(SelectedDrive.Value.FullName);
            BreadCrumbDictionaries = new ReactiveCollection<AbstractFileData>();
            SelectedDirectoryPath = new ReactiveProperty<FileData>();
            SelectedFilePath = new ReactiveProperty<FileData>();
            StatusMessage = new ReactiveProperty<string>();
            IsBusy = new ReactiveProperty<bool>();
            SortFilePropertyName = new ReactiveProperty<string>();
            IsSortFileAsc = new ReactiveProperty<bool>();
            SortDirectoryPropertyName = new ReactiveProperty<string>();
            IsSortDirectoryAsc = new ReactiveProperty<bool>();

            DriveVolume = SelectedDrive.ToReactivePropertyAsSynchronized(x => x.Value.Name);
            DriveFormatType = SelectedDrive.ToReactivePropertyAsSynchronized(x => x.Value.FormatType);
            DriveType = SelectedDrive.ToReactivePropertyAsSynchronized(x => x.Value.DriveType);
            DriveFreeSpace = SelectedDrive.ToReactivePropertyAsSynchronized(x => x.Value.FreeSpaceKB);
            DriveTotalSize = SelectedDrive.ToReactivePropertyAsSynchronized(x => x.Value.TotalSizeKB);


                Files.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (SelectedFilePath.Value == null || Files.Count == 1)
                    {
                        SelectedFilePath.Value = Files.First();
                    }
                }
            };

            Directories.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (SelectedDirectoryPath.Value == null || Directories.Count == 1)
                    {
                        SelectedDirectoryPath.Value = Directories.First();
                    }
                }
            };


            ScanDriveCommand = new ReactiveCommand();
            ScanDriveCommand.Subscribe(async () => await ScanDriveAsync());

            StopScanDriveCommand = new ReactiveCommand();
            StopScanDriveCommand.Subscribe(StopScanDrive);

            FolderSelectCommand = new ReactiveCommand();
            FolderSelectCommand.Subscribe(async () => await SelectDirectoryAsync());

            MoveFolderCommand = new ReactiveCommand<string>();
            MoveFolderCommand.Subscribe(async path => await MoveDirectoryAsync(path));

            SortDirectoryCommand = new ReactiveCommand<string>();
            SortDirectoryCommand.Subscribe(async propertyName => await SortDirectoriesAsync(propertyName));

            SortFileCommand = new ReactiveCommand<string>();
            SortFileCommand.Subscribe(async propertyName => await SortFilesAsync(propertyName));

            OpenExplorerCommand = new ReactiveCommand<FileData>();
            OpenExplorerCommand.Subscribe(OpenExplorer);

            DeleteCommnad = new ReactiveCommand<FileData>();
            DeleteCommnad.Subscribe(DeleteFile);

            SwitchThemeCommand = new ReactiveCommand<bool>();
            SwitchThemeCommand.Subscribe(SwitchTheme);

            MovePreviousFolderCommand = new ReactiveCommand();
            MovePreviousFolderCommand.Subscribe(async () => await MovePreviousAsync());
        }


        /// <summary>
        /// スキャンを実行する
        /// </summary>
        /// <returns></returns>
        private async Task ScanDriveAsync()
        {
            IsScanning.Value = true;
            StartTimer();
            try
            {
                _cancelSource = new CancellationTokenSource();
                var token = _cancelSource.Token;
                var tasks = new List<Task>();

                BreadCrumbDictionaries.ClearOnScheduler();
                BreadCrumbDictionaries.AddOnScheduler(SelectedDrive.Value);
                Directories.ClearOnScheduler();
                Files.ClearOnScheduler();


                var progressLogger = new Progress<string>(async message => await Logging(message));
                var progressGetFiles = new Progress<FileData>(Files.AddOnScheduler);
                var progressChangeMaxLengthFile = new Progress<long>(maxLength =>
                {
                    MaxLengthFile.Value = maxLength;
                });
                var taskFile = SelectedDrive.Value.GetFilesAsync(token, progressGetFiles, progressChangeMaxLengthFile, progressLogger);


                var progressGetDirectories = new Progress<FileData>(Directories.AddOnScheduler);
                var progressChangeMaxLengthDirectory = new Progress<long>(maxLength =>
                {
                    MaxLengthDirectory.Value = maxLength;
                });
                var taskDirectory = SelectedDrive.Value.GetDirectoriesAsync(token, progressGetDirectories, progressChangeMaxLengthDirectory, progressLogger);


                await Task.WhenAll(taskFile, taskDirectory);
            }
            finally
            {
                IsScanning.Value = false;
            }
        }


        /// <summary>
        /// 解析のタイマーを開始
        /// </summary>
        private void StartTimer()
        {
            var start = DateTime.Now;

            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                var elapsed = (int)(DateTime.Now - start).TotalSeconds;
                if (IsScanning.Value)
                {
                    SetStatus($"[{SelectedDrive.Value}] Analyzing... ({elapsed} s)");
                }
                else
                {
                    timer.Stop();
                    SetStatus($"[{SelectedDrive.Value}] Analysis Completed ({elapsed} s)");
                }
            };
            timer.Start();
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
        private async Task SelectDirectoryAsync()
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

            await SetCollectionAsync(Directories, dir.SubDirectories, SortDirectoryPropertyName.Value, IsSortDirectoryAsc.Value);
            await SetCollectionAsync(Files, dir.Files, SortFilePropertyName.Value, IsSortFileAsc.Value);
        }


        /// <summary>
        /// フォルダ選択
        /// </summary>
        /// <param name="path"></param>
        private async Task MoveDirectoryAsync(string path)
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

            await SetCollectionAsync(Directories, dir.SubDirectories, SortDirectoryPropertyName.Value, IsSortDirectoryAsc.Value);
            await SetCollectionAsync(Files, dir.Files, SortFilePropertyName.Value, IsSortFileAsc.Value);
        }


        /// <summary>
        /// ディレクトリ一覧をソートする
        /// </summary>
        /// <param name="sortPropertyName"></param>
        private async Task SortDirectoriesAsync(string sortPropertyName)
        {
            IsBusy.Value = true;
            try
            {
                if (Directories.Count == 0)
                {
                    return;
                }

                if (SortDirectoryPropertyName.Value == sortPropertyName)
                {
                    IsSortDirectoryAsc.Value = !IsSortDirectoryAsc.Value;
                }
                else
                {
                    IsSortDirectoryAsc.Value = true;
                    SortDirectoryPropertyName.Value = sortPropertyName;
                }

                await SetCollectionAsync(Directories, Directories.ToList(), SortDirectoryPropertyName.Value, IsSortDirectoryAsc.Value);
                SelectedDirectoryPath.Value = Directories.First();
            }
            finally
            {
                IsBusy.Value = false;
            }
        }


        /// <summary>
        /// ファイル一覧をソートする
        /// </summary>
        private async Task SortFilesAsync(string sortPropertyName)
        {
            IsBusy.Value = true;
            try
            {
                if (Files.Count == 0)
                {
                    return;
                }

                if (SortFilePropertyName.Value == sortPropertyName)
                {
                    IsSortFileAsc.Value = !IsSortFileAsc.Value;
                }
                else
                {
                    IsSortFileAsc.Value = true;
                    SortFilePropertyName.Value = sortPropertyName;
                }

                await SetCollectionAsync(Files, Files.ToList(), SortFilePropertyName.Value, IsSortFileAsc.Value);
                SelectedFilePath.Value = Files.First();
            }
            finally
            {
                IsBusy.Value = false;
            }
        }


        /// <summary>
        /// Collectionをソートする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="isAscending"></param>
        private async Task SetCollectionAsync<T>(ReactiveCollection<T> collection, IEnumerable<T> items, string sortPropertyName, bool isAscending)
        {
            await Task.Run(() =>
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
            });
        }


        /// <summary>
        /// エクスプローラーで表示
        /// </summary>
        /// <param name="fileData"></param>
        private void OpenExplorer(FileData fileData)
        {
            string path = string.Empty;
            if (fileData == null)
            {
                return;
            }

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
            if (fileData == null)
            {
                return;
            }

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


        private async Task MovePreviousAsync()
        {
            if (BreadCrumbDictionaries.Count <= 1)
            {
                return;
            }

            BreadCrumbDictionaries.RemoveAt(BreadCrumbDictionaries.Count - 1);
            var path = BreadCrumbDictionaries.Last().FullName;
            SearchDirectory.Value = path;
            await MoveDirectoryAsync(path);
        }


        /// <summary>
        /// テーマを切り替える
        /// </summary>
        /// <param name="isDark"></param>
        private void SwitchTheme(bool isDark)
        {
            var helper = new PaletteHelper();
            ITheme theme = helper.GetTheme();
            theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);
            helper.SetTheme(theme);

            ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, isDark ? "Dark" : "Light");
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


        /// <summary>
        /// ステータスを表示
        /// </summary>
        /// <param name="message"></param>
        private void SetStatus(string message)
        {
            StatusMessage.Value = message;
        }


        /// <summary>
        /// ログを出力
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task Logging(string message)
        {
            try
            {
                SetStatus(message);
                await File.AppendAllTextAsync("log.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} {message}", Encoding.UTF8);
            }
            catch { }
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

        public ReactiveProperty<string> StatusMessage { get; set; }

        public ReactiveProperty<bool> IsBusy { get; set; }

        public ReactiveProperty<string> SortFilePropertyName { get; set; }

        public ReactiveProperty<bool> IsSortFileAsc { get; set; }

        public ReactiveProperty<string> SortDirectoryPropertyName { get; set; }

        public ReactiveProperty<bool> IsSortDirectoryAsc { get; set; }

        public ReactiveProperty<string> DriveFormatType { get; private set; }

        public ReactiveProperty<long> DriveFreeSpace { get; private set; }

        public ReactiveProperty<long> DriveTotalSize { get; private set; }

        public ReactiveProperty<string> DriveVolume { get; private set; }
        public ReactiveProperty<string> DriveType { get; private set; }
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

        public ReactiveCommand<bool> SwitchThemeCommand { get; private set; }

        public ReactiveCommand MovePreviousFolderCommand { get; private set; }

        #endregion コマンド
    }
}
