using System;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public class CPUFloat : CPUType
    {
        public CPUFloat()
            : base(Assembly.Float, new CPUDouble())
        {
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, Scop scop)
        {
            return null;
            throw new NotImplementedException();
        }
    }
}