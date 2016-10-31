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
    [ValueConversion(typeof(DateTime), typeof(string)), ValueConversion(typeof(TimeSpan), typeof(string))]
    class DateTimeOrTimeSpanStringConverter : ConverterMarkupExtension<DateTimeOrTimeSpanStringConverter>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                TimeSpan span = (TimeSpan)value;
                try
                {
                    string options = string.Empty;
                    try { if (parameter != null) options = parameter.ToString(); }
                    catch { }
                    return span.ToUserFriendlyString(options, culture);
                }
                catch (Exception ex)
                {
                    log.Warn(string.Format("Issue converting time span: [{0}]", span), ex);
                    return span.ToString();
                }
            }
            if (value is DateTime)
            {
                DateTime date = (DateTime)value;
                try { return date.Humanize(false, culture: culture); }
                catch (Exception ex)
                {
                    log.Warn(string.Format("Issue converting date time: [{0}]", value), ex);
                    return date.ToString();
                }
            }
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class TimeSpanExtensions
    {
        /// <summary>
        /// Constructs a user-friendly string for this TimeSpan instance.
        /// </summary>
        public static string ToUserFriendlyString(this TimeSpan span, string parameter = "2", CultureInfo culture = null)
        {
            // Try parsing parameter as an integer for precision
            int precision;
            if (int.TryParse(parameter, out precision)) return span.Humanize(precision, culture);

            // Set default value for precision to 2
            precision = 2;

            // Try parsing parameter as a TimeUnit for min unit
            TimeUnit minUnit;
            if (Enum.TryParse(parameter, true, out minUnit)) return span.Humanize(precision, culture, minUnit: minUnit);

            return span.Humanize(precision, culture);
        }
    }
}
