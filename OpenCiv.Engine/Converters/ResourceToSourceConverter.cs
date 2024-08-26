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
    public sealed class ResourceToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            ResourceType resource = (ResourceType)value;

            switch (resource)
            {
                case ResourceType.Wheat:
                    return $"terrain/resource-wheat.png";
                case ResourceType.Gold:
                    return $"terrain/resource-gold.png";
                case ResourceType.Horse:
                    return $"terrain/resource-horse.png";
                case ResourceType.Iron:
                    return $"terrain/resource-iron.png";
                case ResourceType.Buffalo:
                    return $"terrain/resource-buffalo.png";
                case ResourceType.Citrus:
                    return $"terrain/resource-citrus.png";
            }

            return $"terrain/blank.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
