// ReSharper disable ConditionIsAlwaysTrueOrFalse
using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;

namespace Qs.Parse.Developed
{
    public class For : ExtendParse
    {

        public For(BasicParse basicParse)
            : base(EPNames.For, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            Pile.Save(Kind.For);
            var instruction = base[EPNames.Instruction];
            var expression = base[EPNames.Expression];
            var T = new Tree(BasicParse.Pile, parent, Kind.For){GeneratedBy = this};
            return

                T.Set(KeyWord(Ts, "for") && KeyWord(Ts, "(") && (instruction.Parse(T) | true) && KeyWord(Ts, ";") &&
                      expression.Parse(T) && KeyWord(Ts, ";") && (instruction.Parse(T) | true) && KeyWord(Ts, ")") &&
                      instruction.Parse(T));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            tree[0].GeneratedBy.Compile(load, scop, tree[0]);
            AsmLabel
                label1 = load.Optimum.SetLabel(null, true),
                label2 = load.Optimum.SetLabel(null, false);

            var condition = tree[1].GeneratedBy.Compile(load, scop, tree[1]);
            if (condition == null || condition.Return != Assembly.Bool)
                load.LogIn(scop, tree, condition, "Condition Value Must be Of Type System.Bool");
            load.Add("test", condition, FieldInfo.Immediate(0));
            load.Optimum.SetGoto("jne", label2);
            tree[3].GeneratedBy.Compile(load, scop, tree[3]);
            tree[2].GeneratedBy.Compile(load, scop, tree[2]);
            load.Optimum.SetGoto("jmp", label1);
            load.Optimum.SetLabel(label2);
            return null;
        }
    }
}