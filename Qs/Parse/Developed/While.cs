using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;

namespace Qs.Parse.Developed
{
    public class While : ExtendParse
    {

        public While(BasicParse basicParse)
            : base(EPNames.While, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            var expression = base[EPNames.Expression];

            Pile.Save(Kind.While);
            var T = new Tree(Pile, parent, Kind.While){GeneratedBy = this};
            var instruction = (Instruction) base[EPNames.Instruction];
            return
                T.Set(KeyWord(Ts, "while") && KeyWord(Ts, "(") && expression.Parse(T) && KeyWord(Ts, ")") &&
                      instruction.Parse(T));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            AsmLabel
                label1 = load.Optimum.SetLabel(null, true),
                label2 = load.Optimum.SetLabel(null, false);

            var condition = load.Compile(scop, tree[0]);
            if (condition == null || condition.Return != Assembly.Bool)
                load.LogIn(scop, tree, condition, "Condition Value Must be Of Type System.Bool");
            load.Add("test", condition, FieldInfo.Immediate(0));
            load.Optimum.SetGoto("jne", label2);
            var bloc = tree[1];
            if (tree.Count == 2 && bloc.GeneratedBy != null) bloc.GeneratedBy.Compile(load, scop, bloc);
            load.Optimum.SetGoto("jmp", label1);
            load.Optimum.SetLabel(label2);
            return RegInfo.eax;
        }
    }
}