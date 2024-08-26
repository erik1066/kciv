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
    public sealed class TotalCombatPowerToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double power = 0;

            if (double.TryParse(value.ToString(), out power))
            {
                if (power <= 25)
                {
                    return "Worthless";
                }
                else if (power > 25 && power <= 50)
                {
                    return "Pathetic";
                }
                else if (power > 50 && power <= 100)
                {
                    return "Aenmic";
                }
                else if (power > 100 && power <= 150)
                {
                    return "Weak";
                }
                else if (power > 150 && power <= 350)
                {
                    return "Average";
                }
                else if (power > 350 && power <= 500)
                {
                    return "Capable";
                }
                else if (power > 500 && power <= 750)
                {
                    return "Formidible";
                }
                else if (power > 750 && power <= 1000)
                {
                    return "Mighty";
                }
                else if (power > 1000 && power <= 2000)
                {
                    return "Terror of the Earth";
                }
                else if (power > 2000)
                {
                    return "Unstoppable";
                }
            }
            return "Pathetic";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
