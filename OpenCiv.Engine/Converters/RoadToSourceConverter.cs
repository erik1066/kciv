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
    public sealed class RoadToSourceConverter : IValueConverter
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

                if (connections.Length >= 2 && connections.Length <= 6 && connections.Length != 4 && connections.Length != 3)
                {
                    return $"Images/Roads/road_{roads}.png";
                }
                else if (connections.Length == 4)
                {
                    // just get the first 3 connections, let the alt converter handle adding 2nd layer
                    return $"Images/Roads/road_{connections[0]}_{connections[1]}.png";
                }
                else if (connections.Length == 3)
                {
                    // just get the first 2 connections, let the alt converter handle adding 2nd layer
                    return $"Images/Roads/road_{connections[0]}_{connections[1]}.png";
                }
                else if (connections.Length == 1)
                {
                    string connection = connections[0];

                    if (connection.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                        connection.Equals("s", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Images/Roads/road_n_s.png";
                    }

                    if (connection.Equals("ne", StringComparison.OrdinalIgnoreCase) ||
                        connection.Equals("sw", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Images/Roads/road_ne_sw.png";
                    }

                    if (connection.Equals("nw", StringComparison.OrdinalIgnoreCase) ||
                        connection.Equals("se", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Images/Roads/road_se_nw.png";
                    }
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
