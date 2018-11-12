using Qs.Enumerators;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Bloc : ExtendParse
    {
        public Bloc(BasicParse basicParse)
            : base(EPNames.Bloc, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Bloc);
            var T = new Tree(BasicParse.Pile, parent, Kind.Bloc){GeneratedBy = this};
            if (!BasicParse.GetKeyWord(BasicParse.Temp, "{")) return BasicParse.Pile.Leave(false);
            var instruction = base[EPNames.Instruction];
            while (instruction.Parse(T)) ;
            return T.Set(BasicParse.GetKeyWord(BasicParse.Temp, "}"));
        }
    }
}