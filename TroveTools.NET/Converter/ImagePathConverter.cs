using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.Model;

namespace TroveTools.NET.Converter
{
    class ImagePathConverter : IValueConverter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = value as string;
            if (string.IsNullOrEmpty(source)) return null;
            return new Uri(new Uri(TrovesaurusApi.TrovesaursBaseUrl), source).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string url = value as string;
            if (string.IsNullOrEmpty(url)) return null;
            return new Uri(url).MakeRelativeUri(new Uri(TrovesaurusApi.TrovesaursBaseUrl)).ToString();
        }
    }
}
