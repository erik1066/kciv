using System;
using System.Collections.Generic;

namespace OpenCiv.Engine {
    public sealed class Tech {

        private readonly List<Tech> _prereqs = new List<Tech>();

        public string Name {get;private set;}

        public IList<Tech> Prereqs { 
            get {
                return _prereqs.AsReadOnly();
            }
        }

        public double Science { get; private set; }

        public Tech(string name, int science) {
            Name = name;
            Science = science;
        }

        public void AddPrereq(Tech prereq) {
            if (prereq == this) throw new ArgumentException(nameof(prereq));
            if (_prereqs.Contains(prereq)) throw new ArgumentException(nameof(prereq));

            _prereqs.Add(prereq);
        }
    }
}