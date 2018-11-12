using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Qs.Enumerators;
using Qs.IntelligentC.Optimization;
using Qs.Parse;
using Qs.Structures;
using Qs.Utils.Base;
using Qs.Utils.Syntax;

namespace Qs.Utils
{
    public delegate void BasicInstruction(Operand v1 = default(Operand), Operand v2 = default (Operand));

    public partial class Tree : IEnumerable<Tree>
    {
        public readonly List<Tree> Children;

        //private TempFieldInfo aVariable;

        //public Tree(string content, Kind kind, Tree parent = null)
        //{
        //    Pile = new Pile(content);
        //    End = content.Length - 1;
        //    Kind = kind;
        //    Children = new List<Tree>(0);
        //    Parent = parent;
        //}

        public Tree(Pile pile, Tree parent, Kind kind)
        {
            Parent = parent;
            Kind = kind;
            Children = new List<Tree>(0);
            Pile = pile;
        }

        /// <summary>
        ///   If the Value==Null ==> there are an Error else
        ///   If the Value==false ==> the analyse Symentique is not started
        ///   If the Value==Null ==> the analyse Symentique is started yet
        /// </summary>
        public bool? Compiled { get; set; }

        public bool? IsVariable_Method { get; set; }
        public Type This { get; set; }

        public Builder Builder { get; set; }

        public int Count
        {
            get { return Children.Count; }
        }

        public Tree this[int i]
        {
            get
            {
                return Children[i]; 
                
            }
            set { Children[i] = value; }
        }

        internal string Compile { get; set; }
        public Kind Kind { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public Tree Parent { get; set; }
        public Pile Pile { get; set; }
        public Class Type { get; set; }
        public FieldInfo Membre { get; set; }
        public ExtendParse GeneratedBy { get; set; }
        public string Content
        {
            get
            {
                var arr = new char[End - Start + 1];
                Array.Copy(Pile.Stream, Start, arr, 0, End - Start + 1);
                var result = new StringBuilder();
                foreach (char c in arr) result.Append(c);
                return result.ToString();
            }
        }

        public DataPortor BaseCall { get; set; }


        public void Set(string content, Kind kind)
        {
            Pile = new Pile(content);
            End = content.Length - 1;
            Kind = kind;
        }

        public Tree Add(Tree tree)
        {
            tree.Parent = this;
            Children.Add(tree);
            return tree;
        }

        public bool Set(bool save = true, bool Mark = true)
        {
            if (save)
            {
                Start = Pile.PilePos[Pile.PilePos.Count - 1];
                End = Pile.CurrentPos - 1;
                Parent.Children.Add(this);
            }
            return Pile.Leave(save, Mark);
        }

        IEnumerator<Tree> IEnumerable<Tree>.GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        public override string ToString()
        {
            return Kind + ": " + Content;
        }

        public string Join()
        {
            string s = "";
            if (Children.Count > 0) s = Children[0].Content;
            for (int i = 1; i < Children.Count; i++)
            {
                Tree item = Children[i];
                s += "," + item.Content;
            }
            return s;
        }

        internal void CloneFrom(Tree tree)
        {
            throw new NotImplementedException();
        }
    }

    public partial class Tree
    {
        public bool IsLife
        {
            get { return Kind == Kind.Numbre||Kind== Kind.Variable||Kind== Kind.Hyratachy|| Children == null || Children.Count == 0; }
        }

        public MethodInfo Method { get; set; }

        public FieldInfo Parcure(Tree parent, MethodInfo method)
        {
            if (Content == "this.a")
            {
                
            }
            if (this.Kind == Kind.Expression && Membre != null) return Membre;
            switch (Kind)
            {
                case Enumerators.Kind.Numbre:
                    break;
                case Enumerators.Kind.Variable:
                    return this.Membre;
                case Enumerators.Kind.String:
                    break;
            }
            if (this.Membre != null) return Membre;
            Membre = parent == null || parent.Membre == null
                ? new TempFieldInfo(Type, method)
                : parent.Membre is TempFieldInfo
                    ? ((TempFieldInfo) parent.Membre).As(Type).Disactivate()
                    : parent.Membre;
            if (Children.Count != 3) return Membre;
            var l = Children[0];
            var r = Children[2];
            //var b1 = !new Qs.Parse.Expressions.TypeCalc(null).CalcTypes(r);
            bool b = !l.IsLife || !r.IsLife;
            if (b)
            {
                var t = l.Parcure(this, method);
                if (!l.IsLife && t is TempFieldInfo) Activate(t);
                Disactivate(r.Parcure(this, method));
            }

            if (l.Membre != null) Disactivate(l.Membre);
            return Membre;
        }

        private static FieldInfo Activate(FieldInfo f)
        {
            return f is TempFieldInfo ? ((TempFieldInfo) f).Activate() : f;
        }

        private static FieldInfo Disactivate(FieldInfo f)
        {
            return f is TempFieldInfo ? ((TempFieldInfo) f).Disactivate() : f;
        }

        public void ToString(StringBuilder s)
        {
            Tree l, r;
            if (Count == 3)
            {
                r = Children[2];
                l = Children[0];
                if (!l.IsLife) l.ToString(s);
                if (!r.IsLife) r.ToString(s);
            }
            else return;
            if (r.IsLife && l.IsLife)
                s.Append(string.Format("{0}={1} {2} {3}\r", Membre.Name, l.Content, Children[1].Content,
                    r.Content));
            else if (r.IsLife && !l.IsLife)
                s.Append(string.Format("{0}={1} {2} {3}\r", Membre.Name, l.Membre.Name,
                    Children[1].Content, r.Content));
            else if (!r.IsLife && l.IsLife)
                s.Append(string.Format("{0}={1} {2} {3}\r", Membre.Name, l.Content, Children[1].Content,
                    r.Membre.Name));
            else
                s.Append(string.Format("{0}={1} {2} {3}\r", Membre.Name, l.Membre.Name,
                    Children[1].Content, r.Membre.Name));
        }

        public IEnumerator GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}