using System;
using System.Diagnostics;

namespace OpenCiv.Engine
{
    [DebuggerDisplay("X = {X}, Y = {Y}")]
    public class Coords : ObservableObject
    {        
        private readonly int _x = -1;
        private readonly int _y = -1;

        public Coords(int x, int y) 
        {
            if (x < 0) { throw new ArgumentOutOfRangeException(nameof(x)); }
            if (y < 0) { throw new ArgumentOutOfRangeException(nameof(y)); }

            if (x > 256) { throw new ArgumentOutOfRangeException(nameof(x)); }
            if (y > 256) { throw new ArgumentOutOfRangeException(nameof(y)); }

            _x = x;
            _y = y;

            RaisePropertyChanged(nameof(X));
            RaisePropertyChanged(nameof(Y));
        }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
    }
}
