using FolderSizeCollection.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Threading;
using FolderSizeCollection.Views.UserControls;

namespace FolderSizeCollection.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            _log = new Log();

            Drives = new ReactiveProperty<IEnumerable<DriveData>>(Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.Drive));
            //Drives.Value = Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.Drive);
            //    .Select(n => {
            //    var drv = new DriveInfo(n.Substring(0, 1));
            //    return $"{drv.Name} : {drv.VolumeLabel}";
            //});
            SelectedDrive = new ReactiveProperty<DriveData>(Drives.Value.FirstOrDefault());
            TreeSource = new ReactiveProperty<List<TreeSource>>();
            SelectedTreeSource = new ReactiveProperty<TreeSource>();
            LogList = new ObservableCollection<string>();
            Logtext = _log.ObserveProperty(n => n.Value).ToReactiveProperty();
            Status = new ReactiveProperty<string>();
            IsScanning = new ReactiveProperty<bool>();
            TreeFontSize = new ReactiveProperty<double>(13);

            TreeSources = new ReactiveCollection<TreeSource>();
            //BindingOperations.EnableCollectionSynchronization(TreeSources, new object());

            var factory = new TreeSourceFactory();

            ScanDriveCommand = new ReactiveCommand<DriveData>();
            ScanDriveCommand.Subscribe(async n =>
            {
                Status.Value = $"[{n.Drive}]のサイズ計測中・・・";

                _fileCount = 0;
                LogList.Clear();
                if (TreeSource.Value != null)
                {
                    TreeSource.Value = null;
                }

                _cancelSource = new CancellationTokenSource();
                var token = _cancelSource.Token;

                //TreeSource.Value = new List<TreeSource>() { await TreeSourceFactory.MakeInstance(n) };
                //factory.TreeSourceCreated += (sender, e) =>
                //{
                //    TreeSources.AddOnScheduler(e.TreeSource);
                //};

                await Task.Run(async () =>
                {
                    IsScanning.Value = true;
                    try
                    {
                        //TreeSource.Value = new List<TreeSource>() { await factory.MakeInstanceAsync(n.Drive, token) };
                        TreeSources.ClearOnScheduler();
                        TreeSources.AddOnScheduler(await factory.MakeInstanceAsync(n.Drive, token));
                        //await factory.StartMakeTreeSourceAsync(n.Drive, token);

                    }
                    finally
                    {
                        IsScanning.Value = false;
                    }
                });

                Status.Value = $"[{n.Drive}]のサイズ計測完了";
            });

            StopScanDriveCommand = new ReactiveCommand();
            StopScanDriveCommand.Subscribe(() =>
            {
                _cancelSource.Cancel();
            });

            OpenExploreCommand = SelectedTreeSource.Select(n => n != null).ToReactiveCommand<TreeSource>();
            //OpenExploreCommand = new ReactiveCommand<TreeSource>();
            OpenExploreCommand.Subscribe(n =>
            {
                Status.Value = string.Empty;

                if (Directory.Exists(n.Path))
                {
                    Process.Start("EXPLORER.EXE", n.Path);
                }
                else if (Directory.Exists(n.Parent.Path))
                {
                    Process.Start("EXPLORER.EXE", n.Parent.Path);
                }
                else
                {
                    Status.Value = "フォルダが見つかりませんでした。";
                    //_ = Task.Delay(10 * 1000).ContinueWith(n => Status.Value = string.Empty);
                }
            });

            factory.ReadingFile += TreeSourceFactory_ReadingFile;
        }

        private void TreeSourceFactory_ReadingFile(object sender, ReadingFileEventArgs e)
        {
            try
            {
                _ = Application.Current.Dispatcher.InvokeAsync(() =>
                  {
                      if (e.IsError)
                      {
                          //LogList.Add(e.ToString());
                          _log.Add(e.ToString());
                      }
                      ++_fileCount;
                  },
                System.Windows.Threading.DispatcherPriority.Background);
            }
            catch
            {
                //errorは無視
            }
        }

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        private Log _log;
        private long _fileCount;
        private CancellationTokenSource _cancelSource;


        //Property-----------------------------------------------------------------
        public ReactiveProperty<IEnumerable<DriveData>> Drives { get; set; }

        public ReactiveProperty<DriveData> SelectedDrive { get; set; }

        public ReactiveProperty<List<TreeSource>> TreeSource { get; set; }

        public ReactiveCollection<TreeSource> TreeSources { get; set; }

        public ReactiveProperty<TreeSource> SelectedTreeSource { get; set; }

        public ObservableCollection<string> LogList { get; set; }

        public ReactiveProperty<string> Logtext { get; set; }

        public ReactiveProperty<string> Status { get; set; }

        public ReactiveProperty<bool> IsScanning { get; set; }

        public ReactiveProperty<double> TreeFontSize { get; set; }
        //Property-----------------------------------------------------------------


        //Command--------------------------------------------------------------------
        public ReactiveCommand<DriveData> ScanDriveCommand { get; private set; }

        public ReactiveCommand StopScanDriveCommand { get; private set; }

        public ReactiveCommand<TreeSource> OpenExploreCommand { get; private set; }
        //Command--------------------------------------------------------------------
    }
}
