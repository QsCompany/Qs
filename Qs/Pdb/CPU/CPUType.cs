using System.Collections.Generic;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public abstract class CPUType
    {
        static CPUType()
        {
            var cpu = new CPUByte();
        }
        protected static readonly Dictionary<Class, CPUType> instants = new Dictionary<Class, CPUType>();
        private readonly Class Type;

        protected CPUType(Class type, CPUType nextHighType)
        {
            CPUType y;
            if (nextHighType != null && !instants.TryGetValue(nextHighType.Type, out y))
                instants.Add(nextHighType.Type, nextHighType);

            if (instants.TryGetValue(Type = type, out y)) return;
            instants.Add(type, this);
        }

        public int Index
        {
            get
            {
                var i = 0;
                foreach (var instant in instants)
                    if (instant.Value == this) return i;
                    else
                        i++;
                return -1;
            }
        }
        protected abstract FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, Scop scop);

        public static FieldInfo Call(LoadClasses load,Scop scop,FieldInfo ret,MethodInfo method, FieldInfo l, FieldInfo r)
        {
            var cpuTypeL = instants[l.Return];
            var cpuTypeR = instants[r.Return];
            return (cpuTypeL.Index < cpuTypeR.Index ? cpuTypeL : cpuTypeR).BeginCompile(method, ret, l, r, load, scop);
        }

        protected static bool IsSigned(Class @return)
        {
            return @return == Assembly.Int || @return == Assembly.Int || @return == Assembly.Long;
        }
    }
}