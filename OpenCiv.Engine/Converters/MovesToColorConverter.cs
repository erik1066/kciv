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
    public sealed class MovesToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.WhiteSmoke;

            int moves = 0;

            bool success = int.TryParse(value.ToString(), out moves);
            if (!success) return Brushes.WhiteSmoke;

            if (moves == 0) { return Brushes.Tomato; }
            return Brushes.WhiteSmoke;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
