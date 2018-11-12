using System.Collections.Generic;
using Qs.Enumerators;

namespace Qs.Utils.Base
{
    public static class CUal
    {

        public static Dictionary<string, CPUFunction> strFunction = new Dictionary<string, CPUFunction>();

        public static readonly List<CPUFunction> Functions = new List<CPUFunction>();

        public static void Add(string name, Script paramScript)
        {
            var function = new CPUFunction { Name = name, Script = paramScript, Index = (byte)Functions.Count };
            Functions.Add(function);
            strFunction.Add(function.Name, function);
        }

        public static CPUFunction GetFunction(byte hachCode)
        {
            return Functions[hachCode];
        }
        public static CPUFunction GetFunction(string fnName)
        {
            CPUFunction r;
            strFunction.TryGetValue(fnName, out r);
            return r;
        }

        static CUal()
        {
            foreach (var s in new[] { "halt", "ret", "leave", "nop","rop", "wait" }) Add(s, Script.ZeroParam);
            var t = new[] {
                                                  "mov", "lea"
                                                  , "add", "sub", "mul", "div", "abs","adc","sbb"
                                                  , "iadd", "isub", "imul", "idiv"
                                                  , "dadd", "dsub", "dmul", "ddiv", "dabs", "dcmp"
                                                  , "and", "or", "xand", "xor", "nand", "nor"
                                                  , "eq", "neq", "cmp", "test"
                                                  , "shl", "shr", "xchg","sar"
                                                  , "in", "out", "rep"
                                                  ,"fild","ifld","fdivp","fidiv"
                          };
            foreach (var s in t) Add(s, Script.TwoParam);
            t = new[]
            {
                "call", "dec", "inc", "int", "lock",
                "pop", "push", "set",
                "neg", "not",
                "loop", "loopd", "loopz", "loope", "loopne", "loopnz",
                "jmp", "jcxz"
                , "jg"
                , "jge"
                , "jl"
                , "jle"
                , "je"
                , "jne"
                , "fadd", "fsub", "fmul", "fdiv", "fabs", "fcmp","fld","fstp"

            };
            foreach (var s in t) Add(s, Script.OneParam);
        }

        internal static readonly string Jumps = string.Join("|",
            new[] {"jmp", "jcxz", "jg", "jge", "jl", "jle", "je", "jne"});
    }
}
