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
    public sealed class CityHitPointToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0.0;

            double mod = 15.0;

            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), out mod);
            }

            double pct = 0.0;

            bool success = double.TryParse(value.ToString(), out pct);
            if (!success) return Brushes.Transparent;

            return pct * mod;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
