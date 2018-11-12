using System.Collections.Generic;
using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class MethodCaller : ExtendParse
    {

        public MethodCaller(BasicParse basicParse)
            : base(EPNames.MethodCaller, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Caller);
            var T = new Tree(BasicParse.Pile, parent, Kind.Caller){GeneratedBy = this};
            var CallParameter = base[EPNames.CallParameter];
            return T.Set(BasicParse.GetHeritachy(T) && CallParameter.Parse(T));
        }
        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            if (tree.Method == null)
                tree.Method = load.GetMethod ( tree[0], tree[1].Children );
            if (tree.Method == null) {
                load.LogIn ( scop, tree, this, "Function(" + tree[0].Content + ") Entrer cannot be found" );
                return null;}
            if (tree.Method.ISCPUMethod)
            {
                var parm = new List<FieldInfo>(2);
                foreach (Tree e in tree[1])
                    parm.Add(load.Compile(scop, e));
            }
            foreach (Tree e in tree[1])
            {
                var z = load.Compile ( scop, e );
                load.Optimum.PushParam(z);
            }
            load.Optimum.Call(tree.Method);
            return tree.Membre = RegInfo.eax;
        }
    }
}