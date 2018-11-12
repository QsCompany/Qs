using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public class CPUByte : CPUType
    {
        internal CPUByte()
            : base(Assembly.Byte, new CPUShort())
        {
            
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r,
            LoadClasses load, Scop scop)
        {
            var o = (string) null;
            switch (method.Name)
            {
                case "+":
                    o = "add";
                    break;
                case "-":
                    o = "sub";
                    break;
                case "*":
                    o = "imul";
                    break;
                case "/":
                    o = "idiv";
                    break;
                case "&":
                    o = "and";
                    break;
                case "^":
                    o = "xor";
                    break;
                case "|":
                    o = "or";
                    break;
                case ">>":
                    o = "shr";
                    break;
                case "<<":
                    o = "shl";
                    break;
                case "++":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("inc", RegInfo.eax);
                    load.Add("mov", l, RegInfo.eax);
                    return ret;
                case "--":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("dec", RegInfo.eax);
                    load.Add("mov", l, RegInfo.eax);
                    return ret;
                case "==":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("cmp", RegInfo.eax, r);
                    load.Add("sete", RegInfo.eax);
                    break;
                case ">=":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("cmp", RegInfo.eax, r);
                    load.Add("setge", RegInfo.eax);
                    break;
                case "<=":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("cmp", RegInfo.eax, r);
                    load.Add("setle", RegInfo.eax);
                    break;
                case ">":
                    load.Add("cmp", l, r);
                    load.Add("setg", RegInfo.eax);
                    break;
                case "<":
                    load.Add("cmp", l, r);
                    load.Add("setl", RegInfo.eax);
                    break;
                case "!=":
                    load.Add("cmp", l, r);
                    load.Add("setne", RegInfo.eax);
                    break;
            }
            if (o != null) load.Add(o, l, r);
            load.Add("mov", ret, RegInfo.eax);
            return ret;
        }
    }
}