using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenCiv.Engine.Converters
{
    public sealed class ImprovementToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            ImprovementType improvement = (ImprovementType)value;

            if (improvement == ImprovementType.Farms)
            {
                //return $"terrain/0011_{r}.bmp";
                return $"terrain/improvement-farm.png";
            }
            if (improvement == ImprovementType.Mines)
            {
                return $"terrain/improvement-mine.png";
            }
            if (improvement == ImprovementType.Fortress)
            {
                return $"terrain/fortress.png";
            }

            return $"terrain/blank.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
