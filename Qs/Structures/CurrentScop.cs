using System;
using System.Collections.Generic;
using Qs.Utils.Indexation;
using Qs.Utils.Indexation.Find;

namespace Qs.Structures
{
    public partial class CurrentScop
    {
        public readonly List <string> AssemblyList = new List <string>();
        public readonly List <FieldInfo> ConstsList = new List <FieldInfo>();
        public readonly List <Class> ImportList = new List <Class>();
        public readonly Dictionary <int, string> JumpInstruction = new Dictionary <int, string>();
        public readonly VariableGenerator Labels = new VariableGenerator("<lab_", ">");
        public readonly Dictionary <string, int> LabelsInstruction = new Dictionary <string, int>();
        public readonly List <CurrentScop> References = new List <CurrentScop>();
        public readonly List <Namespace> Usings = new List <Namespace>();
        public int EPSInc = 0;
        public Scop _Current;
        public readonly Finder Finder;
        public Scop Current
        {
            get { return _Current; }
            set
            {
                if ( value == null ) throw new NullReferenceException();
                _Current = value;
            }
        }

        internal Class CurrentClass
        {
            get
            {
                if (_Current is MethodInfo) return (Class) _Current.Parent;
                return _Current is Class ? (Class) _Current : null;
            }
        }

        public MethodInfo CurrentMethod
        {
            get
            {
                if (_Current is MethodInfo) return (MethodInfo)_Current;
                return null;
            }
        }

        public DataPortor CurrentDataPortor
        {
            get { return _Current as DataPortor; }
        }

        internal bool Add (Class @base, string className)
        {
            var b = true;
            var _new = GetByName(EScop.Class, className);
            if (_new == null)
            {
                if (Current.SType == EScop.Namespace) _new = new Class(@base);
                else throw new Exception();
                _new.Name = className;
                _new.Parent = Current;
                _new.Parent.Scops.Add(_new);
                b = false;
            }
            else ((Class) _new).Base = @base;
            SetCurrent(_new);
            return b;
        }
    }
    public partial class CurrentScop
    {
        private CurrentScop (string globalSpaceName)
        {
            Finder = new Finder(this);
            Root = (Namespace) (Current = new Namespace(globalSpaceName ?? "Global"));
            foreach ( var ass in Assembly.Assemblies ) {
                if ( ass.Current.Name != "System" ) continue;
                References.Add(ass);
                Usings.Add(ass.Root);
                return;
            }
        }

        internal void SetCurrent (Scop current) { _Current = current; }

        public static CurrentScop Initialize (string globalSpaceName)
        {
            var currentScop = new CurrentScop(globalSpaceName);
            Assembly.Assemblies.Add(currentScop);
            return currentScop;
        }

        public void Add (FieldInfo var)
        {
            if ( Current.SType == EScop.Namespace ) throw new Exception();
            switch ( Current.SType ) {
                case EScop.Class:
                case EScop.Method:
                    ((DataPortor) Current).Add(var);
                    break;
            }
        }

        public FieldInfo New (Class type, string name)
        {
            if ( Current.SType != EScop.Namespace )
                switch ( Current.SType ) {
                    case EScop.Class:
                    case EScop.Method:
                        return ((DataPortor) Current).New(type, name);
                }
            return null;
        }

        public bool Add (EScop scop, string name)
        {
            Scop _new;
            return Add(scop, name, out _new);
        }

        /// <returns>If The Method Existed Or Created   </returns>
        public bool Add (EScop scop, string name, out Scop _new)
        {
            var b = true;
            _new = GetByName(scop, name);
            if ( _new == null ) {
                if ( Current.SType == EScop.Namespace )
                    if ( scop == EScop.Class ) _new = new Class(null);
                    else if ( scop == EScop.Namespace ) _new = new Namespace(name);
                    else throw new Exception();
                else if (Current.SType == EScop.Class)
                    if ( scop == EScop.Method ) _new = new MethodInfo();
                    else throw new Exception();
                else throw new Exception();
                _new.Name = name;
                _new.Parent = Current;
                _new.Parent.Scops.Add(_new);
                b = false;
            }
            SetCurrent(_new);
            return b;
        }

        private Scop Out ()
        {
            var ret = Current;
            SetCurrent(Current.Parent);
            return ret;
        }

        internal Scop Out (EScop eScop)
        {
            if ( _Current.SType == eScop ) return Out();
            throw new Exception();
        }

        public IEnumerable <T> GetEnumerator <T> (string name) where T : Scop
        {
            foreach ( var sc in GetEnumerator <T>(Root) ) if ( sc.Equals(name) ) yield return sc;
            foreach ( var reference in References ) foreach ( var sac in reference.GetEnumerator <T>(reference.Root) ) yield return sac;
        }

        public IEnumerable <T> GetEnumerator <T> (Scop root) where T : Scop
        {
            foreach ( var scp in root.Scops ) {
                if ( scp is T ) yield return (T) scp;
                if ( scp is Namespace ) foreach ( var sc in GetEnumerator <T>(scp) ) yield return sc;
                if ( !typeof (MethodInfo).IsSubclassOf(typeof (T)) || !(scp is Class) ) continue;
                foreach ( var scop in scp.Scops ) yield return (T) scop;
            }
        }
    }
    public partial class CurrentScop
    {
        public readonly Namespace Root;
        
        private Scop GetByName (EScop _scop, string name)
        {
            foreach ( var scop in _Current.Scops ) if ( scop.Name == name && _scop == scop.SType ) return scop;
            return null;
        }
    }
}
