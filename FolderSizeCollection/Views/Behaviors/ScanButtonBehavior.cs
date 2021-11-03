using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FolderSizeCollection.Views.Behaviors
{
    public class ScanButtonBehavior : Behavior<Button>
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

        public static readonly DependencyProperty IsScanningProperty = DependencyProperty.Register(nameof(IsScanning), typeof(bool), typeof(ScanButtonBehavior), new PropertyMetadata(IsScanningPropertyChanged));

        private static void IsScanningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var be = BindingOperations.GetMultiBindingExpression((d as ScanButtonBehavior).AssociatedObject, System.Windows.Controls.Primitives.ButtonBase.CommandProperty);
            //var be = (d as ScanButtonBehavior).AssociatedObject.GetMul(System.Windows.Controls.Primitives.ButtonBase.CommandProperty);
            be.UpdateTarget();
        }
    }
}
