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
    public sealed class RoadAltToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return $"units/blank.png";

            string roads = value.ToString();

            if (string.IsNullOrEmpty(roads) || !roads.StartsWith("t")) return $"units/blank.png";
            if (roads.Equals("t", StringComparison.OrdinalIgnoreCase)) return $"Images/Roads/road_n_se_sw.png";

            if (roads.Length >= 3)
            {
                roads = roads.Substring(2);

                string[] connections = roads.Split('_');

                if (connections.Length == 4)
                {
                    return $"Images/Roads/road_{connections[2]}_{connections[3]}.png";
                }
                else if (connections.Length == 3)
                {
                    return $"Images/Roads/road_{connections[1]}_{connections[2]}.png";
                }
            }
            return $"units/blank.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
