using System.Collections.Generic;
using System.Linq;
using Qs.Enumerators;
using Qs.Utils;
using Qs.Utils.Base;

namespace Qs.Parse
{
    interface IParse
    {
        bool Parse(Tree parent);
    }

    public partial class BasicParse
    {
        public Tree Temp
        {
            get
            {
                if (_tts == null) _tts = new Tree(this.Pile, null, Kind.Program);
                _tts.Children.Clear();
                return _tts;
            }
            set { _tts = value; }
        }
        const string Eps = " \0\n\t\r\v";
        public bool PESpace()
        {
            return ESpace();
        }
        private bool ESpace()
        {
            while (Pile.Open)
            {
                if (!Contain(Eps, Pile.Current)) break;
                Pile.Next();
            }
            return true;
        }
        
        public bool Contain(IList<char> list, Tree parent)
        {
            Pile.Save();
            var T = new Tree(Pile, parent, (Kind) Kind.Operator);
            return Pile.Open && list.Contains(Pile.Current) ? Pile.Next() | T.Set() : Pile.Leave(false);
        }
        public static bool Contain(string chainChars, char charact)
        {
            return chainChars.Any(t => t == charact);
        }

        public Pile Pile;
        private Tree _tts;
        public Trace Trace = new Trace(Kind.Null);
    }
}
