using Qs.Enumerators;
using Qs.Parse.Developed;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public class CPUUInt : CPUType
    {
        public CPUUInt()
            : base(Assembly.UInt, new CPULong())
        {
        }

        private static bool IsSigned(FieldInfo r)
        {
            return r.Return == Assembly.Int || r.Return == Assembly.Short;
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r,
            LoadClasses load, Scop scop)
        {
            var lsigned = IsSigned(l);
            var rsigned = IsSigned(r);
            var returnTrue = new AsmLabel("{lasm" + u++ + "}");
            var returnFalse = new AsmLabel("{lasm" + u++ + "}");
            var @return = new AsmLabel("{lasm" + u++ + "}");
            string o = "", oc = "";
            var cnc="";
            switch (method.Name)
            {
                    #region +-&^|
                case "opers":
                    return UintSOperation(ret, l, r, load, o, oc);
                case "+":
                    o = "add";
                    oc = "adc";
                    goto case "opers";
                case "-":
                    o = "sub";
                    oc = "sbb";
                    goto case "opers";
                case "&":
                    oc = o = "and";
                    goto case "opers";
                case "^":
                    oc = o = "xor";
                    goto case "opers";
                case "|":
                    oc = o = "or";
                    goto case "opers";
                    #endregion

                    #region */

                case "*":
                    o = "imul";
                    goto case "*/";
                case "/":
                    o = "idiv";
                    goto case "*/";
                case "*/":
                    if (lsigned == rsigned)
                    {
                        load.Add(o, l, r);
                        load.Add("mov", ret, RegInfo.eax);
                        return ret;
                    }
                    if (lsigned) CallManager.swap(ref l, ref r);
                    ConvertToLong(load, l, true);
                    load.Add("push", RegInfo.edx);
                    load.Add("push", RegInfo.eax);
                    ConvertToLong(load, r, true);
                    load.Add("push", RegInfo.edx);
                    load.Add("push", RegInfo.eax);
                    load.Optimum.Call(method);
                    Move(load, ret, Reg.eax, Reg.eax);
                    break;

                    #endregion

                    #region none

                case ">>":
                    break;
                case "<<":
                    break;
                case "++":
                case "--":
                    load.Add("mov", RegInfo.eax, l);
                    load.Add(method.Name == "++" ? "inc" : "dec", RegInfo.eax);
                    load.Add("mov", l, RegInfo.eax);
                    return ret;
                    #endregion

                #region       ==  !=
                case "==!=":
                    if (lsigned != rsigned)
                        return Compile(ret, l, r, load, o, returnFalse, oc, returnTrue, "je", @return, false, true);
                    load.Add("mov", RegInfo.eax, l);
                    load.Add("cmp", RegInfo.eax, r);
                    load.Add(cnc, RegInfo.eax);
                    load.Add("mov", ret, RegInfo.eax);
                    return ret;
                case "==":
                    cnc = "sete";
                    o = "jne";
                    goto case "==!=";
                case "!=":
                    o = "je";
                    cnc = "setne";
                    goto case "==!=";
                    #endregion
                    #region <==>
                case ">=":
                    
                    cnc = "sete";
                    if (lsigned != rsigned)
                        return Compile(ret, l, r, load, "jg", returnFalse, "jl", returnTrue, "jb", @return, true);
                    goto case "<==>";
                case "<==>":
                     load.Add("mov", RegInfo.eax, l);
                    load.Add("cmp", RegInfo.eax, r);
                    if (cnc == "sete") load.Add("sbb", RegInfo.eax, RegInfo.eax);
                    load.Add(cnc, RegInfo.eax);
                    load.Add("mov", ret, RegInfo.eax);
                    break;
                case "<=":
                    cnc = "setbe";
                    if (lsigned != rsigned)
                        return Compile(ret, l, r, load, "jl", returnFalse, "jb", returnTrue, "ja", @return, true);
                    goto case "<==>";
                    #endregion
                case ">":
                    #region >
                    if (lsigned == rsigned)
                    {
                        load.Add("mov", RegInfo.eax, l);
                        load.Add("cmp", RegInfo.eax, r);
                        load.Add("seta", RegInfo.eax);
                        load.Add("mov", ret, RegInfo.eax);
                        return ret;
                    }
                    return Compile(ret, l, r, load, "jl", returnFalse, "jg", returnTrue, "ja", @return, false);

                    #endregion

                case "<":
                    #region >
                    if (lsigned == rsigned)
                    {
                        load.Add("mov", RegInfo.eax, l);
                        load.Add("cmp", RegInfo.eax, r);
                        load.Add("sbb", RegInfo.eax, RegInfo.eax);
                        load.Add("neg", RegInfo.eax);
                        load.Add("mov", ret, RegInfo.eax);
                        return ret;
                    }
                    return Compile(ret, l, r, load, "jg", returnFalse, "jl", returnTrue, "jb", @return, false);

                    #endregion
            }
            return ret;
        }

        private static FieldInfo Compile(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, string o, AsmLabel returnFalse, string oc, AsmLabel returnTrue, string cn, AsmLabel @return, bool orEqual, bool IsEquality = false)
        {
            PushParams(load, l, r);
            load.Add("cmp", RegInfo.edx, RegInfo.ebx);
            load.Optimum.SetGoto(o, returnFalse);
            if (!IsEquality)
                load.Optimum.SetGoto(oc, returnTrue);
            FPReturn(ret, load, cn, returnTrue, returnFalse, @return, orEqual);
            return ret;
        }

        private static void FPReturn(FieldInfo ret, LoadClasses load, string cn, AsmLabel returnTrue, AsmLabel returnFalse, AsmLabel @return, bool OrEqual)
        {
            load.Add("cmp", RegInfo.eax, RegInfo.ecx);
            load.Optimum.SetGoto(cn, returnTrue);
            Return(ret, load, returnTrue, returnFalse, @return, OrEqual);
        }

        private static void Return(FieldInfo ret, LoadClasses load, AsmLabel returnTrue, AsmLabel returnFalse, AsmLabel @return, bool OrEqual)
        {
//Return True
            load.Optimum.SetLabel(returnTrue);
            load.Add("mov", RegInfo.eax, ConstInfo.True);
//Return False
            load.Optimum.SetLabel(returnFalse);
            load.Add("xor", RegInfo.eax, RegInfo.eax);
            load.Optimum.SetGoto("jmp", @return);
//Return
            load.Optimum.SetLabel(@return);
            if (OrEqual)
            {
                load.Add("test", RegInfo.eax, RegInfo.eax);
                load.Add("sete", RegInfo.eax);
            }
            load.Add("mov", ret, RegInfo.eax);
        }

        public static int u;

        private static FieldInfo UintSOperation(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, string oper,
            string operC)
        {
            var lsigned = IsSigned(l);
            var rsigned = IsSigned(r);
            if (lsigned == rsigned)
            {
                load.Add(oper, l, r);
                load.Add("mov", ret, RegInfo.eax);
                return ret;
            }
            PushParams(load, l, r);
            load.Add(oper, RegInfo.eax, RegInfo.ecx);
            load.Add(operC, RegInfo.edx, RegInfo.ebx);
            Move(load, l, Reg.eax, Reg.edx);
            return ret;
        }

        private static void ConvertToLong(LoadClasses load, FieldInfo f, bool left)
        {
            if (IsSigned(f))
                if (left)
                {
                    load.Add("mov", RegInfo.eax, f);
                    load.Add("cdq");
                }
                else
                {
                    load.Add("mov", RegInfo.ecx, f);
                    load.Add("mov", RegInfo.ebx, RegInfo.ecx);
                    load.Add("sar", RegInfo.ebx, new ConstInfo(Assembly.Byte, new[] {(byte) 0x1F}));
                }
            else if (left)
            {
                load.Add("mov", RegInfo.eax, f);
                load.Add("xor", RegInfo.edx, RegInfo.edx);
            }
            else
            {
                load.Add("mov", RegInfo.ecx, f);
                load.Add("xor", RegInfo.ebx, RegInfo.ebx);
            }
        }

        private static void PushParams(LoadClasses load, FieldInfo l, FieldInfo r)
        {
            ConvertToLong(load, l, true);
            ConvertToLong(load, r, false);
        }

        private static void Move(LoadClasses load, FieldInfo f, Reg a0, Reg a2)
        {
            load.Add("mov", f.Handle.ToDwordPtr, a0);
            load.Add("mov", f.Handle.ToDwordPtr + 4, a2);
        }
        
    }
}