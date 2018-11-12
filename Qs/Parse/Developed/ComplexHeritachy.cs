//using System;

using Qs.Enumerators;
using Qs.Parse.Expressions;
using Qs.Parse.Utils;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;


namespace Qs.Parse.Developed
{
    public delegate void Add(string function, Operand destination = null, Operand source = null);

    public class ComplexHeritachy : ExtendParse
    {
        public ComplexHeritachy(BasicParse basicParse)
            : base(EPNames.ComplexHeritachy, basicParse)
        {
        }

        private bool Word(Tree parent)
        {
            return this[EPNames.Word].Parse(parent);
        }

        public override bool Parse(Tree parent)
        {            
            BasicParse.Pile.Save(Kind.Word);
            var T = new Tree(BasicParse.Pile, parent, Kind.Hyratachy) {GeneratedBy = this};
            var vB1 = Word(T);
            while (vB1 && (BasicParse.GetKeyWord(BasicParse.Temp, ".") && Word(T)))
            {
                
            }
            if (T.Children.Count == 1)
            {
                T = T.Children[0];
                parent.Add(T);
                return Pile.Leave(vB1);
            }
            return T.Set(vB1);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }
    }

    public class New : ExtendParse
    {
        public New(BasicParse basicParse)
            : base(EPNames.New, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Caller);
            var T = new Tree(BasicParse.Pile, parent, Kind.Caller) {GeneratedBy = this};
            var callParameter = base[EPNames.CallParameter];
            return
                T.Set(BasicParse.GetKeyWord(BasicParse.Temp, "new") && BasicParse.GetHeritachy(T) &&
                      callParameter[T]);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }
    }

    public class Word : ExtendParse
    {
        public Word(BasicParse basicParse)
            : base(EPNames.Word, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save();
            var vB = SHyratachy(parent);
            if (!vB) vB = Caller(parent, true);
            if (!vB) vB = Caller(parent, false);
            if (!vB) vB = BasicParse.GetHeritachy(parent);
            if (!vB) vB = BasicParse.GetNumbre(parent);
            if (!vB) vB = BasicParse.GetString(parent);
            if (!vB) vB = Parent(parent);
            if (!vB) vB = UnairExpr();
            return BasicParse.Pile.Leave(vB);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }

        private bool UnairExpr()
        {
            return false;
        }

        private bool Parent(Tree parent)
        {
            return base[EPNames.Parent][parent];
        }


        private bool Caller(Tree parent, bool asMethod_Array)
        {
            return base[asMethod_Array ? EPNames.MethodCaller : EPNames.ArrayCaller][parent];
        }
        private bool SWord(Tree parent)
        {
            return base[EPNames.SWord].Parse(parent);
        }
        private bool SHyratachy(Tree parent)
        {
            return base[EPNames.SHyratachy].Parse(parent);
        }

    }

    public class SWord : ExtendParse
    {
        public SWord(BasicParse basicParse)
            : base(EPNames.SWord, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save();
            var vB = BasicParse.GetNumbre(parent);
            if (!vB) vB = BasicParse.GetString(parent);
            if (!vB) vB = BasicParse.GetVariable(parent, true);
            return BasicParse.Pile.Leave(vB);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return null;
        }
    }

    public class Variable:ExtendParse
    {
        public Variable(BasicParse basicParse)
            : base(EPNames.Variable, basicParse)
        {
        }
        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.Variable);
            var T = new Tree(BasicParse.Pile, parent, Kind.Variable) { GeneratedBy = this };
            var vB=BasicParse.GetVariable(parent);
            if (vB)
            {
                T.CloneFrom(T.Children[0]);
            }
            return T.Set(vB);
        }
    }

    public class SHyratachy : ExtendParse
    {
        public SHyratachy(BasicParse basicParse)
            : base(EPNames.SHyratachy, basicParse)
        {
        }
        public override bool Parse(Tree parent)
        {
            BasicParse.Pile.Save(Kind.SHyratachy);
            var T = new Tree(BasicParse.Pile, parent, Kind.SHyratachy) { GeneratedBy = this };
            var sword = base[EPNames.SWord];
            if (sword.Parse(T))
            {
                do
                    if (BasicParse.GetKeyWord(BasicParse.Temp, "."))
                        if (BasicParse.GetVariable(T))
                        {

                        }
                        else return T.Set(false);
                    else break;
                while (true);
            }
            else return T.Set(false);
            return T.Set(true);
        }
        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            return base.Compile(load, scop, tree);
        }
    }

    public class Expression : ExtendParse
    {
        public Expression(BasicParse basicParse)
            : base(EPNames.Expression, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            return Parse(parent, 0);
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            if (!new TypeCalc(load.ByteCodeMapper).CalcTypes(tree))
            {
                load.LogIn(scop, tree, this, "there are undefined variables or methods in expression : " + tree.Content);
                return tree[0].Membre;
            }
            tree.Parcure(tree.Parent, (MethodInfo) scop);
            var m = load.Compile(scop, tree[0]);
            var n = tree.Count == 3 ? load.Compile(scop, tree[2]) : null;
            if (tree.Method is CPUMethodInfo)
            {
                var e = Qs.Pdb.CPU.CPUType.Call(load, scop, tree.Membre, tree.Method, tree[0].Membre, tree[2].Membre);
                //TODO :Obsolete load.Caller.Call(scop, tree.Method, m, n, tree.Membre);
            }
            else
            {
                load.Optimum.PushParam(m);
                if(n!=null)load.Optimum.PushParam(n);
                load.Optimum.Call(tree.Method);
            }
            return tree.Membre;
        }

        public bool Parse(Tree parent, int i)
        {
            BasicParse.Pile.Save(Kind.Expression);
            var T = new Tree(BasicParse.Pile, parent, GetKind(i)) {GeneratedBy = this};
            bool vB;
            var vB1 = i < 4 ? Parse(T, ++i) : this[EPNames.ComplexHeritachy].Parse(T);
            if (vB1)
                do
                {
                    vB = BasicParse.GetOperator(T, Descripter.Operators[i - 1]);
                    if (vB) vB = Parse(T, i);
                } while (vB);
            T = Reform(T);
            T.GeneratedBy = this;
            if (vB1)
                if (T.Children.Count == 1)
                {
                    T = T.Children[0];
                    parent.Add(T);
                    return Pile.Leave(vB1);
                }
            return T.Set(vB1);
        }


        private static Kind GetKind(int i)
        {
            switch (i - 1)
            {
                case 0:
                    return Kind.Term;
                case 1:
                    return Kind.Facteur;
                case 2:
                    return Kind.Power;
                case 3:
                    return Kind.Logic;
                case 4:
                    return Kind.Word;
            }
            return Kind.Expression;
        }

        private Tree Reform(Tree tree)
        {
            if (tree.Children.Count <= 4) return tree;
            var lt = tree.Children[0];
            var last = tree.Children.Count - 1;
            for (var i = tree.Children.Count - 1; i >= 2; i -= 2)
            {
                var t = new Tree(BasicParse.Pile, null, Kind.Parent) {GeneratedBy = this};
                var d = tree.Children[last - i + 2];
                Add(t, lt, tree.Children[last - i + 1], d);
                t.Start = lt.Start;
                t.End = d.End;
                lt = t;
            }
            lt.Parent = tree.Parent;
            lt.Kind = tree.Kind;
            return lt;
        }

        private static void Add(Tree parent, params Tree[] child)
        {
            foreach (var tree in child)
            {
                parent.Children.Add(tree);
                tree.Parent = parent;
            }
        }

    }
}