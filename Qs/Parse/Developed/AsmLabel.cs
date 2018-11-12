using System.Collections.Generic;

namespace Qs.Parse.Developed
{
    public class AsmLabel
    {
        private int _location;
        public readonly string Name;
        private bool _freezed;
        public readonly List<AsmGoto> Goto = new List<AsmGoto>();

        public AsmLabel(string name)
        {
            Name = name;
        }

        public int Location
        {
            get { return _location; }
            set { if(!_freezed)_location = value; }
        }

        public bool Freezed
        {
            get { return _freezed; }
            set
            {
                if (!Freezed)
                    _freezed = value;
            }
        }

        internal void ReSet(int location)
        {
            _location = location;
        }
    }
}