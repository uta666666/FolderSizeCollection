using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xaml;

namespace FolderSizeExplorer.Views.Converters
{
    public class FileSizeBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double colSize = (double)values[0];
                long maxValue = (long)values[1];
                long value = (long)values[2];

                var x = colSize * value / maxValue;
                return x * 0.9;
            }
            catch
            {
                return 0;
            }
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
