using FolderSizeExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FolderSizeExplorer.Models
{
    public class DriveData : AbstractFileData
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

        public override string ToString() => FullName.TrimEnd('\\');
    }
}
