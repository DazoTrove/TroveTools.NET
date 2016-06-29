using Humanizer;
using Humanizer.Localisation;
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
    class DateTimeOrTimeSpanStringConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                try
                {
                    TimeSpan span = (TimeSpan)value;
                    string options = string.Empty;
                    try { if (parameter != null) options = parameter.ToString(); }
                    catch { }
                    return span.ToUserFriendlyString(options);
                }
                catch (Exception ex) { log.Error(string.Format("Error converting time span: [{0}]", value), ex); }
            }
            if (value is DateTime)
            {
                try
                {
                    DateTime date = (DateTime)value;
                    return date.Humanize(false);
                }
                catch (Exception ex) { log.Error(string.Format("Error converting date time: [{0}]", value), ex); }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class TimeSpanExtensions
    {
        /// <summary>
        /// Constructs a user-friendly string for this TimeSpan instance.
        /// </summary>
        public static string ToUserFriendlyString(this TimeSpan span, string parameter = "2")
        {
            // Try parsing parameter as an integer for precision
            int precision;
            if (int.TryParse(parameter, out precision)) return span.Humanize(precision, collectionSeparator: null);

            // Set default value for precision to 2
            precision = 2;

            // Try parsing parameter as a TimeUnit for min unit
            TimeUnit minUnit;
            if (Enum.TryParse(parameter, true, out minUnit)) return span.Humanize(precision, minUnit: minUnit, collectionSeparator: null);

            return span.Humanize(precision, collectionSeparator: null);
        }
    }
}
