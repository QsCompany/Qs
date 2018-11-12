using System;

namespace Qs.Structures
{
  
    public sealed class Namespace : Scop
    {
        public string FullName
        {
            get
            {
                Scop t = this;
                var s = "";
                while (t != null && !string.IsNullOrEmpty(t.Name))
                {
                    s = t.Name + (s != "" ? "." + s : "");
                    t = t.Parent;
                }
                return s;
            }
        }

        public override bool Equals(string name) { return string.Compare(name, Name, StringComparison.Ordinal) == 0 || string.Compare(name, FullName, StringComparison.Ordinal) == 0; }

        public Namespace (string name) { Name = name; }
    }
}