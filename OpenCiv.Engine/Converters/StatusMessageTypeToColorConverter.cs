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
    public sealed class StatusMessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StatusMessageType messageType = (StatusMessageType)value;

            switch(messageType)
            {
                case StatusMessageType.BuildingConstructed:
                    return Brushes.LightBlue;
                case StatusMessageType.CityFounded:
                    return Brushes.AliceBlue;
                case StatusMessageType.CityGainedCapture:
                    return Brushes.AliceBlue;
                case StatusMessageType.CityGainedRazed:
                    return Brushes.AliceBlue;
                case StatusMessageType.CityLostCapture:
                    return Brushes.AliceBlue;
                case StatusMessageType.CityLostRazed:
                    return Brushes.AliceBlue;
                case StatusMessageType.GameOver:
                    return Brushes.Red;
                case StatusMessageType.GameStarted:
                    return Brushes.LightGreen;
                case StatusMessageType.Generic:
                    return Brushes.WhiteSmoke;
                case StatusMessageType.ImprovementBuilt:
                    return Brushes.LightBlue;
                case StatusMessageType.Research:
                    return Brushes.CornflowerBlue;
                case StatusMessageType.TurnComplete:
                    return Brushes.LightGreen;
                case StatusMessageType.UnitConstructed:
                    return Brushes.LightBlue;
                case StatusMessageType.UnitLost:
                    return Brushes.Red;
                case StatusMessageType.CombatReport:
                    return Brushes.Tomato;
                case StatusMessageType.UnitPromotion:
                    return Brushes.LightBlue;
                case StatusMessageType.UnitVictorious:
                    return Brushes.Aquamarine;
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
