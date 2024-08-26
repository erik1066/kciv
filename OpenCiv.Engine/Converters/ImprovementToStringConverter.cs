using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenCiv.Engine.Converters
{
    public sealed class ImprovementToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            ImprovementType improvement = (ImprovementType)value;

            if (improvement != ImprovementType.None)
            {
                return improvement.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
