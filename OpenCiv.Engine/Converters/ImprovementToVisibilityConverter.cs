using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenCiv.Engine.Converters
{
    public sealed class ImprovementToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return System.Windows.Visibility.Collapsed; 

            ImprovementType improvement = (ImprovementType)value;

            if (improvement != ImprovementType.None)
            {
                return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
