using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TroveTools.NET.Converter
{
    /// <summary>
    /// Returns a Visibility object depending on whether the value passed is null and the passed parameter to select between collapsed and hidden 
    /// </summary>
    class NullToVisibilityConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string defaultValue = parameter as string;
                Visibility invisibility = Visibility.Collapsed;
                try { if (!string.IsNullOrEmpty(defaultValue)) invisibility = (Visibility)Enum.Parse(typeof(Visibility), defaultValue); }
                catch (Exception ex) { log.Warn(string.Format("Invalid visibility parameter: [{0}]", parameter), ex); }

                if (value == null) return invisibility;
                if (value is string && string.IsNullOrWhiteSpace(value as string)) return invisibility;
                if (value.Equals(GetDefault(value.GetType()))) return invisibility;
            }
            catch (Exception ex) { log.Error(string.Format("Error converting value to visibility: [{0}]", value), ex); }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private object GetDefault(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }
    }
}
