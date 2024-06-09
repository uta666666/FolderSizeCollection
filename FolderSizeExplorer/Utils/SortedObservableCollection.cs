using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace FolderSizeExplorer.Utils
{
    public class SortedObservableCollection<T> : ObservableCollection<T> where T : class, INotifyPropertyChanged
    {
        public SortedObservableCollection(IComparer<T> comparer) : base()
        {
            Comparer = comparer;
        }

        public SortedObservableCollection(IComparer<T> comparer, IEnumerable<T> collection) : base(collection.OrderBy(x => x, comparer))
        {
            Comparer = comparer;
        }


        private IScheduler scheduler = ReactivePropertyScheduler.Default;

        public void AddOnScheduler(T item) => scheduler.Schedule(() => Add(item));

        public void AddRangeOnScheduler(IEnumerable<T> collection) => scheduler.Schedule(() => AddRange(collection));

        public void ClearOnScheduler() => scheduler.Schedule(() => Clear());


        private readonly object _lockobj = new object();


        /// <summary>
        /// 比較方法
        /// </summary>
        public IComparer<T> Comparer { get; set; }


        /// <summary>
        /// 一括追加
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        /// <summary>
        /// ソート
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            Comparer = comparer;

            var orderdList = Items.OrderBy(x => x, Comparer).ToList();
            ClearOnScheduler();
            AddRangeOnScheduler(orderdList);
        }

        /// <summary>
        /// 自分より小さい最後のインデックスを返す
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int LastIndexOf(T item)
        {
            lock (_lockobj)
            {
                // return this.Where(x => x != item && Comparer.Compare(x, item) <= 0).Select((data, index) => index).Max(x => (int?)x) ?? -1;
                return this.ToList().Select((data, index) => new { data, index }).LastOrDefault(x => x.data != item && Comparer.Compare(x.data, item) <= 0)?.index ?? -1;
            }
        }

        /// <summary>
        /// 適切な位置に挿入
        /// </summary>
        /// <param name="_"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int _, T item)
        {
            // 後ろにつける
            var index = LastIndexOf(item) + 1;
            lock (_lockobj)
            {
                base.InsertItem(index, item);
            }
            item.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// 適切な位置に移動
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="_"></param>
        protected override void MoveItem(int oldIndex, int _)
        {
            var lastIndex = LastIndexOf(this[oldIndex]);
            if (lastIndex >= 0)
            {
                int targetIndex = lastIndex + 1;
                if (targetIndex >= Count)
                {
                    targetIndex = Count - 1;
                }
                if (oldIndex != targetIndex)
                {
                    lock (_lockobj)
                    {
                        base.MoveItem(oldIndex, targetIndex);
                    }
                }
            }
            else
            {
                lock (_lockobj)
                {
                    base.MoveItem(oldIndex, 0);//最初に移動
                }
            }
        }

        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= OnPropertyChanged;
            lock (_lockobj)
            {
                base.RemoveItem(index);
            }
        }

        /// <summary>
        /// クリア
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.PropertyChanged -= OnPropertyChanged; // イベント変更通知を解除
            }
            lock (_lockobj)
            {
                base.ClearItems();
            }
        }

        /// <summary>
        /// セット
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            lock (_lockobj)
            {
                base.MoveItem(index, 0);//第二引数は使わないので適当
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is T item)
            {
                MoveItem(IndexOf(item), 0);
            }
        }
    }
}
