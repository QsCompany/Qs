using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Class : ExtendParse
    {

        public Class(BasicParse basicParse)
            : base(EPNames.Class, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            var instruction = base[EPNames.Instruction];
            var expression = base[EPNames.Expression];
            var TypeAssign = base[EPNames.TypeAssign].SetProperty("endWithComa", true);
            var Constructor = base[EPNames.Constructor];
            var Function = base[EPNames.Function];
            Pile.Save(Kind.Class);
            var i = false;
            var vB = KeyWord(Ts, "class") || (i = KeyWord(Ts, "struct"));

            var T = new Tree(Pile, parent, i ? Kind.Struct : Kind.Class){GeneratedBy = this};
            vB = vB && BasicParse.GetVariable(T);
            if (!i && vB && KeyWord(T, ":")) vB = BasicParse.GetHeritachy(T);
            vB = vB && KeyWord(Ts, "{");
            while (vB && (Function.Parse(T) || TypeAssign.Parse(T) || Constructor.Parse(T)))
            {
            }
            return T.Set(vB && KeyWord(Ts, "}"));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var @class = scop.GetClass(tree[0].Content, SearcheMode.Flaten);
            load.ByteCodeMapper.OpenScop(@class);
            foreach (Tree t in tree)
                if (t.Kind == Kind.Function || t.Kind == Kind.Constructor)
                    if (t.GeneratedBy != null) 
                        t.GeneratedBy.Compile(load, @class, t);
            load.ByteCodeMapper.CloseScop();
            return null;
        }
    }
}