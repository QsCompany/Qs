using Qs.Enumerators;
using Qs.Interfaces;
using Qs.Structures;

namespace Qs.Utils.Indexation.Find
{
    public class HeritachyType
    {
        public string name;
        public IScop Scop;
        public HeritachyType Children;
        public HeritachyType(string name, IScop iscop)
        {
            this.name = name;
            Scop = iscop;
        }
        public HeritachyType(HeritachyType parent, string name, IScop iscop)
        {
            this.name = name;
            Scop = iscop;
            if (parent != null) parent.Children = this;
        }

        public Class Type
        {
            get
            {
                return Genre == Genre.Type ? Scop as Class : (Genre == Genre.Method ? (Scop as MethodInfo).Return : (Genre == Genre.None ? null : Scop as FieldInfo).Return);
            }
        }

        public Genre Genre
        {
            get
            {
                if (Scop is Var) return Genre.Variable;
                if (Scop is Class) return Genre.Type;
                if (Scop is MethodInfo) return Genre.Method;
                return Genre.None;
            }
        }

        public HeritachyType Add(string _name, IScop iScop)
        {
            return new HeritachyType(this, _name, iScop);
        }

    }
}
