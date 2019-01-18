using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActionBuilderMVVM.Other
{
    public class ImageSourceConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(ImageSource))
            {
                if (value is string str)
                {
                    return new BitmapImage(new Uri(str, UriKind.RelativeOrAbsolute));
                }
                else if (value is Uri uri)
                {
                    return new BitmapImage(uri);
                }
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
