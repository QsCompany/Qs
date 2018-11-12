using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    //[Obsolete("Qs.Parse.Developed.When is no longer supported. Please use the System.Threading.CompressedStack class",true)]
    public class When : ExtendParse
    {

        public When(BasicParse basicParse)
            : base(EPNames.When, basicParse)
        {
            
        }

        public override bool Parse(Tree parent)
        {
            Pile.Save(Kind.When);
            var T = new Tree(BasicParse.Pile, parent, Kind.When){GeneratedBy = this};
            var instruction = base[EPNames.Instruction];
            var complexHeritachy = base[EPNames.ComplexHeritachy];

            return
                T.Set(KeyWord(Ts, "when") && KeyWord(Ts, "(") && KeyWord(Ts, "valueof") &&
                      complexHeritachy.Parse(T) &&
                      WhenSuffix(T) && instruction.Parse(parent));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }

        private bool WhenSuffix(Tree parent)
        {
            var vB = KeyWord(parent, "changed") || KeyWord(parent, "accessed") || KeyWord(parent, "setted") ||
                     KeyWord(parent, "getted");
            return vB;
        }
    }
}