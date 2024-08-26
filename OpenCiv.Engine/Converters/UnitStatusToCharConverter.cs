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
    public sealed class UnitStatusToCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            
            UnitStatus unitStatus = (UnitStatus)value;

            string status = string.Empty;

            switch (unitStatus)
            {
                case UnitStatus.Fortifying:
                    return "F";
                case UnitStatus.Fortified:
                    return "F";
                case UnitStatus.Sentry:
                    return "S";
                case UnitStatus.Working:
                    return "B";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
