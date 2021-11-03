using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FolderSizeCollection.Views.Behaviors
{
    public class CursorBehavior : Behavior<UIElement>
    {
        public bool IsScanning
        {
            get
            {
                return (bool)GetValue(IsScanningProperty);
            }
            set
            {
                SetValue(IsScanningProperty, value);
            }
        }

        public static readonly DependencyProperty IsScanningProperty = DependencyProperty.Register(nameof(IsScanning), typeof(bool), typeof(CursorBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.QueryCursor += AssociatedObject_QueryCursor;
        }

        private void AssociatedObject_QueryCursor(object sender, System.Windows.Input.QueryCursorEventArgs e)
        {
            if (IsScanning)
            {
                e.Cursor = Cursors.Wait;
                e.Handled = true;
            }
        }
    }
}
