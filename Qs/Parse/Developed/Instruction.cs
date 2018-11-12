using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Instruction : ExtendParse
    {

        public Instruction(BasicParse basicParse)
            : base(EPNames.Instruction, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Instruction);
            if (base[EPNames.Boucle].Parse(parent)) return BasicParse.Pile.Leave(true);
            var eqAssign = base[EPNames.EqAssign].SetProperty("endWithComa", false);
            var typeAssign = base[EPNames.TypeAssign].SetProperty("endWithComa", false);
            var vB = base[EPNames.MethodCaller].Parse(parent) || base[EPNames.Goto].Parse(parent) ||
                     base[EPNames.Return].Parse(parent) || (eqAssign.Parse(parent) || typeAssign.Parse(parent)) ||
                     base[EPNames.Expression].Parse(parent) || base[EPNames.When].Parse(parent) ||
                     base[EPNames.Label].Parse(parent);
            return BasicParse.Pile.Leave(!vB ? base[EPNames.Bloc].Parse(parent) : KeyWord(Ts, ";"));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }
    }
}