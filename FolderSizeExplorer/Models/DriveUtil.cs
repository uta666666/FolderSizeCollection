using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSizeExplorer.Models
{
    public class DriveUtil
    {
        public static IEnumerable<DriveData> GetDives()
        {
            var drives = System.IO.DriveInfo.GetDrives();
            foreach (var drive in drives.Where(d => d.DriveType != System.IO.DriveType.CDRom && d.DriveType != System.IO.DriveType.Unknown && d.DriveType != System.IO.DriveType.Ram))
            {
                DriveData driveData;
                try
                {
                    driveData = new DriveData(drive);
                }
                catch
                {
                    continue;
                }
                yield return driveData;
            }
        }
    }
}
