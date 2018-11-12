using System.Collections.Generic;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Base;

namespace Qs.Parse
{
    public abstract class ExtendParse:IParse
    {
        static ExtendParse()
        {
            
        }
        private readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

        public static void Update()
        {
            
        }

        public ExtendParse SetProperty(string propertyName, object value)
        {
            Properties[propertyName] = value;
            return this;
        }

        public T GetProperty<T>(string propertyName)
        {
            object o;
            if (Properties.TryGetValue(propertyName, out o)) return (T) o;
            var value = default(T);
            SetProperty(propertyName, value);
            return value;
        }

        protected readonly BasicParse BasicParse;

        protected ExtendParse(string abbreviation, BasicParse basicParse)
        {
            BasicParse = basicParse;
            ExtendParse ep;
            if (basicParse.Summary.TryGetValue(abbreviation, out ep)) return;
            basicParse.Summary.Add(abbreviation, this);
        }

        public abstract bool Parse(Tree parent);

        public ExtendParse this[string name]
        {
            get { return BasicParse.Summary[name]; }
        }

        public bool this[Tree parent]
        {
            get { return Parse(parent); }
        }
        public Tree Ts{get {return BasicParse.Temp; }}
        public Pile Pile{get { return BasicParse.Pile; }}

        public bool KeyWord(Tree parent, string key)
        {
            return BasicParse.GetKeyWord(parent, key);
        }

        public virtual FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            foreach (Tree t in tree)
                t.GeneratedBy.Compile(load, scop, t);
            return null;
        }

    }
}