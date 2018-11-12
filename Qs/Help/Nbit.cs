using System;
using System.Globalization;

namespace Qs.Help
{
    public struct Nbit
    {
        public bool Equals(Nbit other)
        {
            var e = this;
            Update(ref e, ref other);
            return e._value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Nbit && Equals((Nbit) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private readonly byte Size;

        public int Value
        {
            get { return _value; }
            set { _value = swap(value, Size); }
        }
        private int _value;
        public Nbit(byte size):this()
        {
            if (size > 32) throw new Exception("overflow");
            Size = size;
        }
        public Nbit(byte size, int val)
            : this(size)
        {
            Value = val;
        }

        public static Nbit Convert(Nbit a, byte fsize)
        {
            var b = new Nbit(fsize);
            b.Value = a.IsNeg() ? Negate((-a)._value, fsize) : a.Value;
            return b;
        }

        public static void Update(ref Nbit a,ref Nbit b)
        {

            if (b.Size > a.Size)
                a = Convert(a, b.Size);
            else if (b.Size < a.Size) b = Convert(b, a.Size);
        }

        

        public static Nbit operator /(Nbit a, Nbit b)
        {
            Update(ref a,ref b);
            var c = a.Value * b.Value;
            var s = a.Size > b.Size ? a.Size : b.Size;
            return new Nbit(s)
            {
                Value = c
            };
        }
        public static Nbit operator *(Nbit a, Nbit b)
        {
            Update(ref a, ref b);
            if (TimeOverflow(a, b)) throw new Exception("overflow");
            var c = a.Value*b.Value;
            var s = a.Size > b.Size ? a.Size : b.Size;
            return new Nbit(s)
            {
                Value = c
            };
        }

        public static Nbit operator +(Nbit a, Nbit b)
        {
            Update(ref a, ref b);
            if (PlusOverflow(a, b)) throw new Exception("overflow");
            var c = a.Value + b.Value;
            var s = a.Size > b.Size ? a.Size : b.Size;
            return new Nbit(s)
            {
                Value = c
            };
        }


        public static bool operator ==(Nbit a, Nbit b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Nbit a, Nbit b)
        {
            return !(a == b);
        }

        private bool IsNeg()
        {
            return (Value & Bit.Bins[Size - 1]) != 0;
        }

        public static bool operator <(Nbit a, Nbit b)
        {
            Update(ref a,ref b);
            if (a.IsNeg() && b.IsNeg())
                return a.Value > b.Value;
            if (!a.IsNeg()&&!b.IsNeg())
                return a.Value < b.Value;
            if (a.IsNeg()) return true;
            return false;
        }

        public static bool operator >(Nbit a, Nbit b)
        {
            return !(a < b || a.Value == b.Value);
        }

        public static Nbit operator -(Nbit a, Nbit b)
        {
            return a + -b;
        }
        public static Nbit operator -(Nbit a)
        {
            return new Nbit(a.Size)
            {
                Value = Negate(a.Value, a.Size)
            };
        }
        public static implicit operator int(Nbit a)
        {
            var s = Bit.Bins[a.Size] - 1;
            if (a.IsNeg())
                return - (Negate(a.Value, a.Size) & s);
            return a.Value & s;
        }

        internal static byte SizeOf(int a)
        {
            if ( a < 0 ) return 32;
            byte i = 1;
            for ( ; i < 32; i++)
                if (a < Bit.Bins[i])
                    return  i ;
            return 31;
        }
        public static implicit operator Nbit(int a)
        {
            var n = new Nbit(SizeOf(a));
            if (a < 0)
            {
                n.Value = -a;
                n = -n;
            }
            else
                n.Value = a;
            return n;
        }

        public override string ToString()
        {
            if (IsNeg())
                return "-" + (Negate(Value, Size) & (Bit.Bins[Size] - 1)).ToString(CultureInfo.InvariantCulture);
            return (Value & (Bit.Bins[Size] - 1)).ToString(CultureInfo.InvariantCulture);
        }

        static int Negate(int val, byte size)
        {
            var s = Bit.Bins[size];
            return (s - val) & (s - 1);
        }

        private static int swap(int val, int size)
        {
            var e = Bit.Bins[size];
            return val & e - 1;
        }
        public static bool IsInRange(int val, byte size)
        {
            var e = Bit.Bins[size + 0];
            return (val & e-1) == val || val == e ;
        }

        public static bool TimeOverflow(Nbit a, Nbit b)
        {
            var e = a.Size > b.Size ? a.Size : b.Size;
            var s= Bit.Bins[e];
            var neg = a.IsNeg();
            var negb = b.IsNeg();
            if (neg && !negb) return b._value * (s - a._value) <= s / 2;
            if (neg) return (s - a._value) * (s - b._value) < s / 2;
            return negb ? a._value * (s - b._value) > s / 2 : a._value * b._value >= s / 2;
        }
        public static bool PlusOverflow(Nbit a, Nbit b)
        {
            var s = a.Size > b.Size ? a.Size : b.Size;            
            var neg = a.IsNeg();
            if (b.IsNeg() != neg) return false;
            var e = Bit.Bins[s];
            var vala = a._value;
            var valb = b._value;
            var val= vala + valb;
            return neg ? 2*e - val > e/2 : val >= e/2;
        }
    }
}
