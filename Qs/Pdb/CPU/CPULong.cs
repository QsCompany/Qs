using Qs.Enumerators;
using Qs.Parse.Developed;
using Qs.Structures;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;

namespace Qs.Pdb.CPU
{
    public class CPULong : CPUType
    {
        public CPULong()
            : base(Assembly.Long, new CPUULong())
        {
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r,
            LoadClasses load, Scop scop)
        {
            switch (method.Name)
            {
                case "+":
                    return add(ret, l, r, load);
                case "-":
                    return sub(ret, l, r, load);

                case "*":
                case "/":
                    return mul(method, ret, l, r, load);
                case "&":
                    return and(ret, l, r, load);
                case "^":
                    return xor(ret, l, r, load);

                case "|":
                    return or(ret, l, r, load);

                case ">>":
                    return shr(method, ret, l, r, load);

                case "<<":
                    return shl(method, ret, l, r, load);
                case "++":
                    return inc(l, load);
                case "--":
                    return dec(l, load);
                case "==":
                    return eq_ne(ret, l, r, load, true);
                case "!=":
                    return eq_ne(ret, l, r, load);
                case ">=":
                    return lte_gte(ret, l, r, load, false);
                case "<=":
                    return lte_gte(ret, l, r, load, true);
                case ">":
                    return lt_gt(ret, l, r, load, false);
                case "<":
                    return lt_gt(ret, l, r, load, true);

            }
            return ret;
        }

        private FieldInfo eq_ne(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load,
            bool iseq = false)
        {
            var r_islong = r.Return == Assembly.Long;
            AsmLabel trueLabel;
            AsmLabel returnLabel;
            var falseLabel = PushParams(l, r, load, out trueLabel, out returnLabel);

            load.Add("cmp", Reg.edx, r_islong ? r.GetHandle(4) : Reg.ebx);

            load.Optimum.SetGoto("jne", iseq ? falseLabel : trueLabel, load, new Operand(0xFFFF, DataType.Byte));
            load.Add("cmp", RegInfo.eax, r_islong ? r : RegInfo.ecx);

            load.Optimum.SetGoto(iseq ? "je" : "jne", trueLabel, load, new Operand(0xFFFF, DataType.Byte));
            load.Optimum.SetLabel(falseLabel);
            load.Add("xor", RegInfo.eax, RegInfo.eax);
            load.Optimum.SetGoto("jmp", returnLabel, load, new Operand(0xFFFF, DataType.Byte));
            load.Optimum.SetLabel(trueLabel);
            load.Add("mov", Reg.eax, new Operand(1, DataType.Byte));
            load.Optimum.SetLabel(returnLabel);
            load.Add("mov", ret, RegInfo.eax);
            return ret;
        }

        private static FieldInfo lt_gt(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, bool is_lt_gt)
        {
            string jm1, jm2, jm3;
            if (is_lt_gt)
            {
                jm1 = "jg";
                jm2 = "jl";
                jm3 = "jb";
            }
            else
            {
                jm1 = "jl";
                jm2 = "jg";
                jm3 = "ja";
            }
            AsmLabel falseLabel = new AsmLabel("{asm1}"),
                trueLabel = new AsmLabel("{asm3}"),
                returnLabel = new AsmLabel("{ret}" + CPUUInt.u);

            var r_islong = r.Return == Assembly.Long;
            pushParam(load, l.Handle, true, IsSigned(l.Return), l.Return == Assembly.Long);
            if (!r_islong)
                pushParam(load, r.Handle, false, IsSigned(r.Return), false);

            load.Add("cmp", Reg.edx, !r_islong ? Reg.ebx : r.GetHandle(4));
            load.Optimum.SetGoto(jm1, trueLabel, load, cByte.Clone());
            load.Optimum.SetGoto(jm2, falseLabel, load, cByte.Clone());
            load.Add("cmp", RegInfo.eax, !r_islong ? RegInfo.ecx : r);
            load.Optimum.SetGoto(jm3, trueLabel, load, cByte.Clone());
            load.Optimum.SetLabel(falseLabel);
            load.Add("xor", RegInfo.eax, RegInfo.eax);
            load.Optimum.SetGoto("jmp", returnLabel, load, new Operand(0xFF, DataType.Byte));
            load.Optimum.SetLabel(trueLabel);
            load.Add("mov", Reg.eax, new Operand(1, DataType.Byte));
            load.Optimum.SetLabel(returnLabel);
            load.Add("mov", ret, RegInfo.eax);
            return ret;
        }


        private FieldInfo lte_gte(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, bool is_lte_gte)
        {
            string jm1;
            string jm2;
            string jm3;
            if (is_lte_gte)
            {
                jm1 = "jl";
                jm2 = "jg";
                jm3 = "ja";
            }
            else
            {
                jm1 = "jg";
                jm2 = "jl";
                jm3 = "jb";
            }
            var r_islong = r.Return == Assembly.Long;
            AsmLabel trueLabel;
            AsmLabel returnLabel;
            var falseLabel = PushParams(l, r, load, out trueLabel, out returnLabel);

            load.Add("cmp", Reg.edx, r_islong ? r.Handle + 4 : Reg.ebx);
            load.Optimum.SetGoto(jm1, trueLabel, load, cByte.Clone());
            load.Optimum.SetGoto(jm2, falseLabel, load, cByte.Clone());
            load.Add("cmp", RegInfo.eax, r_islong ? r : RegInfo.ecx);

            load.Optimum.SetGoto(jm3, trueLabel, load, cByte.Clone());
            load.Optimum.SetLabel(falseLabel);
            load.Add("xor", RegInfo.eax, RegInfo.eax);
            load.Optimum.SetGoto("jmp", returnLabel, load, new Operand(0xFF, DataType.Byte));
            load.Optimum.SetLabel(trueLabel);
            load.Add("mov", Reg.eax, new Operand(1, DataType.Byte));
            load.Optimum.SetLabel(returnLabel);

            load.Add("test", RegInfo.eax, RegInfo.eax);
            load.Add("sete", RegInfo.eax);
            load.Add("mov", ret, RegInfo.eax);
            return ret;
        }

        private AsmLabel PushParams(FieldInfo l, FieldInfo r, LoadClasses load, out AsmLabel trueLabel,
            out AsmLabel returnLabel)
        {
            trueLabel = new AsmLabel("{asm3}");
            returnLabel = new AsmLabel("{ret}" + CPUUInt.u);
            pushParam(load, l.Handle, true, IsSigned(l.Return), l.Return == Assembly.Long);

            if (r.Return != Assembly.Long)
                pushParam(load, r.Handle, false, IsSigned(r.Return), false);
            return new AsmLabel("{asm1}");
        }

        internal static FieldInfo dec(FieldInfo l, LoadClasses load)
        {
            return add(l, l, new ConstInfo(Assembly.Byte, new byte[] {1}), load);
        }

        internal static FieldInfo inc(FieldInfo l, LoadClasses load)
        {
            return sub(l, l, new ConstInfo(Assembly.Byte, new byte[] {1}), load);
        }

        internal static FieldInfo shl(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            var ol = l.Handle;
            var or = r.Handle;
            load.Add("mov", Reg.eax, ol);
            load.Add("mov", Reg.edx, ol + 4);
            load.Add("mov", Reg.ecx, or);
            load.Add("mov", Reg.ebx, new ConstInfo(Assembly.Byte, new[] {(byte) 63}).Handle);
            load.Add("and", Reg.ebx, Reg.ecx);
            load.Optimum.Call(method);
            return @return(load, ret);
        }

        internal static FieldInfo shr(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            var ol = l.Handle;
            var or = r.Handle;
            load.Add("mov", Reg.eax, ol);
            load.Add("mov", Reg.edx, ol + 4);
            load.Add("mov", Reg.ecx, or);
            load.Add("mov", Reg.ebx, new ConstInfo(Assembly.Byte, new[] {(byte) 63}).Handle);
            load.Add("and", Reg.ebx, Reg.ecx);
            load.Optimum.Call(method);
            return @return(load, ret);
        }


        internal static FieldInfo or(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            if (instants[l.Return].Index > instants[r.Return].Index)
                CallManager.swap(ref l, ref r);
            var ol = l.Handle;
            var or = r.Handle;
            pushParam(load, ol, true, false);
            if (r.Return != Assembly.Long)
                pushParam(load, or, false, r.Return == Assembly.Int || r.Return == Assembly.Short);
            call(load, "or", "or", r.Return == Assembly.Long ? or : null);
            return @return(load, ret);
        }

        internal static FieldInfo xor(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            if (instants[l.Return].Index > instants[r.Return].Index)
                CallManager.swap(ref l, ref r);
            var ol = l.Handle;
            var or = r.Handle;
            pushParam(load, ol, true, false);
            if (r.Return != Assembly.Long)
                pushParam(load, or, false, r.Return == Assembly.Int || r.Return == Assembly.Short);
            call(load, "xor", "xor", r.Return == Assembly.Long ? or : null);
            return @return(load, ret);
        }

        internal static FieldInfo and(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            if (instants[l.Return].Index > instants[r.Return].Index)
                CallManager.swap(ref l, ref r);
            var ol = l.Handle;
            var or = r.Handle;
            pushParam(load, ol, true, false);
            if (r.Return != Assembly.Long)
                pushParam(load, or, false, r.Return == Assembly.Int || r.Return == Assembly.Short);
            call(load, "and", "and", r.Return == Assembly.Long ? or : null);
            return @return(load, ret);

        }

        internal void pushParam(LoadClasses load, Operand l, bool first, bool isigned, bool islong, bool iscpucall)
        {
            if (iscpucall) pushParam(load, l, first, isigned, islong);
            else
                //if (first)
                if (islong)
                {
                    load.Add("push", l + 4);
                    load.Add("push", l);
                }
                else if (isigned)
                {
                    load.Add("mov", Reg.eax, l);
                    load.Add("cdq");
                    load.Add("push", Reg.edx);
                    load.Add("push", Reg.eax);
                }
                else
                {
                    load.Add("xor", Reg.edx, Reg.edx);
                    load.Add("push", Reg.edx);
                    load.Add("push", l);
                }
        }

        private
            FieldInfo mul(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            var ol = l.Handle;
            var or = r.Handle;
            var rsigned = r.Return == Assembly.Int || r.Return == Assembly.Short;
            var lsigned = l.Return == Assembly.Int || l.Return == Assembly.Short;
            pushParam(load, ol, true, lsigned, l.Return == Assembly.Long, false);
            pushParam(load, or, true, rsigned, l.Return == Assembly.Long, false);
            load.Optimum.Call(method);
            @return(load, ret);
            return ret;
        }

        internal static FieldInfo sub(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            var ol = l.Handle;
            var or = r.Handle;
            var rsigned = r.Return == Assembly.Int || r.Return == Assembly.Short;
            var lsigned = l.Return == Assembly.Int || l.Return == Assembly.Short;

            pushParam(load, ol, true, lsigned, l.Return == Assembly.Long);
            if (instants[l.Return].Index <= instants[r.Return].Index && r.Return != Assembly.Long)
                pushParam(load, or, false, rsigned);
            call(load, "sub", "sbb", r.Return == Assembly.Long ? or : null);
            return @return(load, ret);
        }

        internal static FieldInfo add(FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load)
        {
            if (instants[l.Return].Index > instants[r.Return].Index)
                CallManager.swap(ref l, ref r);
            var ol = l.Handle;
            var or = r.Handle;
            pushParam(load, ol, true, false);
            if (r.Return != Assembly.Long)
                pushParam(load, or, false, r.Return == Assembly.Int || r.Return == Assembly.Short);
            call(load, "add", "adc", r.Return == Assembly.Long ? or : null);
            return @return(load, ret);
        }

        private static FieldInfo @return(LoadClasses load, FieldInfo ret)
        {
            var _ret = ret.Handle;
            load.Add("mov", _ret, Reg.eax);
            load.Add("mov", _ret + 4, Reg.eax);
            return ret;
        }

        internal static void call(LoadClasses load, string op, string cop, Operand r = null)
        {
            if (r == null)
            {
                load.Add(op, Reg.eax, Reg.ecx);
                load.Add(cop, Reg.edx, Reg.ebx);
            }
            else
            {
                load.Add(op, Reg.eax, r);
                load.Add(cop, Reg.edx, r + 4);
            }
        }

        internal static void pushParam(LoadClasses load, Operand l, bool first, bool signed, bool islong = true)
        {

            if (first)
            {
                load.Add("mov", Reg.eax, l);
                if (islong)
                    load.Add("mov", Reg.edx, l + 4);
                else if (signed)
                    load.Add("cdq");
                else
                    load.Add("xor", Reg.edx, Reg.edx);
            }
            else

            {

                load.Add("mov", Reg.ecx, l);
                if (signed)
                {
                    load.Add("mov", Reg.ebx, Reg.ecx);
                    load.Add("sar", RegInfo.ebx, c1F);
                }
                else
                    load.Add("xor", Reg.ebx, Reg.ebx);
            }
        }

        public static ConstInfo c1F = new ConstInfo(Assembly.Byte, new[] {(byte) 0x1F});
        public static ConstInfo c2F = new ConstInfo(Assembly.Byte, new[] {(byte) 0x2F});
        public static Operand cByte = new Operand(0xFFFF, DataType.Byte);
    }
}