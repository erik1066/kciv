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
    public sealed class RangedAttackToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return $"terrain/blank.png";

            bool isInRange = (bool)value;
            
            if (isInRange)
            {
                return $"terrain/target.png";
            }

            return $"terrain/blank.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
