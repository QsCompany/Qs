using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{

    public class EqAssign : ExtendParse
    {

        public EqAssign(BasicParse basicParse)
            : base(EPNames.EqAssign, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.EqAssign);
            var T = new Tree(BasicParse.Pile, parent, Kind.EqAssign){GeneratedBy = this};
            return
                T.Set(BasicParse.GetHeritachy(T) && BasicParse.GetKeyWord(BasicParse.Temp, "=") &&
                      base[EPNames.Expression][T]);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var dataPortor = scop as DataPortor;
            var l = load.SetVariable(dataPortor, tree[0]);
            tree.BaseCall = dataPortor;
            tree.Type = l.Return;
            tree.Membre = l;
            tree.Compiled = true;
            if (tree.Children.Count == 2) return null;
            var fieldInfo = load.Compile(scop, tree[1]);
            return load.Add("mov", l, fieldInfo);
        }
    }
}