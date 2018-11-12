using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Space : ExtendParse
    {

        public Space(BasicParse basicParse)
            : base(EPNames.Space, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            var Class = base[EPNames.Class];
            var Space = base[EPNames.Space];
            Pile.Save(Kind.Space);

            var T = new Tree(Pile, parent, Kind.Space){GeneratedBy = this};
            bool Mark, end;
            var vB = end = (Mark = KeyWord(Ts, "space")) && BasicParse.GetHeritachy(T) && KeyWord(Ts, "{");
            while (vB && (Class.Parse(T) || Space.Parse(T))) ;
            if (end) end = KeyWord(Ts, "}");
            return T.Set(end, Mark);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var space = scop.GetSpace(tree[0].Content, SearcheMode.Flaten);
            load.ByteCodeMapper.OpenScop(space);
            foreach (Tree t in tree)
                if (t.GeneratedBy != null) t.GeneratedBy.Compile(load, space, t);
            load.ByteCodeMapper.CloseScop();
            return null;
        }
    }
}