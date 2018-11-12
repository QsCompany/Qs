using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Label : ExtendParse
    {

        public Label(BasicParse basicParse)
            : base(EPNames.Label, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {

            BasicParse.Pile.Save(Kind.Label);
            var T = new Tree(BasicParse.Pile, parent, Kind.Label){GeneratedBy = this};
            return T.Set(BasicParse.GetVariable(T) && BasicParse.GetKeyWord(BasicParse.Temp, ":"));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            load.Optimum.SetLabel(tree[0].Content, true);
            return null;
        }
    }
}