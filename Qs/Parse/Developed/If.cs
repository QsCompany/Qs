using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;

namespace Qs.Parse.Developed
{
    public class If : ExtendParse
    {

        public If(BasicParse basicParse)
            : base(EPNames.If, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            Pile.Save(Kind.If);
            var T = new Tree(BasicParse.Pile, parent, Kind.If){GeneratedBy = this};
            var instruction = base[EPNames.Instruction];
            var vB = KeyWord(Ts, "if") && KeyWord(Ts, "(") && base[EPNames.Expression].Parse(T) && KeyWord(Ts, ")");
            if (vB) vB = instruction.Parse(T);
            if (vB && KeyWord(Ts, "else")) vB = instruction.Parse(T);
            return T.Set(vB);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var hasElse = tree.Count == 3;
            AsmLabel debElse = load.Optimum.SetLabel(null, false),
                finElse = load.Optimum.SetLabel(null, false);

            var condition = load.Compile(scop, tree[0]);
            if (condition == null || condition.Return != Assembly.Bool)
                load.LogIn(scop, tree, condition, "Condition Value Must be Of Type System.Bool");

            load.Add("test", condition, FieldInfo.Immediate(0));
            load.Optimum.SetGoto("jne", debElse);
            if (tree[1].GeneratedBy != null)
            {
                tree[1].GeneratedBy.Compile(load, scop, tree[1]);
                load.Optimum.SetLabel(debElse);
                if (!hasElse) return null;
                load.Optimum.SetGoto("jmp", finElse);
                tree[2].GeneratedBy.Compile(load, scop, tree[2]);
                load.Optimum.SetLabel(finElse);
            }
            else load.Optimum.SetLabel(debElse);
            return null;
        }
    }
}