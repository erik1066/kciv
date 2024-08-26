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
    public sealed class AttackMethodToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return System.Windows.Visibility.Hidden;

            AttackMethod method = (AttackMethod)value;

            bool isRangedOutput = false;
            if (parameter != null) isRangedOutput = bool.TryParse(parameter.ToString(), out isRangedOutput);

            if (method == AttackMethod.None)
            {
                return System.Windows.Visibility.Hidden;
            }

            if (method == AttackMethod.Melee && isRangedOutput)
            {
                return System.Windows.Visibility.Hidden;
            }

            else return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
