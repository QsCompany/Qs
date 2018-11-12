using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class ArrayCaller : ExtendParse
    {
        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }

        public ArrayCaller(BasicParse basicParse)
            : base(EPNames.ArrayCaller, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Caller);
            var T = new Tree(BasicParse.Pile, parent, Kind.Caller){GeneratedBy = this};
            return T.Set(BasicParse.GetHeritachy(T) && ArrayParameter(T));
        }

        public bool ArrayParameter(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Param);
            var T = new Tree(BasicParse.Pile, parent, Kind.Param){GeneratedBy = this};
            bool end;
            var vB = (end = BasicParse.GetKeyWord(BasicParse.Temp, "[")) && base[EPNames.Expression].Parse(T);
            while (vB && BasicParse.GetKeyWord(BasicParse.Temp, ",") && base[EPNames.Expression].Parse(T))
            {
            }
            return T.Set(end && BasicParse.GetKeyWord(BasicParse.Temp, "]"));
        }

    }
}