using System;
using System.Collections.Generic;
using Qs.Enumerators;

namespace Qs.Utils.Base
{
    [Serializable]
    public class Pile
    {
        public readonly char[] Stream;
        public int CurrentPos { get; set; }
        public readonly List<int> PilePos;

        public Pile(string s)
        {
            CurrentPos = 0;
            Stream = s.ToCharArray();
            PilePos = new List<int>(5);
            Trace = new Trace(0, Stream.Length, Kind.Null);
        }
        public Pile(char[] s)
        {
            CurrentPos = 0;
            Stream = s;
            PilePos = new List<int>(5);
            Trace = new Trace(0, Stream.Length, Kind.Null);
        }

        

        public bool Close
        {
            get { return Stream.Length <= CurrentPos; }
        }

        public bool Open
        {
            get { return Stream.Length > CurrentPos; }
        }

        public char Current
        {
            get { return Stream[CurrentPos]; }
        }
        
        public bool Previous()
        {
            return --CurrentPos >= 0;
        }

        public bool Next()
        {
            return ++CurrentPos < Stream.Length;
        }
        
        public Trace Trace ;

        public void Save(Kind kind=Kind.Null)
        {
            Trace = Trace.Add(CurrentPos, -1, kind);
            PilePos.Add(CurrentPos);
        }

        private void MarkError(bool mark)
        {
            if (mark)
            {
                Trace.Error = true;
                Trace.End = CurrentPos;
                Trace = Trace.Parent;
            }
            else
            {
                var t = Trace.Parent;
                t.Remove(Trace);
                Trace = t;
            }
        }
        public bool Leave(bool savePos,bool mark=true)
        {
            if (savePos)
            {
                MarkError(false);
                PilePos.RemoveAt(PilePos.Count - 1);
                return true;
            }
            mark &= Trace.Start != CurrentPos && Trace.Kind != Kind.Null;
            MarkError(mark);
            CurrentPos = PilePos[PilePos.Count - 1];
            PilePos.RemoveAt(PilePos.Count - 1);
            return false;
        }
    }
}