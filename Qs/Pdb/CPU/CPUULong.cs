
using System;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.Pdb.CPU
{
    public class CPUULong : CPUType
    {
        public CPUULong()
            : base(Assembly.ULong, new CPUFloat())
        {
            ulong ul_w = 0;
            ulong ul_v = 0;
            long l_b = 0;
            uint u_c = 0;
            int i_d = 0;
            ushort us_e = 0;
            short s_f = 0;
            byte b_g = 0;
            
            /****************ulong***********************/
            var wv = ul_w+ul_v;
            var ll = l_b+l_b;
            /****************long***********************/
            var ac = +(ulong)l_b + ul_w;
            var ac_ = +l_b + l_b;
            /***************uint************************/
            var ad =  u_c + ul_w;
            var ac__ =  u_c + l_b;
            /***************int*************************/
            var ae = (uint)i_d + ul_w;
            var __ac =  (uint)i_d + l_b;
            /***************ushort**********************/
            var af =  us_e + ul_w;
            var _ac_ =  us_e + l_b;
            /***************short***********************/
            var af_ =  (uint)s_f + ul_w;
            var _ac__ =  (uint)s_f + l_b;
            /***************byte***********************/
            var ag =  b_g + ul_w;
            var _ag_ = b_g + l_b;
            /*******************************************/
            
            /*******************************************/
            (ac + ad +ag+(decimal) _ag_+ ae + af + __ac + ac_ + ac__ + _ac_ + wv  +af_+_ac_).GetHashCode();
        }

        protected override FieldInfo BeginCompile(MethodInfo method, FieldInfo ret, FieldInfo l, FieldInfo r, LoadClasses load, Scop scop)
        {
            throw new NotImplementedException();
        }
    }
}