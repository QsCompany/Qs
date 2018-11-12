using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Goto : ExtendParse
    {

        public Goto(BasicParse basicParse)
            : base(EPNames.Goto, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Goto);
            var T = new Tree(BasicParse.Pile, parent, Kind.Goto){GeneratedBy = this};
            return !BasicParse.GetKeyWord(T, "goto") ? T.Set(false) : T.Set(BasicParse.GetVariable(T));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            load.Optimum.SetGoto("jmp", tree[0].Content);
            return null;
        }
    }
}