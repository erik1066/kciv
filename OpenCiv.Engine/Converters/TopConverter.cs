using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using OpenCiv.Engine;

namespace OpenCiv.Engine.Converters
{
    public sealed class TopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Coords coords = (Coords)value;
            int left = coords.Y;
            int top = coords.X;

            int offset = left % 2 == 0 ? 0 : 20;

            return offset + (top * 39);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
