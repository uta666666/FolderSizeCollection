using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZetaLongPaths;

namespace FolderSizeCollection.Models
{
    public class TreeSource
    {
        private bool _IsExpanded = true;
        private string _Text = "";
        private TreeSource _Parent = null;
        private List<TreeSource> _Children = null;


        public bool IsExpanded {
            get { return _IsExpanded; }
            set { _IsExpanded = value; }
        }

        public string Text {
            get { return _Text; }
            set { _Text = value; }
        }

        public long Size { get; set; }

        public TreeSource Parent {
            get { return _Parent; }
            set { _Parent = value; }
        }

        public List<TreeSource> Children {
            get { return _Children; }
            set { _Children = value; }
        }

        public void Add(TreeSource child)
        {
            if (null == Children) Children = new List<TreeSource>();
            child.Parent = this;
            Children.Add(child);
        }
    }

    public class TreeSourceFactory : INotifyPropertyChanged
    {
        private string _logtext;

        public TreeSourceFactory()
        {
        }

        public string Logtext {
            get {
                return _logtext;
            }
            set {
                if (_logtext == value) return;
                _logtext = value;
                RaisePropertChanged(nameof(Logtext));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ReadingFileEventArgs> ReadingFile;
        protected void OnReadingFile(string fileName)
        {
            ReadingFile?.Invoke(null, new ReadingFileEventArgs() { FileName = fileName });
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs> ReadFileCompleted;
        protected void OnReadFileCompleted()
        {
            ReadFileCompleted?.Invoke(null, new EventArgs());
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Task<TreeSource> MakeInstance(string path)
        {
            return Task.Run(() => GetDirectories(path));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<TreeSource> MakeInstanceAsync(string path)
        {
            var t = GetDirectoriesAsync(path, true);
            //OnReadFileCompleted();
            return t;
        }


        private static TreeSource GetDirectories(string path)
        {
            var src = new TreeSource();

            try
            {
                var size = 0;// Directory.GetFiles(path).Sum(n => new ZlpFileInfo(n).Length);

                foreach (var dir in Directory.GetDirectories(path))
                {
                    var temp = GetDirectories(dir);
                    //size += temp.Size;
                    if (temp != null)
                    {
                        src.Add(temp);
                    }
                }

                src.Text = path;//string.IsNullOrEmpty(Path.GetFileName(path)) ? Path.GetPathRoot(path) : Path.GetFileName(path);
                src.Size = size;

            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return src;
        }


        private async Task<TreeSource> GetDirectoriesAsync(string path, bool isRoot)
        {
            //Logtext = path;
            OnReadingFile(path);

            var src = new TreeSource();

            try
            {
                long size = 0;

                await Directory.EnumerateDirectories(path).ForEachAsync((Func<string, Task>)(async dir =>
                {
                    var temp = await GetDirectoriesAsync(dir, false);
                    if (temp != null)
                    {
                        size += temp.Size;
                        src.Add((TreeSource)temp);
                    }
                }), 200);

                var fileSize = DirectoryUtil.EnumerateFilesData(path).Sum(n => n.Length);
                if (fileSize > 0)
                {
                    src.Add(new TreeSource() { Text = "Files", Size = fileSize });
                }
                try
                {
                    src.Children?.Sort((x, y) => ((y?.Size ?? 0) > (x?.Size ?? 0)) ? 1 : -1);
                }
                catch { }
                //src.Text = path;
                src.Text = isRoot ? Path.GetPathRoot(path) : Path.GetFileName(path);
                //src.Size = size + Directory.EnumerateFiles(path).Sum(n => new ZlpFileInfo(n).Length); ;
                src.Size = size + fileSize;
            }
            catch (UnauthorizedAccessException ex)
            {
                //System.Diagnostics.Debug.WriteLine($"{path} {ex.Message}");
                OnReadingFile($"{path} {ex.Message}");
                return null;
            }
            catch (DirectoryNotFoundException ex)
            {
                //System.Diagnostics.Debug.WriteLine($"{path} {ex.Message}");
                return null;
            }
            catch (FileNotFoundException ex)
            {
                //System.Diagnostics.Debug.WriteLine($"{path} {ex.Message}");
                return null;
            }
            catch
            {
                return null;
            }
            return src;
        }

        //private static async Task<TreeSource> GetDirectories2Async(string path)
        //{ 
        //    var src = new TreeSource();

        //        var size = 0;// Directory.GetFiles(path).Sum(n => new ZlpFileInfo(n).Length);

        //        var enumerateDirCollection = GetAllDirectories(@"C:\");
        //        await enumerateDirCollection.ForEachAsync(async enumerateDirs =>
        //        {
        //            foreach (var temp in await enumerateDirs)
        //            {
        //                var temp = GetDirectories(dir);
        //                if (temp != null)
        //                {
        //                    src.Add(temp);
        //                }
        //            }
        //        }, 200);//, _cancellationToken);
        //        src.Text = path;//string.IsNullOrEmpty(Path.GetFileName(path)) ? Path.GetPathRoot(path) : Path.GetFileName(path);
        //        src.Size = size;

        //    return src;
        //}

        //private static IEnumerable<ConfiguredTaskAwaitable<IEnumerable<string>>> GetAllDirectories(string path)
        //{
        //    IEnumerable<string> directories;

        //    try
        //    {
        //        directories = Directory.EnumerateDirectories(path);

        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        yield break;
        //    }
        //    catch (DirectoryNotFoundException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        yield break;
        //    }

        //    foreach (var dir in directories.Select(GetAllDirectories).SelectMany(n => n))
        //    {
        //        yield return dir;
        //    }

        //    var tcs = new TaskCompletionSource<IEnumerable<string>>();
        //    tcs.SetResult(directories);
        //    yield return tcs.Task.ConfigureAwait(false);

        //}

        //private static async Task<ConfiguredTaskAwaitable<TreeSource>> GetAllDirectories2Async(string path)
        //{
        //    var src = new TreeSource();

        //    try
        //    {
        //        var size = 0;// Directory.GetFiles(path).Sum(n => new ZlpFileInfo(n).Length);

        //        foreach (var dir in Directory.EnumerateDirectories(path))
        //        {
        //            var temp = await GetAllDirectories2Async(dir);
        //            //size += temp.Size;
        //            if (temp != null)
        //            {
        //                src.Add(temp);
        //            }
        //        }

        //        src.Text = path;//string.IsNullOrEmpty(Path.GetFileName(path)) ? Path.GetPathRoot(path) : Path.GetFileName(path);
        //        src.Size = size;

        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //    catch (DirectoryNotFoundException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //    var tcs = new TaskCompletionSource<TreeSource>();
        //    tcs.SetResult(src);
        //    return tcs.Task.ConfigureAwait(false);
        //}

    }

    public class ReadingFileEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }


    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int concurrency, CancellationToken cancellationToken = default(CancellationToken), bool configureAwait = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (concurrency <= 0) throw new ArgumentOutOfRangeException("concurrencyは1以上の必要があります");

            using (var semaphore = new SemaphoreSlim(initialCount: concurrency, maxCount: concurrency))
            {
                var exceptionCount = 0;
                var tasks = new List<Task>();

                foreach (var item in source)
                {
                    if (exceptionCount > 0) break;
                    cancellationToken.ThrowIfCancellationRequested();

                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(configureAwait);
                    var task = action(item).ContinueWith(t =>
                    {
                        semaphore.Release();

                        if (t.IsFaulted)
                        {
                            Interlocked.Increment(ref exceptionCount);
                            throw t.Exception;
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(configureAwait);
            }
        }
    }
}
