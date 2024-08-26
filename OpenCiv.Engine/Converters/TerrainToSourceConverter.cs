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
    public sealed class TerrainToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tile tile = (Tile)value;
            System.Threading.Thread.Sleep(7);
            System.Random rnd = new Random(System.DateTime.Now.Millisecond);
            int r = rnd.Next(1, 5);

            //if (tile.Improvement == ImprovementType.Farms)
            //{
            //    return $"terrain/0011_{r}.bmp";
            //}

            switch (tile.Terrain)
            {
                case TerrainType.Ocean:                                        
                    return $"terrain/0002_{r}.bmp";
                case TerrainType.Coast:                    
                    return $"terrain/0002_{r}.bmp";
                case TerrainType.Grassland:
                    return $"terrain/0006_{r}.bmp";
                case TerrainType.Plains:
                    return $"terrain/0009_{r}.bmp";
                case TerrainType.Forest:
                    return $"terrain/0019_{r}.bmp";
                case TerrainType.ForestHills:
                    return $"terrain/0020_{r}.bmp";
                case TerrainType.Swamp:
                    return $"terrain/0010_{r}.bmp";
                case TerrainType.Hills:
                    if (tile.X <= 6)
                    {
                        return $"terrain/0054_{r}.bmp";
                    }
                    else if (tile.X > 6 && tile.X <= 10)
                    {
                        return $"terrain/0014_{r}.bmp";
                    }
                    else
                    {
                        return $"terrain/0017_{r}.bmp";
                    }
                case TerrainType.Mountains:
                    return $"terrain/0013_{r}.bmp";
                case TerrainType.Desert:
                    return $"terrain/0027_{r}.bmp";
                case TerrainType.Arctic:
                    return $"terrain/0055_{r}.bmp";
                case TerrainType.Tundra:
                    return $"terrain/0056_{r}.bmp";
                case TerrainType.RockDesert:
                    return $"terrain/0026_{r}.bmp";
                default:                    
                    return $"terrain/0002_{r}.bmp";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
