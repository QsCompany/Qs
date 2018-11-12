using System.Collections.Generic;
using System.Runtime.InteropServices;
using Qs.Enumerators;

namespace Qs.Utils.Base
{
    [ComVisible(true)]
    public sealed class Trace:List<Trace>
    {
        public int End;
        public string Identifier;
        public Trace Parent;
        public bool Error ;
        public int Start;

        public Trace Add(int start, int end, Kind kind)
        {
            var trace = new Trace(start, end, kind);
            Add(trace);
            return trace;
        }

        public new Trace Add(Trace item)
        {
            base.Add(item);
            item.Parent = this;
            return item;
        }
        public Trace(int start, int end, Kind kind)
        {
            End = end;
            Start = start;
            Kind = kind;
        }

        public Trace(Kind kind)
        {
            Kind = kind;
        }

        public Kind Kind { get; set; }
    }
}