using System;
using System.Collections.Generic;
using Qs.Enumerators;
using Qs.Structures;

namespace Qs.Utils.Indexation.Find
{
    public partial class Finder
    {

        public List<MethodInfo> GetMethods(Tree val)
        {
            if (val.Kind == Kind.Variable) return CurrentScop.CurrentClass.GetMethods(val.Content);
            if (val.Kind != Kind.Hyratachy) throw new Exception("Function expected");
            var cm = new string[val.Count - 1];
            int k;
            for (k = 0; k < cm.Length; k++) cm[k] = val[k].Content;
            var heritability = GetVariable(cm);
            return heritability == null ? new List<MethodInfo>(0) : heritability.main.Return.GetMethods(val[k].Content);
        }

    }
    public partial class Finder
    {
        public static T GetBackFirst<T>(Scop scop) where T : Scop
        {
            while (!(scop is T)) scop = scop.Parent;
            return (T)scop;
        }

        public Class GetClass_Name_Partiel(string name)
        {
            var currentNameSpace = GetBackFirst<Namespace>(CurrentScop._Current).Scops;
            foreach (var @class in currentNameSpace) if (String.CompareOrdinal(@class.Name, name) == 0) return (Class)@class;
            foreach (var @using in CurrentScop.Usings)
            {
                Class _class;
                if ((_class = @using.GetClass(name, SearcheMode.Heigher)) != null) return _class;
            }
            return null;
        }

        internal Namespace GetSpace_Name_Partiel(string name, ICollection<Namespace> exclude)
        {
            foreach (var ns in CurrentScop.Usings)
            {
                if (exclude.Contains(ns)) continue;
                exclude.Add(ns);
                if (String.CompareOrdinal(ns.Name, name) == 0) return ns;
                var d = GetSpace_Name_Partiel(name, exclude);
                if (d != null) return d;
            }
            return null;
        }

        public Namespace GetSpace_Entrer(string name)
        {
            var exclude = new List<Namespace>();
            foreach (var s in CurrentScop.References)
            {
                var d = s.Finder.GetSpace_Name_Partiel(name, exclude);
                if (d != null) return d;
            }
            return null;
        }

        public bool FunctionPrototypeExist(string name, IList<Tree> parameters, out MethodInfo methodInfoInfo)
        {
            var _class = (CurrentScop.CurrentMethod == null ? CurrentScop._Current : CurrentScop.CurrentMethod.Parent) as Class;
            foreach (var scop in _class.Scops)
            {
                var method = (MethodInfo)scop;
                if (!method.Name.Equals(name)) continue;
                var b = true;
                if (parameters.Count != method.Params.Count) continue;
                for (var j = 0; j < parameters.Count; j++)
                {
                    var s = parameters[j].Children[0].Content;
                    var param = method.Params[j];
                    if (s.Equals(param.Return.FullName) || s.Equals(param.Name)) continue;
                    b = false;
                    break;
                }
                if (!b) continue;
                methodInfoInfo = method;
                return true;
            }
            methodInfoInfo = null;
            return false;
        }
    }

    public partial class Finder
    {
        public readonly CurrentScop CurrentScop;

        public Finder(CurrentScop currentScop)
        {
            CurrentScop = currentScop;
        }

        public Var GetVariable(string name)
        {
            if (CurrentScop.Current.SType == EScop.Method) return CurrentScop.Current.GetVar(name, SearcheMode.Heigher);
            throw new Exception("the scop for getting the variable is not authorized");
        }

        //public IScop Get (string eher)
        //{
        //    var hers = new[] {eher};
        //    var c =(IScop) GetVariable(hers);
        //    if ( c != null ) return c;
        //}
        public Heritachy<Var> GetVariable(IList<string> hers)
        {
            HeritachyType vh;
            if (hers == null || hers.Count == 0) return null;
            var var = GetVariable(hers[0]);
            if (var == null) return null;
            var tmp = vh = new HeritachyType(null, hers[0], var);
            var h = new Heritachy<Var>(vh, null);
            var scop = var.Return;
            for (var i = 1; i < hers.Count; i++)
            {
                var t = scop.GetField(hers[i], SearcheMode.Flaten);
                if (t == null) return null;
                var.Push(t);
                scop = t.Return;
                tmp = new HeritachyType(tmp, hers[i], new Var(t));
            }
            h.main = var;
            return h;
        }

        public MethodInfo GetMethod(string name, IList<Class> @params)
        {
            var method = CurrentScop._Current.GetMethod(name, @params);
            if (method != null) return method;
            foreach (var @class in CurrentScop.ImportList)
            {
                method = @class.GetMethod(name, @params);
                if (method != null) return method;
            }
            return null;
        }

        public MethodCallHiretachy GetMethod(IList<string> hers, IList<Class> @params)
        {
            if (hers == null || hers.Count == 0) return null;
            var deb = hers[0];
            HeritachyType tmp;
            if (hers.Count == 1)
            {
                var method = GetMethod(deb, @params);
                return new MethodCallHiretachy(new HeritachyType(deb, method), method);
            }
            Var var;
            MethodCallHiretachy h = null;
            Scop scop;
            if ((var = GetVariable(deb)) != null)
            {
                h = new MethodCallHiretachy(tmp = new HeritachyType(deb, var), null);
                scop = var.Return;
                for (var i = 1; i < hers.Count - 1; i++)
                {
                    var t = scop.GetField(hers[i], SearcheMode.Flaten);
                    if (t == null) return null;
                    var.Push(t);
                    scop = t.Return;
                    tmp = tmp.Add(hers[i], t);
                }
                if (scop == null) return null;
                tmp.Add(hers[hers.Count - 1], h.main = scop.GetMethod(hers[hers.Count - 1], @params));
            }
            else if ((scop = GetClass(deb)) != null)
            {
                h = new MethodCallHiretachy(tmp = new HeritachyType(deb, scop));
                tmp.Add(hers[hers.Count - 1], h.main = hers.Count != 2 ? null : scop.GetMethod(hers[1], @params));
            }
            else if ((scop = GetSpace(deb)) != null)
            {
                h = new MethodCallHiretachy(tmp = new HeritachyType(deb, scop));
                for (var i = 1; i < hers.Count - 2; i++)
                    if ((scop = scop.GetSpace(hers[i], SearcheMode.Flaten)) == null) return null;
                    else tmp = tmp.Add(hers[i], scop);
                tmp = tmp.Add(hers[hers.Count - 2], scop = scop.GetClass(hers[hers.Count - 2], SearcheMode.Flaten));
                if (scop == null) return null;
                tmp.Add(hers[hers.Count - 1], h.main = scop.GetMethod(hers[hers.Count - 1], @params));
            }
            return h;
        }

        public Class GetClass(Namespace sp, string name, SearcheMode searcheMode = SearcheMode.Deep)
        {
            var @class = sp.GetClass(name, SearcheMode.Deep);
            if (@class != null) return @class;
            foreach (var @using in CurrentScop.Usings)
            {
                @class = @using.GetClass(name, SearcheMode.Flaten);
                if (@class != null) return @class;
            }
            return null;
        }

        public Class GetClass(string name,SearcheMode searcheMode= SearcheMode.Heigher)
        {
            var @class = CurrentScop.Current.GetClass(name, searcheMode);
            if (@class != null) return @class;
            foreach (var @using in CurrentScop.Usings)
            {
                @class = @using.GetClass(name, SearcheMode.Flaten);
                if (@class != null) return @class;
            }
            return null;
        }

        public Heritachy<Class> GetClass(IList<string> hers)
        {
            HeritachyType tmp;
            if (hers == null || hers.Count == 0) return null;
            if (hers.Count == 1) return new Heritachy<Class>(tmp = new HeritachyType(hers[0], GetClass(hers[0])), (Class)tmp.Scop);
            var space = GetSpace(hers[0]);
            if (space == null) return null;
            var h = new Heritachy<Class>(tmp = new HeritachyType(null, hers[0], space), null);
            for (var i = 1; i < hers.Count - 1; i++)
            {
                space = space.GetSpace(hers[i], SearcheMode.Flaten);
                if (space == null) return null;
                tmp = tmp.Add(hers[i], space);
            }
            var @class = space.GetClass(hers[hers.Count - 1], SearcheMode.Flaten);
            if (@class != null) tmp.Add(hers[hers.Count - 1], @class);
            else return null;
            h.main = @class;
            return h;
        }


        public Namespace GetSpace(string name)
        {
            var @class = CurrentScop.Current.GetSpace(name, SearcheMode.Heigher);
            if (@class != null) return @class;
            foreach (var @using in CurrentScop.Usings)
            {
                @class = @using.GetSpace(name, SearcheMode.Flaten);
                if (@class != null) return @class;
            }
            foreach (var @using in CurrentScop.References)
            {
                if (string.CompareOrdinal(@using.Root.Name, name) == 0) return @using.Root;
                @class = @using.Finder.GetSpace(name);
                if (@class != null) return @class;
            }
            return null;
        }

        public Namespace GetSpace(IList<string> hers)
        {
            if (hers == null || hers.Count == 0) return null;
            if (hers.Count == 1) return GetSpace(hers[0]);
            var space = GetSpace(hers[0]);
            for (var i = 1; i < hers.Count; i++)
            {
                if (space == null) return null;
                space = space.GetSpace(hers[i], SearcheMode.Flaten);
            }
            return space;
        }

    }
}