using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;

namespace Qs.Parse.Developed
{
    public class Do : ExtendParse
    {

        public Do(BasicParse basicParse)
            : base(EPNames.Do, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            var instruction = base[EPNames.Instruction];
            var expression = base[EPNames.Expression];
            Pile.Save(Kind.Do);
            var T = new Tree(Pile, parent, Kind.Do){GeneratedBy = this};
            return
                T.Set(KeyWord(Ts, "do") && instruction.Parse(T) && KeyWord(Ts, "while") && KeyWord(Ts, "(") &&
                      expression.Parse(T) && KeyWord(Ts, ")"));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            var
                label1 = load.Optimum.SetLabel(null, true);

            load.Compile(scop, tree[0]);
            var condition = tree[1].GeneratedBy.Compile(load, scop, tree[1]);
            if (condition == null || condition.Return != Assembly.Bool)
                load.LogIn(scop, tree, tree[1], "Condition Value Must be Of Type System.Bool");
            load.Add("test", condition, FieldInfo.Immediate(0));
            load.Optimum.SetGoto("jne", label1);
            return RegInfo.eax;
        }
    }
}