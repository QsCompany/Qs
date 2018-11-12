using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Qs.Structures;

namespace Qs.IntelligentC.Optimization
{
    public class TempFieldInfo:FieldInfo
    {
        public override string ToString()
        {
            return Return + " var" + Index + (Active ? "(activated)" : "(disactivated)");
        }

        private readonly MethodInfo method;
        public readonly List <TempFieldInfo> Children = new List <TempFieldInfo>();
        public readonly int Index;
        public bool Active { get; private set; }

        private TempFieldInfo(Class @return, int index, MethodInfo method) : base("<temp>" + index, @return,true)
        {            
            Index = index;
            this.method = method;
            //isLocal = true;
            //method.Global = parent == null ? new List <TempFieldInfo> {this} : parent.method.Global;
            //GlobalCloned = parent == null ? new List<TempFieldInfo> { this } : parent.GlobalCloned;
        }

        public TempFieldInfo (Class @return, MethodInfo method) : this(@return, 0, method)
        {
        }

        public TempFieldInfo Add (Class type)
        {
            var t = new TempFieldInfo(type, method.TempVars.Count, method);
            method.TempVars.Add(t);
            Children.Add(t);
            return t;
        }

        public override  FieldInfo Clone ()
        {
            var r = new TempFieldInfo(Return, Index, method);
            method.TempVarsCloned.Add ( r );
            return r;
        }

        public TempFieldInfo As (Class type)
        {
            var @var = Get(type, new Collection <int>());
            if ( @var == null ) return Add(type);
            @var = (TempFieldInfo) @var.Clone();
            Children.Add(@var);
            return @var;
        }

        private TempFieldInfo Get (Class type, ICollection <int> prevent)
        {
            if ( prevent.Contains(Index) ) return null;
            prevent.Add(Index);
            if ( !Active && Return == type ) return this;
            foreach (var var in method.TempVars)
                if (!var.Active && var.Return == type) return var;
            return null;
            //foreach ( var child in this.Children ) {
            //    var t = child.Get(type, prevent);
            //    if ( t != null ) return t;
            //}
            //return this.Parent != null ? this.Parent.Get(type, prevent) : null;
        }

        public TempFieldInfo Activate()
        {
            foreach ( var var in method.TempVars.Where(var => var.Index == Index) )
        //        if ( @var.Active ) throw new SynchronizationLockException();
            //    else 
            @var.Active = true;
            foreach (var var in method.TempVarsCloned.Where(var => var.Index == Index))
                //        if ( @var.Active ) throw new SynchronizationLockException();
                //    else 
                @var.Active = true;
            return this;
        }

        public TempFieldInfo Disactivate()
        {
            foreach (var var in method.TempVars.Where(var => var.Index == Index)) @var.Active = false;
            foreach (var var in method.TempVarsCloned.Where(var => var.Index == Index)) @var.Active = false;
            return this;
        }
    }
}