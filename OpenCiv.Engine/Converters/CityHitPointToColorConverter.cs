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
    public sealed class CityHitPointToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;

            double pct = 0.0;

            bool success = double.TryParse(value.ToString(), out pct);            
            if (!success) return Brushes.Transparent;

            double cutoff1 = 0.7;
            double cutoff2 = 0.4;
            
            if (pct >= cutoff1) return Brushes.Green;
            else if (pct < cutoff1 && pct >= cutoff2) return Brushes.Yellow;
            else if (pct <= cutoff2) return Brushes.Red;            

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
