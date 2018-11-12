using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class TypeAssign : ExtendParse
    {
        public TypeAssign(BasicParse basicParse)
            : base(EPNames.TypeAssign, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.TypeAssigne);
            var T = new Tree(BasicParse.Pile, parent, Kind.TypeAssigne){GeneratedBy = this};
            var b = BasicParse.GetHeritachy(T) && BasicParse.GetVariable(T) &&
                    (!BasicParse.GetKeyWord(BasicParse.Temp, "=") || this[EPNames.Expression][T]);
            return T.Set(b & (!GetProperty<bool>("endWithComa") || (KeyWord(Ts, ";"))));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            if (tree.Count == 3 && tree[2].Kind == Kind.Numbre)
            {
                
            }
            var dataPortor = scop as DataPortor;
            var l = load.SetVariable(dataPortor, tree);
            tree[1].BaseCall= tree.BaseCall = dataPortor;
            tree[1].Type=tree.Type = l.Return;
            tree[1].Membre = tree.Membre = l;
            tree[1].Compiled = true;
            tree.Compiled = true;
            if (tree.Children.Count == 2) return null;
            var fieldInfo = load.Compile(scop, tree[2]);
            if (l != fieldInfo)
            {
                load.Optimum.Assign(l, fieldInfo);
            }
            return l;
        }
    }
}