//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;
//using Compiler.Compiler.Assembly;
//using Qs.Enumerators;
//using Qs.Pdb;
//using Qs.Structures;
//using Qs.Utils.Indexation.Find;
//using MethodInfo = Qs.Structures.MethodInfo;

//namespace Qs.Utils.Indexation
//{
//    public class TypeCalc
//    {
//        private ByteCodeMapper _byteCodeMapper;
//        private VarManager varManager;
//        public bool CalcTypes(IList<Tree> trees)
//        {
//            for (var i = 0; i < trees.Count; i++) { if (!CalcTypes(trees[i])) return false; }
//            return true;
//        }

//        private static HeritachyType CopyTypes (HeritachyType firstHeritachy, IList <Tree> children)
//        {
//            HeritachyType ltmp;
//            var tmp = ltmp = firstHeritachy;
//            for (var i = 0; i < children.Count; i++) {
//                if (tmp == null) return null;
//                children[i].Type = tmp.Type;
//                if ( children[i].Type != null ) children[i].Compiled = true;
//                tmp = (ltmp = tmp).Children;
//            }
//            return ltmp;
//        }

//        public bool CalcHeratachyType (Tree tree)
//        {
//            Heritachy <Var> v;
//            Heritachy<Class> e;
//            var fn = _byteCodeMapper.CurrentScop.Finder;
//            var hers = ByteCodeMapper.Split(tree.Content);
//            var parent = tree.Parent;
//            Heritachy <MethodInfo> t;
//            if (parent != null && parent.Kind == Kind.Caller && (t = varManager.GetMethod(parent[0], parent[1].Children)) != null)
//            {
//                tree.Type = t.main.Return;
//                var tmp = t.HeritachyType;
//                CopyTypes(t.HeritachyType, tree.Children);
//            }
//            else if ( (v = fn.GetVariable(hers)) != null ) {
//                CopyTypes(v.HeritachyType, tree.Children);
//                tree.Type = v.main.Return;
//            }
//            else if ((e = fn.GetClass(hers)) != null)
//            {
//                tree.Type = CopyTypes(e.HeritachyType, tree.Children).Type;
//                tree.Type = e.main;
//            }
//            else if (fn.GetSpace(hers) == null) return SetCompiled(tree, false);
//            else tree.Type = fn.GetClass("System.void");
//            return SetCompiled(tree, true);
//        }
//        public bool CalcTypes(Tree tree)
//        {
//            if (tree.Content == "return") return true;
//            switch (tree.Kind) {
                    
//                case Kind.Variable:
//                    List<MethodInfo> mi;
//                    Structures.FieldInfo fi = varManager.GetVariable(tree.Content.Trim());
//                    if ( fi==null )
//                        if ((mi = _byteCodeMapper.CurrentScop.Finder.GetMethods(tree)).Count == 0 ) return SetCompiled(tree, false);
//                        else {
//                            if ( mi.Count != 1 ) throw new AmbiguousMatchException("Ambiguous Match Method Name: " + tree + " ");
//                            tree.BaseCall = _byteCodeMapper.CurrentClass;
//                            tree.Type = mi[0].Return;
//                            tree.IsVariable_Method = false;
//                            return SetCompiled(tree, true);
//                        }
//                    tree.BaseCall = fi.IsLocal ? (DataPortor) _byteCodeMapper.CurrentMethod : _byteCodeMapper.CurrentClass;
//                    tree.Type = fi.Return;
//                    tree.IsVariable_Method = true;
//                    tree.Membre = fi;
//                    return  SetCompiled(tree, true);
//                case Kind.Hyratachy:
//                    tree[0].BaseCall = _byteCodeMapper.CurrentClass;
//                    return CalcHeratachyType(tree);
//                case Kind.Term:
//                case Kind.Facteur:
//                case Kind.Unair:
//                case Kind.Expression:
//                case Kind. Parent:
//                    return CalcExpressionType(tree);
//                case Kind.Caller:
//                    return CalcCallerType(tree);
//                case Kind.EqAssign:
//                    return CalcAssignType(tree);
//                case Kind.TypeAssigne:
//                    return CalcDeclAssigneType(tree);
//                case Kind.Assigne:
//                    if ( tree.Count == 2 )
//                        return CalcAssignType(tree);
//                    return CalcDeclAssigneType(tree);
//                case Kind.Numbre:
//                    return CalcNumbreType(tree);
//                case Kind.String:
//                    tree.Type = _byteCodeMapper.CurrentScop.Finder.GetClass("System.string");
//                    return true;
//                case Kind.Array:
//                    return CalcCallerType(tree);
//                case Kind.Return:
//                    return CalcReturnType(tree);
//                case Kind.Logic:
//                    if(CalcTypes(tree[0])) 
//                        if ( CalcTypes(tree[2]) )
//                        { tree.Type = _byteCodeMapper.CurrentScop.Finder.GetClass("System.bool"); tree.Compiled = true; return true; }
//                    return false;
//            }
//            return true;
//        }

//        public static bool SetCompiled(Tree tree, bool value)
//        {
//            tree.Compiled = value;
//            return value;
//        }

//        private bool CalcReturnType (Tree tree)
//        {
//            tree[0].Kind = Kind.Variable;
//            var z = CalcTypes(tree[1]);
//            if(z)
//            return SetCompiled(tree, tree[0].Type == tree[1].Type);
//            return SetCompiled(tree, false);            
//        }

//        private bool CalcNumbreType (Tree tree)
//        {
//            double z;
//            if ( !double.TryParse(tree.Content, NumberStyles.Any, CultureInfo.InvariantCulture, out z) ) return SetCompiled(tree, false);
//            z = z > 0 ? z : -z;
//            var e = z%1;
//            tree.Type = Math.Abs(e) < 1e-9 ? _byteCodeMapper.CurrentScop.Finder.GetClass(z > int.MaxValue ? "long" : "int") : _byteCodeMapper.CurrentScop.Finder.GetClass(z > float.MaxValue ? "double" : "float");

//            return SetCompiled(tree, true);
//        }

//        private bool CalcAssignType (Tree tree)
//        {
//            if (!CalcTypes(tree[1])) return SetCompiled(tree,  false);
//            tree[0].Type = tree[1].Type;
//            tree.Type = tree[0].Type;
//            return  SetCompiled(tree, true);
//        }

//        private bool CalcDeclAssigneType (Tree tree)
//        {
//            tree[1].Type = _byteCodeMapper.CurrentScop.Finder.GetClass(tree[0].Content);
//            tree.Type = tree[1].Type;
//            if ( tree[1].Type == null ) return SetCompiled(tree, false);
//            if ( tree.Count > 2 ) {
//                if ( !CalcTypes(tree[2]) ) return SetCompiled(tree, false);

//                return SetCompiled(tree, tree[1].Type.FullName == tree[2].Type.FullName);
//            }
//            return SetCompiled(tree, true);
//        }

//        private bool CalcCallerType (Tree tree)
//        {
//            if ( !CalcTypes(tree[0]) || !tree[0].IsVariable_Method.Equals(false) ) return SetCompiled(tree, false);
//            var tree1 = tree[1];
//            var i = 0;
//            for (; i < tree1.Count; i++) { if (!CalcTypes(tree1[i])) return SetCompiled(tree, false); }
//            tree.Type = tree[0].Type;
//            tree.Compiled = true;
//            return SetCompiled(tree, true);
//        }

//        private bool CalcExpressionType (Tree tree)
//        {
//            if (tree[0].Kind == Kind.Unair || tree.Count == 2) { return SetCompiled(tree,  CalcOperatorType(tree[0], null, tree[1])); }
//            return SetCompiled(tree, CalcOperatorType(tree[1], tree[0], tree[2]));
//        }

//        private bool CalcOperatorType (Tree @operator, Tree left, Tree right)
//        {
//            var j = true;
//            var Eleft = left != null;
//            var Eright = right != null;
//            if(!Eleft & !Eright) return  false;

//            if ( Eleft ) j&=CalcTypes(left);
//            if (Eright & j) j &= CalcTypes(right);
//            if(!j) return false;

//            var e = Eleft ? left.Type.GetMethods(@operator.Content) : null;
//            var v = Eright ? right.Type.GetMethods(@operator.Content) : null;
//            var d = Eleft ? e : v;
//            if ( Eleft && Eright ) {
//                if(left.Type!=right.Type) e.AddRange(v);
//                return CalcBOperatorType(e, left, right);
//            }
//            return CalcUOperatorType(d, left ?? right);
//        }

//        private static bool CalcBOperatorType(IList<MethodInfo> e, Tree left, Tree right)
//        {
//            for (var i = 0; i < e.Count; i++)
//                if ( e[i].Params.Count == 2 ) {
//                    if ( left.Type.FullName != e[i].Params[0].Return.FullName ) continue;
//                    if ( right.Type.FullName != e[i].Params[1].Return.FullName ) continue;
//                    left.Parent.Type = e[i].Return;

//                    return true;
//                }
//            return false;
//        }

//        private static bool CalcUOperatorType(IList<MethodInfo> e, Tree right)
//        {
//            for (var i = 0; i < e.Count; i++)
//                if (e[i].Params.Count == 1)
//                {
//                    if (right.Type.FullName != e[i].Params[1].Return.FullName) continue;
//                    right.Parent.Type = e[i].Return;
//                    return true;
//                }
//            return false;
//        }
//    }
//}