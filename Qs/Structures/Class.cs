#pragma warning disable 659
using Qs.Utils.Indexation;
#pragma warning disable 660,661
using System;
using System.Collections.Generic;

namespace Qs.Structures
{
    public sealed class Class : DataPortor
    {
        private bool Equals(Class other)
        {
            return Parent == other.Parent && string.CompareOrdinal(other.Name, Name) == 0;
        }

        public Class Base
        {
            set
            {
                if (Finalized) return;
                if (!IsClass) return;
                if (value == null) value = Assembly.Object;
                SetDataSize(DataSize - BaseDataSize + (BaseDataSize = (value == null ? 0 : value.DataSize)));
                _base = value;
            }
            get { return _base; }
        }

        public override bool Finalized
        {
            get { return _finalized; }
            set
            {

                if (_finalized) return;
                Base = Base;
                _finalized = value;
            }
        }

        private int BaseDataSize;
        private Class _base;
        public readonly bool IsClass = true;
        public bool AsCPUType = false;

        public bool IsCPUType
        {
            get
            {
                foreach (var baseClass in Assembly.CPUClasses)
                    if (ReferenceEquals(this, baseClass)) return true;
                return false;
            }
        }

        public Class(Class @base):this(@base,@base!=null)
        {
            
        }
        public Class(Class @base, bool isClass)
            : base(false)
        {
            IsClass = isClass;
            Base = isClass ? (@base ?? Assembly.Object) : null;
        }

        public static Class LoadAsStruct(Namespace nameSpace, string name, int size, bool AsCPUType = false)
        {
            var clss = new Class(null, false){Name = name, Parent = nameSpace, AsCPUType = AsCPUType};
            clss.SetDataSize(size);
            nameSpace.Scops.Add(clss);
            clss.Finalized = true;

            return clss;
        }

        public static Class LoadAsClass(Namespace nameSpace, Class @base, string name, int size)
        {
            var clss = new Class(@base, true){Name = name, Parent = nameSpace};
            nameSpace.Scops.Add(clss);
            clss.SetDataSize(clss.DataSize + size);
            clss.Finalized = true;
            return clss;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Class && Equals((Class) obj);
        }

        public static bool operator == (Class left, Class right)
        {
            if (Equals(left,right)) return true;
            if ( ReferenceEquals(null, left) || ReferenceEquals(null, right) ) return false;
            return right.IsSubClassOf(left);
        }

        public static bool operator != (Class left, Class right) { return !(left == right); }

        public override  bool Equals (string name) { return string.Compare(name, Name, StringComparison.Ordinal) == 0 || string.Compare(name, FullName, StringComparison.Ordinal) == 0; }
        public string FullName
        {
            get
            {
                Scop t = this;
                var s = "";
                while (t != null && !string.IsNullOrEmpty(t.Name))
                {
                    s = t.Name + (s != "" ? "." + s : "");
                    t = t.Parent;
                } 
                return s;
            }
        }

        

        public MethodInfo GetMethod(string name)
        {
            foreach (var scop1 in Scops)
            {
                var scop = (MethodInfo)scop1;
                if (string.Equals(scop.Name, name, StringComparison.Ordinal)) return scop;
            }
            return null;
        }

        public FieldInfo GetField (string name)
        {
            foreach ( var field in Vars ) { if ( field.Name.Equals(name, StringComparison.Ordinal) ) return field; }
            return null;
        }

        public List <MethodInfo> GetMethods ()
        {
            var methods = new List<MethodInfo>();
            foreach ( var scop1 in Scops ) {
                var scop = (MethodInfo) scop1;
                methods.Add((MethodInfo) scop1);
            }
            return methods;
        }
        public List <MethodInfo> GetMethods (string name)
        {
            var methods=new List <MethodInfo>();
            foreach (var scop1 in Scops)
            {
                var scop = (MethodInfo)scop1;
                if ( string.Equals(scop.Name, name, StringComparison.Ordinal) ) methods.Add(scop);
            }
            return methods;
        }

        public override string ToString () { return FullName; }

        public bool IsSubClassOf (Class c)
        {
            var h = this;
            while ( h != null )
                if (ReferenceEquals(c, h) ) return true;
                else h = h.Base;
            return false;
        }
        public int SizeOf()
        {
            return _dataSize;
        }
        public int fSizeOf()
        {
            return IsClass ? 4 : _dataSize;
        }
        public bool IsFromAssembly(CurrentScop root)
        {
            return root.Root == GetRoot();
        }
    }
}