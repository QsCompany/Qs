using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Qs.Enumerators;
using Qs.Interfaces;
using Qs.Pdb;
using Qs.Utils.Syntax;

namespace Qs.Structures
{
    public class RegStat
    {
        public readonly Reg reg;
        public bool locked = false;

        public RegStat(Reg reg)
        {
            this.reg = reg;
        }
    }

    public enum ModeReg
    {
        Auto,
        Manual,
    }

    class StackFrame
    {
         
    }
    public class RegsStat
    {
        private readonly RegStat[] regstats;

        public RegsStat(Reg[] regs)
        {
            ModeReg = ModeReg.Manual;
            regstats = new RegStat[regs.Length];
            for (var i = 0; i < regs.Length; i++)
                regstats[i] = new RegStat(regs[i]);
        }

        public ModeReg ModeReg;

        private RegStat GetStat(Reg reg)
        {
            foreach (var t in regstats)
                if (t.reg == reg)
                    return t;
            return null;
        }

        public void Lock(Reg reg)
        {
            if (ModeReg == ModeReg.Auto) throw new Exception();
            var stat = GetStat(reg);
            if(stat==null)return;
            stat.locked = true;
        }

        public void UnLock(Reg reg)
        {
            if (ModeReg == ModeReg.Auto) throw new Exception();
            var stat = GetStat(reg);
            if (stat == null) return;
            stat.locked = false;
        }

        private readonly List<RegStat> autoReg = new List<RegStat>();
        public RegStat Next()
        {
            if (ModeReg == ModeReg.Manual) throw new Exception();
            foreach (RegStat t in regstats)
                if (!t.locked)
                {
                    t.locked = true;
                    autoReg.Add(t);
                    return t;
                }
            return null;

        }

        public void  Discart()
        {
            if (ModeReg == ModeReg.Manual) throw new Exception();
            if (autoReg.Count == 0) return;
            var t = autoReg[autoReg.Count];
            t.locked = false;
            autoReg.RemoveAt(autoReg.Count - 1);
        }
    }
    public abstract partial class Scop:IScop
    {
        public readonly EScop SType;
        internal readonly List <Scop> Scops;
        public string Name;
        public Scop Parent;

        protected Scop ()
        {
            Scops = new List <Scop>();
            if ( GetType() == typeof (MethodInfo) ) SType = EScop.Method;
            else {
                Scops = new List <Scop>();
                if ( GetType() == typeof (Class) ) SType = EScop.Class;
                else if ( GetType() == typeof (Namespace) ) SType = EScop.Namespace;
            }
        }
        public Scop GetByName (EScop _scop, string name)
        {
            return Scops.FirstOrDefault(scop => scop.Equals(name) && _scop == scop.SType);
        }

        /*
        public Method GetMethod (string name)
        {
            if ( this is Method && string.Equals(this.Name, name, StringComparison.Ordinal) ) return (Method) this;
            foreach ( var scop in this.Scops )
                if ( scop is Namespace ) {
                    var rt = scop.GetMethod(name);
                    if ( rt != null ) return rt;
                }
                else if ( scop is Class )
                    foreach ( var scop1 in scop.Scops ) {
                        var method = (Method) scop1;
                        if ( string.Equals(method.Name, name, StringComparison.Ordinal) ) return method;
                    }
                else if ( scop is Method && string.Equals(scop.Name, name, StringComparison.Ordinal) ) return (Method) scop;
            return null;
        }
        
        public Namespace GetSpace (string name)
        {
            if (this is Namespace && Equals(name)) return (Namespace)this;
            foreach (var scop in this.Scops)
                if ( scop is Namespace ) {
                    var space = scop.GetSpace(name);
                    if ( space != null ) return space;
                }
            return null;
        }
        */

        public abstract bool Equals (string name);
    }
    public partial class Scop
    {
        public Namespace GetSpace(string[] hers)
        {
            var d = GetSpace(hers[0], SearcheMode.Heigher);
            for (var i = 1; i < hers.Length; i++)
            {
                var her = hers[i];
                d = d.GetSpace(her, SearcheMode.Flaten);
                if (d == null) return null;
            }
            return d;
        }
        public Scop GetScop(string name, SearcheMode searcheMode)
        {
            foreach (var scop1 in Scops) if (scop1.Equals(name)) return scop1;
            switch (searcheMode)
            {
                case SearcheMode.Deep:
                    foreach (var scope1 in Scops)
                    {
                        Scop scope;
                        if ((scope = scope1.GetScop(name, searcheMode)) != null)
                            return scope;
                    }
                    break;
                case SearcheMode.Heigher:
                    return Parent == null ? null : Parent.GetScop(name, searcheMode);
            }
            return null;
        }
        public Namespace GetSpace(string name, SearcheMode searchesMode)
        {
            foreach (var scope1 in Scops)
            {
                if (!(scope1 is Namespace)) continue;
                var space = (Namespace)scope1;
                if (space.Equals(name)) return space;
            }
            switch (searchesMode)
            {
                case SearcheMode.Deep:
                    foreach (var scop in Scops)
                    {
                        var ns = scop as Namespace;
                        if (ns == null) continue;
                        var space = ns.GetSpace(name, searchesMode);
                        if (space != null) return space;
                    }
                    break;
                case SearcheMode.Heigher:
                    return Parent == null ? null : Parent.GetSpace(name, searchesMode);
            }
            return null;
        }
        public Class GetClass(string name, SearcheMode searcheMode)
        {
            foreach (var scop1 in Scops)
            {
                if (!(scop1 is Class)) continue;
                var clss = (Class)scop1;
                if (clss.Equals(name)) return clss;
            }
            switch (searcheMode)
            {
                case SearcheMode.Deep:
                    foreach (var scop1 in Scops)
                    {
                        var ns = scop1 as Namespace;
                        if (ns == null) continue;
                        var clss = ns.GetClass(name, searcheMode);
                        if (clss != null) return clss;
                    }
                    break;
                case SearcheMode.Heigher:
                    return Parent == null ? null : Parent.GetClass(name, searcheMode);
            }
            return null;
        }

        private static bool _testCompatibility (MethodInfo method, IList <Class> @params)
        {
            var i = 0;
            foreach ( var param in method.Params ) { if ( !param.Return.Equals(@params[i++]) ) return false; }
            return i == @params.Count;
        }
        private static bool _testCompatibility(MethodInfo method, IList<string> @params)
        {
            var i = 0;
            foreach (var param in method.Params) { if (param != null && !param.Return.Equals(@params[i++])) return false; }
            return i == @params.Count;
        }
        public MethodInfo GetMethod(string name, IList<string> @params)
        {
            Class @class;
            if (this is Class) @class = (Class)this;
            else if (this is MethodInfo) @class = (Class)Parent;
            else throw new PrivilegeNotHeldException();

            foreach (var scop in @class.Scops)
            {
                if (!(scop is MethodInfo)) continue;
                if (string.CompareOrdinal(scop.Name, name) == 0 && _testCompatibility((MethodInfo)scop, @params)) return (MethodInfo)scop;
            }
            return null;
        }
        public MethodInfo GetMethod(string name, IList<Class> @params)
        {
            Class @class;
            if ( this is Class ) @class = (Class) this;
            else if ( this is MethodInfo ) @class = (Class) Parent;
            else throw new PrivilegeNotHeldException();

            foreach ( var scop in @class. Scops ) {
                if ( !(scop is MethodInfo) ) continue;
                if (scop.Equals(name) && _testCompatibility((MethodInfo) scop, @params) ) return (MethodInfo) scop;
            }
            return null;
        }

        public FieldInfo GetField(string name, SearcheMode searcheMode)
        {
            if (!(this is DataPortor)) return null;
            var port = (DataPortor) this;
            if (string.CompareOrdinal(name, "this") == 0)
                return FieldInfo.CreateThis((Class) (this is MethodInfo ? Parent : this));
            if (string.CompareOrdinal(name, "base") == 0)
                return FieldInfo.CreateBase((Class) (this is MethodInfo ? Parent : this));
            l:
            foreach (var field in port.Vars.Where(field => string.CompareOrdinal(field.Name, name) == 0))
                return field;
            if (port is Class || searcheMode != SearcheMode.Heigher) return null;
            port = (DataPortor) Parent;
            goto l;
        }

        public FieldInfo GetField(LoadClasses load, Scop scop, FieldInfo _this, FieldInfo _field)
        {
            var _ret = _field.Clone();
            if (!_this.Return.IsClass)
            {
                _ret.Handle = _this.Handle + _field.Offset;
                _ret.Name = _this.Name + "." + _field.Name;
                return _ret;
            }
            load.Add("mov", RegInfo.eax, _this);
            load.Add("add", RegInfo.eax.Handle, new Operand(_field.Offset));
            if (_field.Return.IsClass)
            {
                
            }
            _ret.Handle = RegInfo.eax.Handle.AsPointer();

            return _ret;
        }
        public FieldInfo GetCField(string n)
        {
            var hers = n.Split(new[]{'.'});
            if (hers.Length == 0) return null;
            var s = GetField(hers[0], SearcheMode.Deep);
            if (s == null) return null;
            var h = s.Handle;
            for (var i = 1; i < hers.Length; i++)
            {
                s = s.Return.GetField(hers[i]);
                if (s == null) return null;
                h += s.Offset;
            }
            return new FieldInfo(n, s.Return, true) { Handle = h, OnLive = true };
        }

        public virtual Var GetVar (string name, SearcheMode searcheMode)
        {
            if (name == "this.a")
            {

            }
            var c = GetField(name, searcheMode);
            return c != null ? new Var(c) : null;
        }

        public Namespace GetRoot()
        {
            var b = this;
            while (b.Parent != null)
                b = b.Parent;
            return b as Namespace;
        }
        
    }
    public class Var : FieldInfo
    {
        public Var(FieldInfo baseField):base(baseField.Name,baseField.Return,baseField.IsLocal)
        {
            Handle = baseField.Handle;
            Name = baseField.Name;
            OnLive = baseField.OnLive;
            Return = baseField.Return;
            Offset = baseField.Offset;
            //IsLocal = baseField.IsLocal;
        }

        public override sealed int Offset
        {
            get { return _offset; }
            set { base.Offset = value; }
        }

        public void Push(FieldInfo baseField)
        {
            Name = baseField.Name;
            OnLive = baseField.OnLive;
            Return = baseField.Return;
            Offset = _offset + baseField.Offset;
        }
    }
    public enum EScop
    {
        Namespace = 1,
        Class = 2,
        Method = 4
    }
}
