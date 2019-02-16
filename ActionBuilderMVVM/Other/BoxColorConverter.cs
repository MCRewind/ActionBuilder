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
            switch ((BoxModel.BoxType) value)
            {
                case BoxModel.BoxType.Hit: return new SolidColorBrush(Colors.Red);
                case BoxModel.BoxType.Hurt: return new SolidColorBrush(Colors.Lime);
                case BoxModel.BoxType.Grab: return new SolidColorBrush(Colors.Red);
                case BoxModel.BoxType.Armor: return new SolidColorBrush(Colors.Red);
                case BoxModel.BoxType.Collision: return new SolidColorBrush(Colors.Red);
                case BoxModel.BoxType.Data: return new SolidColorBrush(Colors.Red);
                case BoxModel.BoxType.Null: return new SolidColorBrush(Colors.Red);
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
