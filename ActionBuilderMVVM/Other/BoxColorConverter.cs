using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using ActionBuilderMVVM.Models;
using Brushes = System.Drawing.Brushes;

namespace ActionBuilderMVVM.Other
{
    class BoxColorConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            switch ((Box.BoxType) value)
            {
                case Box.BoxType.Hit: return new SolidColorBrush(Colors.Red);
                case Box.BoxType.Hurt: return new SolidColorBrush(Colors.Lime);
                case Box.BoxType.Grab: return new SolidColorBrush(Colors.Red);
                case Box.BoxType.Armor: return new SolidColorBrush(Colors.Red);
                case Box.BoxType.Collision: return new SolidColorBrush(Colors.Red);
                case Box.BoxType.Data: return new SolidColorBrush(Colors.Red);
                case Box.BoxType.Null: return new SolidColorBrush(Colors.Red);
                default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
            } 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
