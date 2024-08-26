using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine.Events
{
    public sealed class UnitConstructedEventArgs : EventArgs
    {
        public City City { get; private set; }
        public UnitType Type { get; private set; }
        public Civilization Owner { get; private set; }
        public Tile StartTile { get; private set; }
        public int InitialExperience { get; private set; }

        public UnitConstructedEventArgs(City city, UnitType type, Civilization owner, Tile startTile, int initialExperience)
        {
            City = city;
            Type = type;
            StartTile = startTile;
            Owner = owner;
            InitialExperience = initialExperience;
        }
    }
}
