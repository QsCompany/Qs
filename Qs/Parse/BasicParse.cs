using System.Collections.Generic;
using System.Globalization;
using Qs.Enumerators;
using Qs.Parse.Utils;
using Qs.Utils;

namespace Qs.Parse
{
    public sealed partial class BasicParse
    {
        internal Dictionary<string, ExtendParse> Summary = new Dictionary<string, ExtendParse>();
        

        public bool GetPMun()
        {
            Pile.Save();
            var vB = false;
            while (Pile.Open)
            {
                if (!Descripter.Operators[0].Contain(Pile.Current.ToString(CultureInfo.InvariantCulture))) break;
                vB = true;
                Pile.Next();
            }
            return Pile.Leave(vB);
        }

        public bool GetChiffre(bool Int = false)
        {
            Pile.Save();
            GetPMun();
            bool dot = false, start = true;
            while (Pile.Open)
            {
                if (!char.IsDigit(Pile.Current))
                    if (Pile.Current == '.' & !Int)
                        if (dot) return Pile.Leave(false);
                        else dot = true;
                    else break;
                start = false;
                Pile.Next();
            }
            return Pile.Leave(!start);
        }
      
        public bool GetWhiteSpace()
        {
            Pile.Save();
            var start = true;
            if (Pile.Close) return Pile.Leave(true);
            do
                if (!Contain(Eps, Pile.Current)) return Pile.Leave(!start);
                else start = false; while (Pile.Next());
            return Pile.Leave(true);
        }

        public bool GetKeyWord(Tree parent, string keyWord)
        {
            ESpace();
            Pile.Save(Kind.KeyWord);
            var T = new Tree(Pile, parent, Kind.KeyWord);
            foreach (var item in keyWord)
                if (!Pile.Open || item != Pile.Current) return Pile.Leave(false);
                else Pile.Next();
            return T.Set() & ESpace();
        }

        public bool GetNumbre(Tree parent)
        {
            var i = ESpace();
            Pile.Save();
            var T = new Tree(Pile, parent, Kind.Numbre);
            var n1 = GetChiffre();
            if (n1)
                i = GetKeyWord(Temp, "e") || GetKeyWord(Temp, "E")
                    ? (n1 = GetChiffre()) && GetKeyWord(Temp, "f") || GetKeyWord(Temp, "d")
                    : GetKeyWord(Temp, "f") || GetKeyWord(Temp, "d") || GetKeyWord(Temp, "u") ||
                      GetKeyWord(Temp, "l") || GetKeyWord(Temp, "ul") || GetKeyWord(Temp, "us");
            return T.Set(n1) && ESpace();
        }

        public bool GetString(Tree parent)
        {
            Pile.Save();
            var cls='"';
            ESpace();
            if ((Pile.Current == cls || Pile.Current == (cls = '\'')) && Pile.Next())
                if (GetString(parent, cls) && Pile.Next())
                    return Pile.Leave(true) & ESpace();
            return Pile.Leave(false);
        }
        private bool GetString(Tree parent,char cls)
        {
            Pile.Save();
            if (Pile.Close) return Pile.Leave(false);
            var T = new Tree(Pile, parent, Kind.String);
                while (Pile.Open)
                    if (Pile.Current == cls)
                        return T.Set();
                    else Pile.Next();
            return Pile.Leave(false) && ESpace();
        }
        ////public bool GetString(Tree parent)
        ////{
        ////    var i = ESpace();
        ////    Pile.Save();
        ////    if (Pile.Close) return Pile.Leave(false);
        ////    var sC = Pile.Current;
        ////    var T = new Tree(Pile, parent, Kind.String);
        ////    bool isc;
        ////    if (GetKeyWord(T, "\"") | (isc = GetKeyWord(T, "\'")))
        ////        while (Pile.Open)
        ////            if (Pile.Current == sC)
        ////            {
        ////                Pile.Next();
        ////                return T.Set();
        ////            }
        ////            else Pile.Next();
        ////    return Pile.Leave(false) && ESpace();
        ////}

        public bool GetVariable(Tree parent,bool trim=true)
        {
            var start = !trim || ESpace();
            Pile.Save(Kind.Variable);
            var T = new Tree(Pile, parent, Kind.Variable);
            while (Pile.Open)
            {
                if (char.IsDigit(Pile.Current)) { if (start) return Pile.Leave(false); }
                else if (char.IsLetter(Pile.Current)) { }
                else break;
                start = false;
                Pile.Next();
            }
            return T.Set(!start) && (!trim || ESpace());
        }
        public bool GetHeritachy(Tree parent)
        {
            //return Summary[Qs.Parse.Developed.EPNames.ComplexHeritachy].Parse(parent);
            
            ESpace();
            Pile.Save(Kind.Hyratachy);
            var T = new Tree(Pile, parent, Kind.Hyratachy);
            var vB1 = GetVariable(T,false);
            while (vB1 && GetKeyWord(Temp, ".") && GetVariable(T, false)) ;
            if (T.Children.Count == 1)
            {
                T = T.Children[0];
                T.Parent = parent;
                parent.Children.Add(T);
                return Pile.Leave(vB1) && ESpace();
            }
            T.GeneratedBy = Summary[Qs.Parse.Developed.EPNames.ComplexHeritachy];
            return T.Set(vB1) && ESpace();
        }

        internal bool GetOperator(Tree parent, IEnumerable<System.KeyValuePair<string, string>> list)
        {
            Pile.Save();
            var T = new Tree(Pile, parent, Kind.Operator);
            if (!Pile.Open) return Pile.Leave(false);
            bool any = false;
            foreach (var opr in list)
            {
                if (GetKeyWord(Temp, opr.Key))
                {
                    any = true;
                    break;
                }
            }
            if (any)
            {
                T.Set();
                T.Start = _tts[0].Start;
                T.End = _tts[0].End;
                return true;
            }
            return Pile.Leave(false);
        }
    }
}