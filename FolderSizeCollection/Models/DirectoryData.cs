using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZetaLongPaths;

namespace FolderSizeCollection.Models
{
    public class DirectoryData
    {
        public string Path { get; set; }
        public long Size { get; set; }
    }

    public class DirectoryDataFactory
    {

        //public static List<DirectoryData> MakeInstance(string rootDirPath)
        //{
        //    return GetDirectories(rootDirPath);
        //}

        //private static List<DirectoryData> GetDirectories(string path)
        //{
        //    var dirList = new List<DirectoryData>();

        //    try
        //    {
        //        var size = Directory.GetFiles(path).Sum(n => new ZlpFileInfo(n).Length);

        //        //var dirs = Directory.GetDirectories(path);
        //        //dirList.AddRange(dirs);

        //        foreach (var dir in Directory.GetDirectories(path))
        //        {
        //            var temp = GetDirectories(dir);
        //            size += temp.Sum(n => n.Size);

        //            dirList.AddRange(temp);
        //        }

        //        var data = new DirectoryData()
        //        {
        //            Path = path,
        //            Size = size
        //        };
        //        dirList.Add(data);

        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    catch (DirectoryNotFoundException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return dirList;
        //}

        //private static TreeSource GetDirectories(string path)
        //{
        //    var src = new TreeSource();

        //    try
        //    {
        //        var size = Directory.GetFiles(path).Sum(n => new ZlpFileInfo(n).Length);

        //        foreach (var dir in Directory.GetDirectories(path))
        //        {
        //            var temp = GetDirectories(dir);
        //            size += temp.Children.Sum(n => n.Size);

        //            src.Add(temp);
        //        }

        //        src.Text = path;
        //        src.Size = size;

        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    catch (DirectoryNotFoundException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return src;
        //}
    }
}
