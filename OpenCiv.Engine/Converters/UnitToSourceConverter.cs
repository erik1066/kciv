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
    public sealed class UnitToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return $"units/blank.png";

            UnitType unitType = (UnitType)value;

            switch (unitType)
            {
                case UnitType.Archer:
                    return $"units/archer1.png";
                case UnitType.Crossbowman:
                    return $"units/crossbowman.png";
                case UnitType.Axeman:
                    return $"units/axeman2.png";
                case UnitType.Bowman:
                    return $"units/bowman1.png";
                case UnitType.Warrior:
                    return $"units/spearman1.png";
                case UnitType.Builder:
                    return $"units/worker.png";
                case UnitType.Catapult:
                    return $"units/catapult.png";
                case UnitType.Chariot:
                    return $"units/chariot2.png";
                case UnitType.HorseArcher:
                    return $"units/horse-archer1.png";
                case UnitType.Horseman:
                    return $"units/horseman1.png";
                case UnitType.Knight:
                    return $"units/knight.png";
                case UnitType.Swordsman:
                    return $"units/swordsman1.png";
                case UnitType.Pikeman:
                    return $"units/pikeman1.png";
                case UnitType.Legion:
                    return $"units/legion.png";
                case UnitType.Longswordsman:
                    return $"units/longswordsman.png";
                case UnitType.Spearman:
                    return $"units/spearman2.png";
                case UnitType.Slinger:
                    return $"units/slinger.png";
                case UnitType.Crusader:
                    return $"units/crusader.png";
                case UnitType.Settler:
                    return $"units/settler.png";
                case UnitType.BarbarianLeader:
                    return $"units/barbarian-leader.png";
            }

            return $"units/blank.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
