using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TroveTools.NET.Converter
{
    class SortColumnToVisibilityConverter : IMultiValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string direction = parameter as string;
            string property = values[0] as string;
            string sortBy = values[1] as string;
            ListSortDirection? sortDirection = values[2] as ListSortDirection?;

            //log.DebugFormat("Sorting indicator visibility: direction = [{0}], property = [{1}], sorting dir = [{2}], sorting by = [{3}]",
            //    direction, property, sortDirection, sortBy);

            if (direction == null || property == null || sortDirection == null || sortBy == null) return Visibility.Collapsed;

            return (sortBy == property && sortDirection.ToString().Equals(direction, StringComparison.OrdinalIgnoreCase)) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
