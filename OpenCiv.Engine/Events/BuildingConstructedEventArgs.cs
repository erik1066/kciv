using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine.Events
{
    public sealed class BuildingConstructedEventArgs : EventArgs
    {
        public City City { get; private set; }
        public BuildingType Type { get; private set; }
        public Civilization Owner { get; private set; }

        public BuildingConstructedEventArgs(City city, BuildingType type, Civilization owner)
        {
            City = city;
            Type = type;
            Owner = owner;
        }
    }
}
