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
    public sealed class HitPointToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double hp = 0.0;
            double max = 100.0;

            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), out max);
            }

            double cutoff1 = max * 0.70;
            double cutoff2 = max * 0.40;

            if (double.TryParse(value.ToString(), out hp))
            {
                if (hp >= cutoff1) return Brushes.Green;
                else if (hp < cutoff1 && hp >= cutoff2) return Brushes.Yellow;
                else if (hp <= cutoff2) return Brushes.Red;
            }

            return Brushes.Black;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
