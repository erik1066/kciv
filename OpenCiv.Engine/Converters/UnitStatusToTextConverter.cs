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
    public sealed class UnitStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            UnitStatus unitStatus = (UnitStatus)value;

            string status = string.Empty;

            switch (unitStatus)
            {
                case UnitStatus.Fortifying:
                    status = "Fortifying";
                    break;
                case UnitStatus.Fortified:
                    status = "Fortified";
                    break;
                case UnitStatus.Sentry:
                    status = "Sentry duty";
                    break;
                case UnitStatus.Working:
                    status = "Working";
                    break;
                case UnitStatus.None:
                    status = "Active";
                    break;
            }

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
