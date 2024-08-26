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
    public sealed class CityAllegianceToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;

            Tile tile = (Tile)value;

            if (!tile.HasCity)
                return Brushes.Transparent;

            City city = tile.City;

            switch (city.Owner.Name)
            {
                case "Barbarians":
                    return Brushes.Red;
                default:
                    return Brushes.Purple;
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
