using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Reflection;
using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation.Find;
using Method=Qs.Structures.MethodInfo;
namespace Qs.Parse.Expressions
{
    public class TypeCalc
    {
        //public QsSystem System;
        private readonly ByteCodeMapper _byteCodeMapper;

        public TypeCalc(ByteCodeMapper byteCodeMapper)
        {
            _byteCodeMapper = byteCodeMapper;
        }

        private static HeritachyType CopyTypes(HeritachyType firstHeritachy, IList<Tree> children)
        {
            HeritachyType temp;
            var tmp = temp = firstHeritachy;
            foreach (var t in children)
            {
                if (tmp == null) return null;
                if ((t.Type = tmp.Type) != null) t.Compiled = true;
                tmp = (temp = tmp).Children;
            }
            return temp;
        }

        private bool CalcHeritachyType(Tree tree)
        {
            Heritachy<Var> v;
            Heritachy<Class> e;
            var fn = _byteCodeMapper.CurrentScop.Finder;
            var hers = ByteCodeMapper.Split(tree.Content);
            var parent = tree.Parent;
            Heritachy<Method> t;
            if (parent != null && parent.Kind == Kind.Caller && (t = _byteCodeMapper.ClassLoader.GetMethod(parent[0], parent[1].Children)) != null)
            {
                tree.Type = t.main.Return;
                var tmp = t.HeritachyType;
                CopyTypes(t.HeritachyType, tree.Children);
            }
            else if ((v = fn.GetVariable(hers)) != null)
            {
                CopyTypes(v.HeritachyType, tree.Children);
                tree.Type = v.main.Return;
                tree.Membre=v.main;
            }
            else if ((e = fn.GetClass(hers)) != null)
            {
                tree.Type = CopyTypes(e.HeritachyType, tree.Children).Type;
                tree.Type = e.main;
            }
            else
            {
                var s = fn.GetSpace(hers);
                if (s == null) return SetCompiled(tree, false);
                tree.Type = fn.GetClass("System.void");
            }
            return SetCompiled(tree, true);
        }
        public bool CalcTypes(Tree tree)
        {
            if (tree.Compiled == true || tree.Content == "return") return true;
            switch (tree.Kind)
            {

                case Kind.Variable:
                    List<Method> mi;
                    var fi = _byteCodeMapper.ClassLoader.GetVariable(tree.Content.Trim());
                    if (fi == null)
                        if ((mi = _byteCodeMapper.CurrentScop.Finder.GetMethods(tree)).Count == 0) 
                            return SetCompiled(tree, false);
                        else
                        {
                            if (mi.Count != 1) throw new AmbiguousMatchException("Ambiguous Match Method Name: " + tree + " ");
                            tree.BaseCall = _byteCodeMapper.CurrentClass;
                            tree.Type = mi[0].Return;
                            tree.IsVariable_Method = false;
                            tree.Method=mi[0];
                            return SetCompiled(tree, true);
                        }
                    tree.BaseCall = fi.IsLocal ? (DataPortor)_byteCodeMapper.CurrentMethod : _byteCodeMapper.CurrentClass;
                    tree.Type = fi.Return;
                    tree.IsVariable_Method = true;
                    tree.Membre = fi;
                    return SetCompiled(tree, true);
                case Kind.Hyratachy:
                    tree[0].BaseCall = _byteCodeMapper.CurrentClass;
                    return CalcHeritachyType(tree);
                case Kind.Term:
                case Kind.Facteur:
                case Kind.Unair:
                case Kind.Expression:
                case Kind.Parent:
                case Kind.Logic:
                    return CalcExpressionType(tree);
                case Kind.Caller:
                    return CalcCallerType(tree);
                case Kind.EqAssign:
                    return CalcAssignType(tree);
                case Kind.TypeAssigne:
                    return CalcDeclareAssigneType(tree);
                case Kind.Assigne:
                    if (tree.Count == 2)
                        return CalcAssignType(tree);
                    return CalcDeclareAssigneType(tree);
                case Kind.Numbre:
                    return CalcNumbreType(tree);
                case Kind.String:
                    tree.Type = _byteCodeMapper.CurrentScop.Finder.GetClass("System.string");
                    return true;
                case Kind.Array:
                    return CalcCallerType(tree);
                case Kind.Return:
                    return CalcReturnType(tree);
                //case Kind.Logic:
                //    if (CalcTypes(tree[0]))
                //    {
                //        if (CalcTypes(tree[2]))
                //        {
                //            //if (tree[0].Membre.Return == tree[2].Membre.Return &&
                //            //    tree[0].Membre.Return.IsClass == tree[2].Membre.Return.IsClass)
                //            //{
                                
                //            //}
                //            tree.Type = _byteCodeMapper.CurrentScop.Finder.GetClass("System.bool");
                //            tree.Compiled = true;
                //            return true;
                //        }
                //    }
                //    return false;
            }
            return true;
        }

        private static bool SetCompiled(Tree tree, bool value)
        {
            tree.Compiled = value;
            return value;
        }

        private bool CalcReturnType(Tree tree)
        {
            tree[0].Kind = Kind.Variable;
            var z = CalcTypes(tree[1]);
            return z ? SetCompiled(tree, tree[0].Type == tree[1].Type) : SetCompiled(tree, false);
        }

        private bool CalcNumbreType(Tree tree)
        {
            var c=Constants.GetConstNumbre(tree.Content);
            if (c == null) return SetCompiled(tree, false);
            tree.Type = c.Return;
            tree.Membre = c;
            return SetCompiled(tree, true);
        }

        private bool CalcAssignType(Tree tree)
        {
            if (!CalcTypes(tree[1])) return SetCompiled(tree, false);
            tree[0].Type = tree[1].Type;
            tree.Type = tree[0].Type;
            tree.Membre = tree[0].Membre;
            return SetCompiled(tree, true);
        }

        private bool CalcDeclareAssigneType(Tree tree)
        {
            tree[1].Type = _byteCodeMapper.CurrentScop.Finder.GetClass(tree[0].Content);
            tree.Type = tree[1].Type;
            if (tree[1].Type == null) return SetCompiled(tree, false);
            if (tree.Count > 2)
            {
                if (!CalcTypes(tree[2])) return SetCompiled(tree, false);
                tree.Membre = tree[1].Membre;
                
                return SetCompiled(tree, tree[1].Type.FullName == tree[2].Type.FullName);
            }
            return SetCompiled(tree, true);
        }

        private bool CalcCallerType(Tree tree)
        {
            if (!CalcTypes(tree[0]) || !tree[0].IsVariable_Method.Equals(false)) return SetCompiled(tree, false);
            var tree1 = tree[1];
            var i = 0;
            for (; i < tree1.Count; i++) { if (!CalcTypes(tree1[i])) return SetCompiled(tree, false); }
            tree.Type = tree[0].Type;
            tree.Compiled = true;
            return SetCompiled(tree, true);
        }

        private bool CalcExpressionType(Tree tree)
        {
            if (tree[0].Kind == Kind.Unair || tree.Count == 2)
            {
                return SetCompiled(tree, CalcOperatorType(tree[0], null, tree[1]));
            }
            return SetCompiled(tree, CalcOperatorType(tree[1], tree[0], tree[2]));
        }

        private bool CalcOperatorType(Tree @operator, Tree left, Tree right)
        {
            var j = true;
            var ELeft = left != null;
            var ERight = right != null;
            if (!ELeft & !ERight) return false;

            if (ELeft) j &= CalcTypes(left);
            if (ERight && j) j = CalcTypes(right);
            if (!j) return false;
            var e = ELeft ? left.Type.GetMethods(@operator.Content) : null;
            var v = ERight ? right.Type.GetMethods(@operator.Content) : null;
            var d = ELeft ? e : v;
            if (ELeft && ERight)
            {
                if (left.Type != right.Type) e.AddRange(v);
                return CalcBOperatorType(e,@operator ,left, right);
            }
            return CalcUOperatorType(d, @operator,left ?? right);
        }

        private static bool CalcBOperatorType(IList<Method> e,Tree @operator, Tree left, Tree right)
        {
            foreach (var t in e)
            {
                if (t.Params.Count == 2)
                {
                    if (left.Type == t.Params[0].Return && right.Type == t.Params[1].Return)
                    {
                        left.Parent.Type = t.Return;
                        left.Parent.Method = @operator.Method = t;
                        return true;
                    }
                }
                var r = new GeoPosition<PersianCalendar>();

            }
            return false;
        }

        private static bool CalcUOperatorType(IEnumerable<Method> e, Tree @operator, Tree right)
        {
            foreach (var t in e)
            {
                if (t.Params.Count == 1)
                {
                    if (right.Type.FullName == t.Params[1].Return.FullName)
                    {
                        right.Parent.Type = t.Return;
                        right.Parent.Method = @operator.Method = t;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
