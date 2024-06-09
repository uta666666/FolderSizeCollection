using FolderSizeExplorer.Models;
using Livet;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FolderSizeExplorer.Views.Behaviors
{
    public class MultiSelectBehavior : Behavior<ListView>
    {
        public ObservableCollection<AbstractFileData> SelectedItems
        {
            get { return (ObservableCollection<AbstractFileData>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(ObservableCollection<AbstractFileData>), typeof(MultiSelectBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems == null)
            {
                return;
            }
            SelectedItems.Clear();
            foreach (var item in AssociatedObject.SelectedItems)
            {
                SelectedItems.Add((AbstractFileData)item);
            }
        }
    }
}
