using System;
using Qs.Enumerators;
using Qs.Utils.Syntax;
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedParameter.Local
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb
{
    public class CallManager
    {

        private readonly LoadClasses load;

        public CallManager(LoadClasses load)
        {

            this.load = load;
        }


        private int GetIndex(Class c)
        {
            for (int i = 0; i < Assembly.CPUClasses.Length; i++)
                if (ReferenceEquals(c, Assembly.CPUClasses[i])) return i;
            return -1;
        }

        public FieldInfo Call(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r,FieldInfo ret)
        {
            switch (method.Name)
            {
                case "+":
                    return add(scop, method, l, r, ret);
                case "-":
                    return sub(scop, method, l, r, ret);
                case "==":
                    return eq(scop, method, l, r,ret);
         
                case "*":
                    return mul(scop, method, l, r,ret);

                case "/":
                    return div(scop, method, l, r,ret);

                case ">=":
                    return ge(scop, method, l,r,ret);

                case "<=":
                    return le(scop, method, l,r,ret);

                case ">":
                    return gth(scop, method, l,r,ret);

                case "<":
                    return lth(scop, method, l,r,ret);
                case "!=":
                    return ne(scop, method, l,r,ret);

                case "&":
                    return and(scop, method, l,r,ret);

                case "^":
                    return xor(scop, method, l,r,ret);
                case "|":
                    return or(scop, method, l,r,ret);

                case "++":
                    return inc(scop, method, l,r,ret);

                case "--":
                    return dec(scop, method, l,r,ret);

                case ">>":
                    return rsh(scop, method, l,r,ret);

                case "<<":
                    return lsh(scop, method, l, r,ret);
            }
            return RegInfo.eax;
        }


        private FieldInfo eq(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private bool isSigned_4(FieldInfo f)
        {
            return f.Return == Assembly.Int || f.Return == Assembly.Short || f.Return == Assembly.Byte ||
                   f.Return == Assembly.Char;
        }

        private FieldInfo add(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            if (ret == null) throw new Exception("return value cannot be null");
            var il = GetIndex(l.Return);
            var ir = GetIndex(r.Return);
            if (il < ir) swap(ref r, ref l);
            if (l.Return.SizeOf() <= 4 && isSigned_4(r))
            {
                load.Add("add", l, r);
                load.Add("mov", ret, RegInfo.eax);
                return ret;
            }
            if (l.Return == Assembly.Long && (isSigned_4(r) || r.Return == Assembly.Long))
            {
                load.Add("mov", Reg.eax, l.Handle.ToDwordPtr);
                load.Add("mov", Reg.edx, l.Handle.ToDwordPtr + 4);
                load.Add("add", Reg.eax, r.Handle.ToDwordPtr);
                load.Add("adc", Reg.edx, r.Return == Assembly.Long ? r.Handle.ToDwordPtr + 4 : new Operand(0));
                load.Add("mov", ret.Handle.ToDwordPtr, Reg.eax);
                load.Add("mov", ret.Handle.ToDwordPtr + 4, Reg.edx);
                return ret;
            }
            if (l.Return == r.Return)
                if (r.Return == Assembly.Float || l.Return == Assembly.Double)
                {
                    load.Add("fld", l);
                    load.Add("fadd", l);
                    load.Add("fstp", ret);
                    return ret;
                }
            //throw new Exception("Error CPUType");
            return ret;
        }

        internal static void swap(ref FieldInfo l, ref FieldInfo r)
        {
            var c = l;
            l = r;
            r = c;
        }

        private FieldInfo sub(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo mul(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo div(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo ge(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo le(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo gth(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo lth(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo ne(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo and(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo xor(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo or(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo inc(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo dec(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo rsh(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }

        private FieldInfo lsh(Scop scop, MethodInfo method, FieldInfo l, FieldInfo r, FieldInfo ret)
        {
            throw new NotImplementedException();
        }
    }

    
}