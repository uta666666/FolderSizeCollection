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

namespace FolderSizeCollection.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            Drives = new ReactiveProperty<IEnumerable<DriveData>>();
            Drives.Value = Directory.GetLogicalDrives().Select(n => new DriveData(n)).OrderBy(n => n.Drive);
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
                //TreeSource.Value = new List<TreeSource>() { await TreeSourceFactory.MakeInstance(n) };
                await Task.Run(async () =>
                {
                    IsScanning.Value = true;
                    try
                    {
                        TreeSource.Value = new List<TreeSource>() { await factory.MakeInstanceAsync(n.Drive) };
                    }
                    finally
                    {
                        IsScanning.Value = false;
                    }
                });
            });

            factory.ReadingFile += TreeSourceFactory_ReadingFile;
        }

        private void TreeSourceFactory_ReadingFile(object sender, ReadingFileEventArgs e)
        {
            _ = Application.Current.Dispatcher.InvokeAsync(() =>
              {
                  LogList.Add(e.FileName);
                  Logtext.Value = $"{LogList.Count}";
              },
            System.Windows.Threading.DispatcherPriority.Background);
        }


        public ReactiveProperty<IEnumerable<DriveData>> Drives { get; set; }

        public ReactiveProperty<DriveData> SelectedDrive { get; set; }

        public ReactiveProperty<List<TreeSource>> TreeSource { get; set; }

        public ReactiveProperty<string> Logtext { get; set; }

        public ObservableCollection<string> LogList { get; set; }

        public ReactiveProperty<bool> IsScanning { get; set; }


        public ReactiveCommand<DriveData> ScanDriveCommand { get; private set; }
    }
}
