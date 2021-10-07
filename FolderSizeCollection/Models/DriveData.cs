using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FolderSizeCollection.Models
{
    public class DriveData
    {
        public DriveData(string drivePath)
        {
            Drive = drivePath;

            NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
            IntPtr hSuccess = NativeMethods.SHGetFileInfo(drivePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON);

            if (hSuccess != IntPtr.Zero)
            {
                Icon icon = Icon.FromHandle(shinfo.hIcon);

                Image = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                   new Int32Rect(0, 0, icon.Width, icon.Height),
                   BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public BitmapSource Image { get; set; }

        public string Drive { get; set; }

    }
}
