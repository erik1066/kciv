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
    public sealed class UnitAllegianceToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return $"units/blank.png";
            string name = value.ToString();

            if (string.IsNullOrEmpty(name)) return $"units/blank.png";

            switch (name)
            {
                case "Barbarians":
                    return $"units/bg_red.bmp";
                default:
                    return $"units/bg_purple.bmp";
            }
            
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class UnitAllegianceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;
            string name = value.ToString();

            if (string.IsNullOrEmpty(name)) return Brushes.Transparent;

            switch (name)
            {
                case "Barbarians":
                    return Brushes.Red;
                case "Romans":
                    return Brushes.Purple;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
