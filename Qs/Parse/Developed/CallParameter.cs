using Qs.Enumerators;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class CallParameter : ExtendParse
    {
        public CallParameter(BasicParse basicParse)
            : base(EPNames.CallParameter, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Param);
            var T = new Tree(BasicParse.Pile, parent, Kind.Param) {GeneratedBy = this};
            bool end;
            var vB = (end = BasicParse.GetKeyWord(BasicParse.Temp, "(")) && base[EPNames.Expression].Parse(T);
            while (vB && BasicParse.GetKeyWord(BasicParse.Temp, ",") && base[EPNames.Expression].Parse(T))
            {
            }
            return T.Set(end && BasicParse.GetKeyWord(BasicParse.Temp, ")"));
        }
    }
}