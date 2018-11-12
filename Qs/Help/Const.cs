using Qs.Enumerators;

namespace Qs.Help
{
    public static class Const
    {
        public const string not = "not";
        public const string shl = "shl";
        public const string shr = "shr";
        public const string leq = "leq";
        public const string gte = "gte";
        public const string inc = "inc";
        public const string neq = "neq";
        public const string eq = "eq";
        public const string pow = "pow";
        public const string idiv = "idiv";
        public const string rst = "rst";
        public const string getArray = "Get_Array";
        public const string proc = "proc";
        public const string pop = "pop";
        public const string nop = "nop";
        public const string IP = "ip";
        public const string @return = "return";
        public const string rop = "rop";
        public const string ret = "ret";
        public const string endProc = "end proc";
        public const string empty = "";
        public const string mul = "mul";
        public const string div = "div";
        public const string add = "add";
        public const string sub = "sub";
        public const string or = "or";
        public const string and = "and";
        public const string xor = "xor";
        public const string inf = "inf";
        public const string sup = "sup";
        public const string jmp = "jmp";
        public const string ne = "not";
        public const string push = "push";
        public const string mem = "mem";
        public const string call = "call";
        public const string upl = "upl";
        public const string usu = "neg";
        public const string mov = "mov";
        public const string @class = "class";
        public const string space = "space";
        public const string endClass = "end class";
        public const string endSpace = "end space";
        public const string label = "label";
        public const string eax = "eax";
        public const string jne = "jne";
        public const string je = "je";
        public const string Jumps = "jmp|jcxz|jg|jge|jl|jle|je|jne";
        internal static readonly string[] JumpList = {"jmp", "jcxz", "jg", "jge", "jl", "jle", "je", "jne"};
        public const Kind ConstKind = Kind.Variable | Kind.String | Kind.Numbre | Kind.Const;

        public static readonly byte[] JMPS = {130};

        public const string returnLabel = "@return";
        public const string @new = "new";
    }
}
