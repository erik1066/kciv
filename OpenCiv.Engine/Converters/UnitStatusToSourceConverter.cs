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
    public sealed class UnitStatusToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Tile tile = (Tile)value;

            //if (!tile.HasUnit)
            //    return $"units/blank.png";

            //Unit unit = tile.CurrentUnit;

            if (value == null) { return $"units/blank.png"; }

            UnitStatus status = (UnitStatus)value;

            switch (status)
            {
                case UnitStatus.Fortifying:
                    return $"terrain/fortifying.png";
                case UnitStatus.Fortified:
                    return $"terrain/fortified.png";
                default:
                    return $"units/blank.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
