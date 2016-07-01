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
    class UnixTimeSecondsToDateTimeConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                long source;
                if (long.TryParse(value?.ToString(), out source)) return GetLocalDateTime(source);
            }
            catch (Exception ex) { log.Error(string.Format("Error converting unix time to date and time: [{0}]", value), ex); }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            DateTime source = (DateTime)value;
            return GetUnixSeconds(source);
        }

        public static DateTime GetLocalDateTime(string unixTimeSeconds)
        {
            return GetLocalDateTime(long.Parse(unixTimeSeconds));
        }

        public static DateTime GetLocalDateTime(long unixTimeSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).LocalDateTime;
        }

        public static long GetUnixSeconds(DateTime localDateTime)
        {
            return new DateTimeOffset(localDateTime).ToUnixTimeSeconds();
        }
    }
}
