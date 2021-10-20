using FolderSizeCollection.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;

namespace FolderSizeCollection.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            Drives = new ReactiveProperty<IEnumerable<DriveData>>(Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.Drive));
            //Drives.Value = Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.Drive);
            //    .Select(n => {
            //    var drv = new DriveInfo(n.Substring(0, 1));
            //    return $"{drv.Name} : {drv.VolumeLabel}";
            //});

            SelectedDrive = new ReactiveProperty<DriveData>(Drives.Value.FirstOrDefault());
            TreeSource = new ReactiveProperty<List<TreeSource>>();
            Logtext = new ReactiveProperty<string>();
            LogList = new ObservableCollection<string>();
            IsScanning = new ReactiveProperty<bool>();

            var factory = new TreeSourceFactory();
            //Logtext = factory.ObserveProperty(n => n.Logtext).ToReactiveProperty();

            ScanDriveCommand = new ReactiveCommand<DriveData>();
            ScanDriveCommand.Subscribe(async n =>
            {
                _fileCount = 0;
                LogList.Clear();

                _cancelSource = new CancellationTokenSource();
                var token = _cancelSource.Token;

                //TreeSource.Value = new List<TreeSource>() { await TreeSourceFactory.MakeInstance(n) };
                await Task.Run(async () =>
                {
                    IsScanning.Value = true;
                    try
                    {
                        TreeSource.Value = new List<TreeSource>() { await factory.MakeInstanceAsync(n.Drive, token) };
                    }
                    finally
                    {
                        IsScanning.Value = false;
                    }
                });
            });

            StopScanDriveCommand = new ReactiveCommand();
            StopScanDriveCommand.Subscribe(() =>
            {
                _cancelSource.Cancel();
            });

            factory.ReadingFile += TreeSourceFactory_ReadingFile;
        }

        private void TreeSourceFactory_ReadingFile(object sender, ReadingFileEventArgs e)
        {
            _ = Application.Current.Dispatcher.InvokeAsync(() =>
              {
                  if (e.IsError)
                  {
                      LogList.Add(e.FileName);
                  }
                  Logtext.Value = $"{++_fileCount}";
              },
            System.Windows.Threading.DispatcherPriority.Background);
        }

        private long _fileCount;
        private CancellationTokenSource _cancelSource;

        public ReactiveProperty<IEnumerable<DriveData>> Drives { get; set; }

        public ReactiveProperty<DriveData> SelectedDrive { get; set; }

        public ReactiveProperty<List<TreeSource>> TreeSource { get; set; }

        public ReactiveProperty<string> Logtext { get; set; }

        public ObservableCollection<string> LogList { get; set; }

        public ReactiveProperty<bool> IsScanning { get; set; }


        public ReactiveCommand<DriveData> ScanDriveCommand { get; private set; }

        public ReactiveCommand StopScanDriveCommand { get; private set; }
    }
}
