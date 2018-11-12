using Qs.Interfaces;

namespace Qs.Structures
{
    public abstract class MembreInfo:IScop
    {
        public string Name;
        public Class Return;
        protected int _offset;

        public virtual int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }
    }
}
