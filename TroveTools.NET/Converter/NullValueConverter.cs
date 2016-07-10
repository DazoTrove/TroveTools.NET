using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TroveTools.NET.Converter
{
    [ValueConversion(typeof(object), typeof(object))]
    class NullValueConverter : ConverterMarkupExtension<NullValueConverter>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string) return string.IsNullOrEmpty(value as string) ? parameter : value;
                return value == null ? parameter : value;
            }
            catch (Exception ex) { log.Error(string.Format("Error checking for null value: [{0}]", value), ex); }
            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
