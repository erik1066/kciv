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
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return System.Windows.Visibility.Collapsed;

            bool result = (bool)value;

            bool flip = false;

            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out flip);
            }

            if (result == true)
            {
                return flip == false ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            else 
            {
                return flip == false ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
