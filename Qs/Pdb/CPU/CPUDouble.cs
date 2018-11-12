using System;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public class CPUDouble : CPUType
    {
        public CPUDouble() : base(Assembly.Double,null)
        {
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, Scop scop)
        {
            return ret;
            throw new NotImplementedException();
        }
    }
}