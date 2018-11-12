using System.Collections.Generic;

namespace Qs.Utils.Base
{
    public class Builder
    {
        public Builder(string s)
        {
            CurrentPos = 0;
            Stream = s.ToCharArray();
            PilePos = new List<int>(5);
        }

        public bool Close
        {
            get { return Stream.Length <= CurrentPos; }
        }

        public char Current
        {
            get { return Stream[CurrentPos]; }
        }

        public int CurrentPos { get; set; }

        public bool Open
        {
            get { return Stream.Length > CurrentPos; }
        }

        public List<int> PilePos { get; set; }

        public char[] Stream { get; set; }

        public bool Leave(bool savePos)
        {
            if (savePos)
            {
                PilePos.RemoveAt(PilePos.Count - 1);
                return true;
            }
            CurrentPos = PilePos[PilePos.Count - 1];
            PilePos.RemoveAt(PilePos.Count - 1);
            return false;
        }

        public bool Next()
        {
            return ++CurrentPos < Stream.Length;
        }

        public bool Previous()
        {
            return --CurrentPos >= 0;
        }

        public void Save()
        {
            PilePos.Add(CurrentPos);
        }
    }
}