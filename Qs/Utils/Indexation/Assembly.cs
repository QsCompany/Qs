using System.Collections.Generic;
using Qs.Structures;
using Class = Qs.Structures.Class;

namespace Qs.Utils.Indexation
{
    class CPUMethodInfo:MethodInfo
    {
    }
    public class Assembly
    {
        private static readonly bool Initialized;
        public static readonly List<Assembly> AllAssembly = new List<Assembly>();
        public static readonly List<CurrentScop> Assemblies = new List<CurrentScop>(4);
        public static readonly Class Object;
        public static readonly Class Void;
        public static readonly Class[] BaseClasses;
        public static readonly Class[] CPUClasses;
        static Assembly()
        {
            if (Initialized) return;
            Initialized = true;
            var ns = (Namespace) CurrentScop.Initialize("System").Current;
            BaseClasses = new[]
            {
                Object = Class.LoadAsClass(ns, null, "object", 4),
                Void = Class.LoadAsClass(ns, Object, "void", 0),

                Bool = Class.LoadAsStruct(ns, "bool", 1, true),
                Char = Class.LoadAsStruct(ns, "char", 2),
                Byte = Class.LoadAsStruct(ns, "byte", 1, true),

                Short = Class.LoadAsStruct(ns, "short", 2, true),
                UShort = Class.LoadAsStruct(ns, "short", 2, true),

                Int = Class.LoadAsStruct(ns, "int", 4, true),
                UInt = Class.LoadAsStruct(ns, "uint", 4, true),

                Long = Class.LoadAsStruct(ns, "long", 8, true),
                ULong = Class.LoadAsStruct(ns, "ulong", 8, true),

                Float = Class.LoadAsStruct(ns, "float", 4, true),
                Double = Class.LoadAsStruct(ns, "double", 8, true),

                Class.LoadAsStruct(ns, "complex", 16),
                String = Class.LoadAsClass(ns, Object, "string", 4),
                Class.LoadAsClass(ns, Object, "array", 4)
            };
            CPUClasses = new[] {Void, Bool, Char, Byte, Short, Int, Long, UShort, UInt, ULong, Float, Double};
            _shareProTypes(ns);
        }
        public static readonly Class Byte;

        public static readonly Class Short;

        public static readonly Class Char;

        public static readonly Class Int;
        public static readonly Class Bool;
        public static readonly Class Float;
        public static readonly Class Long;
        public static readonly Class Decimal;
        public static readonly Class Double;
        public static readonly Class String;
        public static readonly Class UInt;
        public static readonly Class ULong;
        public static readonly Class UShort;

        /// <summary>
        /// byte byte
        /// short byte
        /// short short
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        private static void _shareProTypes(Namespace assembly)
        {
            string[] opers = {"+", "-", "*", "/"};
            var basicTypes = new[] {Bool, Byte, Int, UInt, Short, Long, Double, Float};
            var extendTypes = new[] {String, Char, Bool};
            for (var oper = 0; oper < 4; oper++)
                for (var left = 0; left < basicTypes.Length; left++)
                    for (var right = 0; right <= left; right++)
                        DeclareMethod<CPUMethodInfo>(opers[oper], basicTypes[left], basicTypes[right], basicTypes[left]);
            for (var oper = 0; oper < 2; oper++)
                foreach (var t in extendTypes)
                    DeclareMethod<MethodInfo>(opers[oper], t, t, t);
            opers = new[] {">", "<", "==", "!=", ">=", "<="};
            for (var oper = 0; oper < 6; oper++)
                for (var left = 4; left < basicTypes.Length; left++)
                    for (var right = 4; right <= left; right++)
                        DeclareMethod<CPUMethodInfo>(opers[oper], basicTypes[left], basicTypes[right], Bool);
            foreach (var t1 in opers)
                foreach (var t in extendTypes)
                    DeclareMethod<MethodInfo>(t1, t, t, Bool);
        }

        private static void DeclareMethod<T>(string opers, Class leftOperand, Class rightOperand,Class @retrn) where T:MethodInfo, new()
        {
            var m = new T {Name = opers, Return = Bool, IsBasic = true};
            m.AddParam(new FieldInfo("a", leftOperand,true));
            m.AddParam(new FieldInfo("b", rightOperand, true));
            leftOperand.Scops.Add(m);

            if (leftOperand != rightOperand)
            {
                var n = new T { Name = opers, Return = @retrn, IsBasic = true };
                n.AddParam(new FieldInfo("b", rightOperand, true));
                n.AddParam(new FieldInfo("a", leftOperand, true));
                leftOperand.Scops.Add(n);
            }
        }
    }
}
