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
    [ValueConversion(typeof(string), typeof(string))]
    class ImagePathConverter : ConverterMarkupExtension<ImagePathConverter>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string source = value as string;
                if (string.IsNullOrEmpty(source)) return null;
                return new Uri(new Uri(TrovesaurusApi.TrovesaurusBaseUrl), source).ToString();
            }
            catch (Exception ex) { log.Error(string.Format("Error converting image path: [{0}]", value), ex); }
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string url = value as string;
                if (string.IsNullOrEmpty(url)) return null;
                return new Uri(url).MakeRelativeUri(new Uri(TrovesaurusApi.TrovesaurusBaseUrl)).ToString();
            }
            catch (Exception ex) { log.Error(string.Format("Error converting image path back: [{0}]", value), ex); }
            return null;
        }
    }
}
