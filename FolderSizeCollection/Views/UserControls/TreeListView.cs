using System;
using System.Windows.Controls;
using System.Windows;


namespace FolderSizeCollection.Views.UserControls
{
    public class TreeListView : TreeView
    {
        public TreeListView()
        {   
            this.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
            this.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);

            this.SelectedItemChanged += this.OnSelectedItemChanged;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }





        public static readonly DependencyProperty BindableSelectedItemProperty = DependencyProperty.Register(nameof(BindableSelectedItem), typeof(object), typeof(TreeListView), new UIPropertyMetadata(null));

        /// <summary>
        /// Bind 可能な SelectedItem を表し、SelectedItem を設定または取得します。
        /// </summary>
        public object BindableSelectedItem
        {
            get { return (object)GetValue(BindableSelectedItemProperty); }
            set { SetValue(BindableSelectedItemProperty, value); }
        }

        protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem == null)
            {
                return;
            }

            SetValue(BindableSelectedItemProperty, SelectedItem);
        }
    }

    public class TreeListViewItem : TreeViewItem
    {
        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        public int Level {
            get {
                if (_level == -1)
                {
                    TreeListViewItem parent =
                        ItemsControl.ItemsControlFromItemContainer(this)
                            as TreeListViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }


        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        private int _level = -1;
    }

}