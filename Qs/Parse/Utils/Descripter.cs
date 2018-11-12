using Qs.Help;
using Qs.System;

namespace Qs.Parse.Utils
{
    public class Descripter
    {
        public static readonly BiDictionary<string, string> CPUFunction = new BiDictionary<string, string>
        {
            {"<=", Const.leq},
            {">=", Const.gte},
            {"++", Const.inc},
            {"!", Const.not},
            {"<<", Const.shl},
            {">>", Const.shr},
            {"+", Const.add},
            {"-", Const.sub},
            {"*", Const.mul},
            {"/", Const.div},
            {"%", Const.rst},
            {"\\", Const.idiv},
            {"^", Const.pow},
            {"&", Const.and},
            {"==", Const.eq},
            {"#", Const.neq},
            {"<", Const.inf},
            {">", Const.sup},
            {"|", Const.or},
        };
        public static readonly BiDictionary<int, BiDictionary<string, string>> Operators;

        static Descripter()
        {
            Operators = new BiDictionary<int, BiDictionary<string, string>> {
                {0, new BiDictionary <string, string> {CPUFunction[0 + 6], CPUFunction[1 + 6]}},
                {1, new BiDictionary <string, string> {CPUFunction[2 + 6], CPUFunction[3 + 6], CPUFunction[4 + 6], CPUFunction[5 + 6]}},
                {2, new BiDictionary <string, string> {CPUFunction[6 + 6]}},
                {3, new BiDictionary <string, string> {CPUFunction[7 + 6], CPUFunction[8 + 6], CPUFunction[9 + 6], CPUFunction[10 + 6], CPUFunction[11 + 6], CPUFunction[12 + 6]}}
            };
        }
    }
}
