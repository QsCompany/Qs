using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class DeclaredParams : ExtendParse
    {

        public DeclaredParams(BasicParse basicParse)
            : base(EPNames.DeclaredParams, basicParse)
        {

        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.DeclareParams);
            var T = new Tree(BasicParse.Pile, parent, Kind.DeclareParams){GeneratedBy = this};
            bool end;
            var vB = (end = BasicParse.GetKeyWord(BasicParse.Temp, "(")) && DeclaredParam(T);
            while (vB && BasicParse.GetKeyWord(BasicParse.Temp, ",") && DeclaredParam(T)) ;
            return T.Set(end && BasicParse.GetKeyWord(BasicParse.Temp, ")"));
        }

        private bool DeclaredParam(Tree parent)
        {
            BasicParse.Pile.Save(Kind.DeclareParam);
            var T = new Tree(BasicParse.Pile, parent, Kind.DeclareParam){GeneratedBy = this};
            return T.Set(BasicParse.GetHeritachy(T) && BasicParse.GetVariable(T));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var method = scop as MethodInfo;
            var l = load.SetVariable(method, tree);
            tree.BaseCall = method;
            tree.Type = l.Return;
            tree.Membre = l;
            tree.Compiled = true;
            if (tree.Children.Count == 2) return l;
            return l;
        }
    }
}