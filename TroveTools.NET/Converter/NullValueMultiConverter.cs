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
    /// <summary>
    /// If the first binding value is null or an empty string, return the second binding value; otherwise return the third binding value
    /// </summary>
    class NullValueMultiConverter : IMultiValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is string) return string.IsNullOrEmpty(values[0] as string) ? values[1] : values[2];
                return values[0] == null ? values[1] : values[2];
            }
            catch (Exception ex) { log.Error(string.Format("Error checking for null value (multi converter): [{0}]", values), ex); }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
