using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FolderSizeExplorer.Utils
{
    public class FileIconUtil
    {
        public static BitmapSource GetIcon(string path, uint iconSize)
        {
            NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
            IntPtr hSuccess = NativeMethods.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), NativeMethods.SHGFI_ICON | iconSize);

            if (hSuccess != IntPtr.Zero)
            {
                Icon icon = Icon.FromHandle(shinfo.hIcon);

                return Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                   new Int32Rect(0, 0, icon.Width, icon.Height),
                   BitmapSizeOptions.FromEmptyOptions());
            }
            return null;
        }
    }
}
