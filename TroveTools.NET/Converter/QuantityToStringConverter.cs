using Humanizer;
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
    class QuantityToStringConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string unit = parameter as string;
                int number = System.Convert.ToInt32(value);
                return unit.ToQuantity(number, "N0");
            }
            catch (Exception ex) { log.Error(string.Format("Error converting string quantity: [{0}]", value), ex); }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
