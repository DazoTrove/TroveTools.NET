using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TroveTools.NET.Model;

namespace TroveTools.NET.Converter
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    class UriToCachedImageConverter : ConverterMarkupExtension<ImagePathConverter>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string source = value as string;
                if (string.IsNullOrEmpty(source)) return null;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(source);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch (Exception ex) { log.Error(string.Format("Error converting image path: [{0}]", value), ex); }
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }
}
