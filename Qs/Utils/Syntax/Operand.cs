using System.Text;
using ValueType = Qs.Enumerators.ValueType;
#pragma warning disable 659
using System;
using Qs.IO.Stream;
using Qs.Enumerators;

namespace Qs.Utils.Syntax
{
    public class HeadType
    {
        public ValueType Value = ValueType.None;
        public OperandType Operand = OperandType.Value;
        public Operation Operation = Operation.Plus;
        public DataType Pointer = DataType.DWord;
        public DataType Immediate = DataType.DWord;

        public int Length
        {
            get
            {
                if (Value == ValueType.None)
                    return 2;
                var r = Operand == OperandType.Pointer ? 5 : 3;
                return Value == ValueType.Immediate ? r + 2 : (Value == ValueType.Reg_Imm ? r + 3 : r);
            }
        }

        public HeadType Clone()
        {
            return new HeadType
            {
                Immediate = Immediate,
                Value = Value,
                Operand = Operand,
                Operation = Operation,
                Pointer = Pointer
            };
        }

        public override bool Equals(object obj)
        {
            var c = obj as HeadType;
            if (c == null) return false;
            if (Value != c.Value) return false;
            if (c.Value == ValueType.None) return true;
            if (c.Operand != Operand) return false;
            if (Operand == OperandType.Pointer)
                if (c.Pointer != Pointer)
                    return false;
            switch (Value)
            {
                case ValueType.Immediate:
                    return Immediate == c.Immediate;
                case ValueType.Reg_Imm:
                    return Immediate == c.Immediate && Operation == c.Operation;
            }
            return true;
        }

        internal void push(StreamWriter w)
        {
            w.push((byte)Value, 2);
            if (Value == ValueType.None)                
                return;
            if (Operand == OperandType.Pointer)
                w.push((byte) ((1 << 2) + (int) Pointer), 3);
            else w.push(0, 1);
            switch (Value)
            {
                case ValueType.Immediate:
                    w.push((byte) Immediate, 2);
                    return;
                case ValueType.Reg_Imm:
                    w.push((byte) (((int)Operation << 2) + (int) Immediate), 3);
                return;
            }
        }

        public void pop(StreamReader r)
        {
            Value = (ValueType) r.read(2);
            if (Value == ValueType.None)
                return;
            Operand = (OperandType) r.read(1);
            if (Operand == OperandType.Pointer)
                Pointer = (DataType) r.read(2);
            switch (Value)
            {
                case ValueType.Immediate:
                    Immediate = (DataType) r.read(2);
                    return;
                case ValueType.Reg_Imm:
                    Operation = (Operation) r.read(1);
                    Immediate = (DataType) r.read(2);
                    return;
            }
        }
    }

    public partial class Operand
    {
        public static implicit operator Operand(int i) { return new Operand(i); }
        public static implicit operator Operand(Reg i) { return new Operand(i); }
        public static Operand operator ++(Operand a)
        {
            return a + 1;
        }
        public static Operand operator +(Operand a, int b)
        {
            var ret = a.Clone();
            switch (a.HeadType.Value)
            {
                    case ValueType.None:
                        ret.HeadType.Value = ValueType.Immediate;
                        ret._imm = b;
                    return ret.Optimize();
                case ValueType.Register:
                    ret.HeadType.Value= ValueType.Reg_Imm;
                    ret.Imm = b;
                    return ret.Optimize();
                case ValueType.Immediate:
                case ValueType.Reg_Imm:
                    ret._imm += b;
                    return ret.Optimize();
            }
            return ret;
        }
 
    }
    public partial class Operand
    {
        private static readonly int[] Delimer = {0xFF, 0xFFFF, -1, -1};
        public HeadType HeadType = new HeadType();
        public Reg Reg;
        private int _imm;
        public bool FixedSize = false;
        public override bool Equals(object obj)
        {
            var c = obj as Operand;
            if(c==null)return false;
            if (!HeadType.Equals(c.HeadType)) return false;
            switch (HeadType.Value)
            {
                case ValueType.Register:
                    return c.Reg == Reg;
                case ValueType.Immediate:
                    return c._imm == _imm;
                case ValueType.Reg_Imm:
                    return c.Reg == Reg && c._imm == _imm;
            }
            return true;
        }

        public Operand Clone()
        {
            return new Operand
            {
                HeadType = HeadType.Clone(),
                Reg = Reg,
                _imm = _imm
            };
        }

        public Operand Optimize()
        {
            if (!FixedSize)
                for (int i = 0; i < Delimer.Length; i++)
                {
                    if ((_imm & Delimer[i]) != _imm) continue;
                    _imm = _imm & Delimer[i];
                    HeadType.Immediate = (DataType) i;
                    return this;
                }
            return this;
        }

        public Operand()
        {

        }

        public Operand(Reg reg)
        {
            HeadType.Value = ValueType.Register;
            Reg = reg;
        }

        public Operand(int imm, DataType immType = DataType.DWord)
        {
            if (imm == 12322) { }
            HeadType.Value = ValueType.Immediate;
            HeadType.Immediate = immType;
            _imm = imm;
        }

        public Operand(Reg reg, int imm, DataType immType = DataType.DWord)
        {
            if (imm == 12322) { }
            Reg = reg;
            _imm = imm;
            HeadType.Immediate = immType;
            HeadType.Value = ValueType.Reg_Imm;
        }

        public Operand AsPointer(DataType pointerType = DataType.DWord)
        {
            HeadType.Operand = OperandType.Pointer;
            HeadType.Pointer = pointerType;
            return this;
        }

        public Operand AsValue(DataType immType)
        {
            HeadType.Operand = OperandType.Value;
            HeadType.Immediate = immType;
            return this;
        }

        public Operand AsValue()
        {
            HeadType.Operand = OperandType.Value;
            return this;
        }

        public override string ToString()
        {
            if (HeadType.Value == ValueType.None) return "";
            var s = new StringBuilder();
            if (HeadType.Operand == OperandType.Pointer)
                if (HeadType.Pointer == DataType.Byte)
                    s.Append("ptr byte[");
                else if (HeadType.Pointer == DataType.Word)
                    s.Append("ptr word[");
                else
                    s.Append((HeadType.Pointer == DataType.QWord) ? "ptr qword[" : "ptr dword[");

            switch (HeadType.Value)
            {
                case ValueType.Register:
                    s.Append(Reg);
                    break;
                case ValueType.Immediate:
                    s.Append(_imm);
                    break;
                case ValueType.Reg_Imm:
                    s.Append(Reg).Append(HeadType.Operation == Operation.Plus ? "+" : "-").Append(_imm);
                    break;
            }
            if (HeadType.Operand == OperandType.Pointer) s.Append("]");
            return s.ToString();
        }

      
        public bool IsValide
        {
            get
            {
                var t = new Operand();
                var w = new StreamWriter();
                push(w);
                t.pop(w);
                return Equals(t);
            }
        }

        public int Imm
        {
            get { return _imm; }
            set { _imm = value & Delimer[(int)HeadType.Immediate]; }
        }

        public bool IsNone { get { return HeadType.Value == ValueType.None; } }

        public int Length
        {
            get
            {
                var r = HeadType.Length;
                return r + (HeadType.Value == ValueType.Register
                    ? 4
                    : (HeadType.Value == ValueType.Immediate
                        ? (int) HeadType.Immediate*8
                        : (HeadType.Value == ValueType.Reg_Imm ? (int) HeadType.Immediate*8 + 4 : 0)));
            }
        }

        public Operand AsMemory
        {
            get { return Clone().AsPointer(); }
        }
        public Operand ToBytePtr
        {
            get
            {
                var l = Clone();
                l.HeadType.Pointer = DataType.Byte;
                return l;
            }
        }
        public Operand TowordPtr
        {
            get
            {
                var l = Clone();
                l.HeadType.Pointer = DataType.Word;
                return l;
            }
        }
        public Operand ToDwordPtr
        {
            get
            {
                var l = Clone();
                l.HeadType.Pointer = DataType.DWord;
                return l;
            }
        }
        public Operand ToQwordPtr
        {
            get
            {
                var l = Clone();
                l.HeadType.Pointer = DataType.QWord;
                return l;
            }
        }
        private static void pushNum(StreamWriter w,int num, DataType type)
        {
            var t = BitConverter.GetBytes(num);
            var l = (int) Math.Pow(2, (int) type);
            for (var i = 0; i < l; i++)
                w.push(t[i], 8);
        }

        private static int popNum(StreamReader r, DataType type)
        {
            var t = new byte[4];
            var l = (int) Math.Pow(2, (int) type);
            for (var i = 0; i < l; i++)
                t[i] = (byte) r.read(8);
            return BitConverter.ToInt32(t, 0);
        }

        public void push(StreamWriter w)
        {
            HeadType.push(w);
            switch (HeadType.Value)
            {
                case ValueType.Register:
                    w.push((byte) Reg, 4);
                    break;
                case ValueType.Immediate:
                    pushNum(w, _imm, HeadType.Immediate);
                    break;
                case ValueType.Reg_Imm:
                    w.push((byte) Reg, 4);
                    pushNum(w, _imm, HeadType.Immediate);
                    break;
            }
        }

        public void pop(StreamReader r)
        {
            HeadType.pop(r);
            switch (HeadType.Value)
            {
                case ValueType.Register:
                    Reg = (Reg) r.read(4);
                    break;
                case ValueType.Immediate:
                    _imm = popNum(r, HeadType.Immediate);
                    break;
                case ValueType.Reg_Imm:
                    Reg = (Reg) r.read(4);
                    _imm = popNum(r, HeadType.Immediate);
                    break;
            }
        }

        public static Operand Pop(StreamReader r)
        {
            var e = new Operand();
            e.pop(r);
            return e;
        }
    }
}
