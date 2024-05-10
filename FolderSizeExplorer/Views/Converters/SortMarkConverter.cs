using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FolderSizeExplorer.Views.Converters
{
    public class SortMarkConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values">
        /// 1: string - Name of the column bind property<br/>
        /// 2: string - Name of the sort<br/>
        /// 3: bool - Is ascending<br/>
        /// </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
            {
                return string.Empty;
            }

            string column = (string)values[0];
            string sortColumn = (string)values[1];
            bool isAscending = (bool)values[2];

            if (column == sortColumn)
            {
                return isAscending ? "▲" : "▼";
            }
            return string.Empty;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
