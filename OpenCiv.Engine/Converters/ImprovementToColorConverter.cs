using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenCiv.Engine.Converters
{
    public sealed class ImprovementToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;

            ImprovementType improvement = (ImprovementType)value;

            if (improvement == ImprovementType.None)
            {
                return Brushes.Transparent;
            }
            else if (improvement == ImprovementType.Fortress)
            {
                return Brushes.Yellow;
            }

            return Brushes.WhiteSmoke;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
