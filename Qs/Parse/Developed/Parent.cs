using Qs.Enumerators;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Parent : ExtendParse
    {

        public Parent(BasicParse basicParse)
            : base(EPNames.Parent, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Parent);

            var savePos = BasicParse.GetKeyWord(BasicParse.Temp, "(") &&
                          (base[EPNames.Expression][parent] && BasicParse.GetKeyWord(BasicParse.Temp, ")"));
            return
                BasicParse.Pile.Leave(savePos);
        }
    }
}