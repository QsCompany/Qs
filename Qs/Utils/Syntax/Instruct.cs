#pragma warning disable 659
using Qs.Enumerators;
using Qs.IO.Stream;
using Qs.Structures;
using Qs.Utils.Base;

namespace Qs.Utils.Syntax
{
    public sealed class ProgInstruct:Instruct
    {
        private FieldInfo destination;
        private FieldInfo source;
        public override Operand Destination
        {
            get
            {
                return destination == null ? null : destination.Handle;
            }
            set
            {
                throw null;
                //base.Destination = value;
            }
        }
        public override Operand Source
        {
            get
            {
                return source == null ? null : source.Handle;
            }
            set
            {
                throw null;
                //base.Destination = value;
            }
        }

        public ProgInstruct(string fnName, FieldInfo destination=null, FieldInfo source = null):base(fnName)
        {
            this.destination = destination;
            this.source = source;
        }
    }
    public class Instruct
    {
        public CPUFunction Function;
        public virtual Operand Destination { get; set; }
        public virtual Operand Source { get; set; }


        public override bool Equals (object obj)
        {
            var e = obj as Instruct;
            if ( e == null ) return false;
            var r = e.Function.Index == Function.Index;
            if (Function.Script == Script.ZeroParam) return r;
            if (Destination != null && !Destination.IsNone) r &= Destination.Equals(e.Destination);
            return Function.Script == Script.TwoParam && !Destination.IsNone && Source != null && !Source.IsNone ? r & Source.Equals(e.Source) : r;
        }

        public int Length
        {
            get
            {
                var r = 8;
                if (Function.Script == Script.ZeroParam) return r;
                if (Destination != null && !Destination.IsNone) r += Destination.Length;
                return
                    _int(Function.Script == Script.TwoParam && !Destination.IsNone && Source != null && !Source.IsNone
                        ? r + Source.Length
                        : r);
            }
        }

        public Instruct (string fnName, Operand destination=null, Operand source = null)
        {
            Function = CUal.GetFunction(fnName);
            Destination = destination;
            Source = source;
        }
        protected Instruct(string fnName)
        {
            Function = CUal.GetFunction(fnName);
        }

        protected Instruct(byte hashFn)
        {
            Function = CUal.GetFunction(hashFn);
        }
        public Instruct(byte hashFn, Operand destination = null, Operand source = null)
        {
            Function = CUal.GetFunction(hashFn);
            Destination = destination;
            Source = source;
        }

        public void Push(StreamWriter s)
        {
            s.push(Function.Index, 8);
            if (Function.Script == Script.OneParam)
                Destination.push(s);
            else if (Function.Script == Script.TwoParam)
            {
                Destination.push(s);
                if (!Destination.IsNone) Source.push(s);
            }
            s.shift();
        }

        public override string ToString ()
        {
            var s = Function.Name;
            var zeroParam = Destination == null || Destination.IsNone || Function.Script == Script.ZeroParam;
            if ( !zeroParam ) s += "    " + Destination;
            else return s;
            if ( Function.Script == Script.TwoParam ) s += ",   " + Source;
            return s + "     (" + Length + ")";
        }

        private int _int(int n)
        {
            if (n%8 == 0) return n/8;
            return n/8 + 1;
        }

        public static Instruct Pop(StreamReader r, int offset)
        {
            r.Save();
            Instruct i=null;
            if (r.Seek(offset))
                i = Pop(r);
            r.Restore();
            return i;
        }

        public static Instruct Pop (StreamReader r)
        {
            var i = new Instruct((byte) r.read(8));
            switch ( i.Function.Script ) {
                case Script.OneParam:
                    i.Destination = Operand.Pop(r);
                    break;
                case Script.TwoParam:
                case Script.ThirdState:
                    i.Destination = Operand.Pop(r);
                    if ( !i.Destination.IsNone ) i.Source = Operand.Pop(r);
                    break;
            }
            r.shift();
            return i;
        }

        internal void Push(StreamWriter r, int offset)
        {
            r.Save();
            if (!r.Seek(offset))
                r.Restore();
            else
                Push(r);
            r.Restore();

        }
    }
}