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
    class TimeSpanToUserFriendlyStringConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                try
                {
                    TimeSpan ts = (TimeSpan)value;
                    return ts.ToUserFriendlyString();
                }
                catch (Exception ex) { log.Error(string.Format("Error converting time span: [{0}]", value), ex); }
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
        public static string ToUserFriendlyString(this TimeSpan span)
        {
            const int DaysInYear = 365;
            const int DaysInMonth = 30;

            List<string> values = new List<string>();

            int days = span.Days;
            if (days >= DaysInYear)
            {
                int years = (days / DaysInYear);
                values.Add(CreateValueString(years, "year"));
                days = (days % DaysInYear);
            }
            if (days >= DaysInMonth)
            {
                int months = (days / DaysInMonth);
                values.Add(CreateValueString(months, "month"));
                days = (days % DaysInMonth);
            }
            if (days >= 1) values.Add(CreateValueString(days, "day"));
            if (span.Hours >= 1) values.Add(CreateValueString(span.Hours, "hour"));
            if (span.Minutes >= 1) values.Add(CreateValueString(span.Minutes, "minute"));

            // Number of seconds (include when 0 if no other components included)
            if (span.Seconds >= 1 || values.Count == 0) values.Add(CreateValueString(span.Seconds, "second"));

            // Combine values into string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < values.Count; i++)
            {
                if (builder.Length > 0) builder.Append(i == values.Count - 1 ? " and " : ", ");
                builder.Append(values[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Constructs a string description of a time-span value.
        /// </summary>
        /// <param name="value">The value of this item</param>
        /// <param name="description">The name of this item (singular form)</param>
        private static string CreateValueString(int value, string description)
        {
            return string.Format("{0:#,##0} {1}", value, (value == 1) ? description : string.Format("{0}s", description));
        }
    }
}
