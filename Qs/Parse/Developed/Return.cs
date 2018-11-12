using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Return : ExtendParse
    {

        public Return(BasicParse basicParse) : base(EPNames.Return, basicParse)
        {
        }
        static bool Stop(bool cond,bool b)
        {
            if(cond==b)
            {

            }
            return b;
        }
        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Return);
            var T = new Tree(BasicParse.Pile, parent, Kind.Return){GeneratedBy = this};
            return T.Set(Stop(true, BasicParse.GetKeyWord(T, "return")) && base[EPNames.Expression].Parse(T) | true);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var ret = load.Compile(scop, tree[1]); //.GeneratedBy.Compile(load, scop, tree[0]);
            load.Return(scop, ret);
            return null;
        }
    }
}

namespace Internet
{
}