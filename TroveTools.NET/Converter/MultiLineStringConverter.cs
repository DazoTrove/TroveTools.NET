using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TroveTools.NET.Converter
{
    class MultiLineStringConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = value as string;
            if (string.IsNullOrEmpty(source)) return null;
            return Regex.Replace(source, "[\r\n]+", ", ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = value as string;
            if (string.IsNullOrEmpty(source)) return null;
            return source.Replace(", ", Environment.NewLine);
        }
    }
}
