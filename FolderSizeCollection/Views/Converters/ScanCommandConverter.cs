using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FolderSizeCollection.Views.Converters
{
    public class ScanCommandConverter : DependencyObject, IMultiValueConverter
    {
        //public bool IsScanning
        //{
        //    get
        //    {
        //        return (bool)GetValue(IsScanningProperty);
        //    }
        //    set
        //    {
        //        SetValue(IsScanningProperty, value);
        //    }
        //}

        //public static readonly DependencyProperty IsScanningProperty = DependencyProperty.Register(nameof(IsScanning), typeof(bool), typeof(ScanCommandConverter), new PropertyMetadata(IsScanningPropertyChanged));

        //private static void IsScanningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    ;
        //}

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isScanning = (bool)values.Last();
            
            if (isScanning)
            {
                return values[1];
            }
            else
            {
                return values[0];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
