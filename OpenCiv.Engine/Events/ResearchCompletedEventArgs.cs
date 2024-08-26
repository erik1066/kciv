using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine.Events
{
    public sealed class ResearchCompletedEventArgs : EventArgs
    {
        public Civilization Civilization { get; private set; }
        public Tech TechResearched { get; private set; }

        public ResearchCompletedEventArgs(Civilization civ, Tech techResearched)
        {
            Civilization = civ;
            TechResearched = techResearched;
        }
    }
}
