using Qs.Help;

namespace Qs
{
    using System;
    using System.Globalization;
    using Qs.Enumerators;
    using IO.Stream;
    using Utils;
    public enum Reg
    {
        eax = 0,
        ebx = 1,
        ecx = 2,
        edx = 3,
        ebp = 4,
        esp = 5,
        esi = 6,
        edi = 7,
        ip = 8,
        dp = 9,
        sp = 10,
        pp = 11,
        dip = 12,
        sip = 13,
        rax = 14,
        Acc = 15// Accumulator
    };
    public enum ValueType
    {
        Register = 0x01,
        Immediate = 0x2,
        Reg_Imm = Register | Immediate,
    };
    public enum ImmediateLength
    {
        Byte = 0,
        Short = 1,
        TShort = 2,
        Int = 3
    };
    public struct Operand
    {
        public DataType DataType;
        public OperandType OperandType;
        public int Value;

        /// <summary>
        ///     This Operant Type Is immediate : reg
        /// </summary>
        public Operand (Regs reg) : this()
        {
            this.OperandType = OperandType.Reg;
            this.Value = (int) reg;
        }

        /// <summary>
        ///     This Operant Type Is Memory : [reg+value]
        /// </summary>
        public Operand (Regs reg, int value)
                                : this()
        {
            this.OperandType = OperandType.Mem;
            this.Value = new Nbit(4, (int) reg) << 28 | new Nbit(28, value).Value;
        }

        public Operand (bool asImmediat_Memory, int value, DataType datatype = DataType.None)
                                : this()
        {
            if ( asImmediat_Memory ) {
                this.OperandType = OperandType.imm;
                this.Value = value;
                this.DataType = datatype == DataType.None ? (DataType) MathHelp.length(value) : this.DataType = datatype;
            }
            else {
                this.OperandType = OperandType.Mem;
                this.Value = new Nbit(4, -1) << 28 | new Nbit(28, value).Value;
            }
        }

        public int Length
        {
            get
            {
                switch ( this.OperandType ) {
                    case OperandType.none:
                        return 2;
                    case OperandType.Reg:
                        return 8;
                    case OperandType.Mem:
                        return 34;
                    case OperandType.imm:
                        switch ( this.DataType ) {
                            case DataType.Hex:
                                return 8;
                            case DataType.Byte:
                                return 12;
                            case DataType.Word:
                                return 20;
                            case DataType.DWord:
                                return 36;
                            default:
                                throw new Exception();
                        }
                }
                return 0;
            }
        }

        public Operand Address
        {
            get
            {
                if ( this.OperandType == OperandType.Mem ) throw new NotImplementedException();
                return this;
            }
        }

        public Operand Memory
        {
            get
            {
                if ( this.OperandType == OperandType.Mem ) return this;
                //if ( this.OperandType == OperandType.Reg ) return AsMem(this.Value)
                if ( this.OperandType == OperandType.Reg ) { }
                return default(Operand);
            }
        }

        public static Operand operator - (Operand a, int b)
        {
            return new Operand {
                                                       DataType = a.DataType, OperandType = a.OperandType, Value = a.Value - b
                               };
        }

        public static Operand operator + (Operand a, int b)
        {
            return new Operand {
                                                       DataType = a.DataType,
                                                       OperandType = a.OperandType,
                                                       Value = a.Value + b
                               };
        }

        public bool Equals (Operand obj)
        {
            if ( this.OperandType == obj.OperandType )
                switch ( this.OperandType ) {
                    case OperandType.none:
                        return true;
                    case OperandType.Reg:
                        return (this.Value & 0x3f) == (obj.Value & 0x3f);
                    case OperandType.Mem:
                        return this.Value == obj.Value;
                    case OperandType.imm:
                        if ( this.DataType == obj.DataType )
                            switch ( this.DataType ) {
                                case DataType.Hex:
                                    return (this.Value & 0xf) == (obj.Value & 0xf);
                                case DataType.Byte:
                                    return (this.Value & 0xff) == (obj.Value & 0xff);
                                case DataType.Word:
                                    return (this.Value & 0xffff) == (obj.Value & 0xffff);
                                case DataType.DWord:
                                    return this.Value == obj.Value;
                            }
                        break;
                }
            return false;
        }

        public void Push (StreamWriter stream)
        {
            stream.push((byte) this.OperandType, (int) AsmDataType.TBits);
            switch ( this.OperandType ) {
                case OperandType.Reg:
                    stream.push((byte) this.Value, (int) AsmDataType.RBits);
                    break;
                case OperandType.Mem:
                    stream.push(Bit.Coder(this.Value), (int) AsmDataType.DWord);
                    break;
                case OperandType.imm:
                    stream.push((byte) this.DataType, (int) AsmDataType.TBits);
                    switch ( this.DataType ) {
                        case DataType.Hex:
                            stream.push((byte) this.Value, (int) AsmDataType.Hex);
                            break;
                        case DataType.Byte:
                            stream.push((byte) this.Value, (int) AsmDataType.Byte);
                            break;
                        case DataType.Word:
                            stream.push(Bit.Coder((Int16) this.Value), (int) AsmDataType.Word);
                            break;
                        case DataType.DWord:
                            stream.push(Bit.Coder(this.Value), (int) AsmDataType.DWord);
                            break;
                    }
                    break;
            }
        }

        public static Operand Pop (StreamReader stream)
        {
            var o = new Operand {OperandType = (OperandType) stream.read((int) AsmDataType.TBits)};
            switch ( o.OperandType ) {
                case OperandType.Reg:
                    o.Value = stream.read((int) AsmDataType.RBits);
                    break;
                case OperandType.Mem:
                    o.Value = stream.read((int) AsmDataType.DWord);
                    break;
                case OperandType.imm:
                    DataType d;
                    o.Value = GetValue(stream, out d);
                    o.DataType = d;
                    break;
            }
            return o;
        }

        private static int GetValue (StreamReader stream, out DataType dataType)
        {
            dataType = (DataType) stream.read((int) AsmDataType.TBits);
            switch ( dataType ) {
                case DataType.Hex:
                    return stream.read((int) AsmDataType.Hex);
                case DataType.Byte:
                    return stream.read((int) AsmDataType.Byte);
                case DataType.Word:
                    return stream.read((int) AsmDataType.Word);
                case DataType.DWord:
                    return stream.read((int) AsmDataType.DWord);
                default:
                    throw new Exception("Bad Error");
            }
        }

        public static Operand Parse (string s)
        {
            var o = new Operand {
                                                        Value = Registers.GetHasheCode(s)
                                };
            var r = 0;
            o.OperandType = Qs.Parse.IsNumbre(s, out r)
                                                    ? OperandType.imm
                                                    : (s[0] == '[' | o.Value == -1 ? OperandType.Mem : OperandType.Reg);
            if ( o.OperandType == OperandType.Mem ) o.Value = Qs.Parse.MemOperandType(s.Substring(1, s.Length - 2));
            else if ( o.OperandType == OperandType.imm ) {
                o.Value = r;
                o.DataType = (DataType) (s.Length > 2 && s[1] == 'x' ? MathHelp.SRound(Math.Log(s.Length - 2, 2)) : MathHelp.length(r));
            }
            return o;
        }

        public override string ToString ()
        {
            switch ( this.OperandType ) {
                case OperandType.Reg:
                    return Registers.GetHashName(this.Value);
                case OperandType.Mem:
                    Nbit a = new Nbit(4, this.Value >> 28), b = new Nbit(28, this.Value);
                    var c = "[";
                    if ( a.Value != 15 ) c += Registers.GetHashName(a.Value) + (b.Value != 0 ? "+" : "");
                    c += a.Value != 15 && b.Value == 0 ? "]" : "0x" + b.Value.ToString("x7") + "]";
                    return c;
                case OperandType.imm:
                    c = "0x";
                    switch ( this.DataType ) {
                        case DataType.Hex:
                            return c + ((byte) (this.Value & 0xf)).ToString("x1", CultureInfo.InvariantCulture);
                        case DataType.Byte:
                            return c + ((byte) this.Value).ToString("x2", CultureInfo.InvariantCulture);
                        case DataType.Word:
                            return c + ((short) this.Value).ToString("x4", CultureInfo.InvariantCulture);
                        case DataType.DWord:
                            return c + this.Value.ToString("x8", CultureInfo.InvariantCulture);
                    }
                    break;
            }
            return "";
        }

        public static Operand AsDValue (int value) { return new Operand(true, value, DataType.DWord); }
        public static Operand AsFValue (int value) { return new Operand(true, value, DataType.Word); }
        public static Operand AsBValue (int value) { return new Operand(true, value, DataType.Byte); }

        public static Operand AsDValue (Regs reg, int value)
        {
            throw new Exception();
            return new Operand(true, value, DataType.DWord);
        }

        public static Operand AsFValue (Regs reg, int value)
        {
            throw new Exception();
            return new Operand(true, value, DataType.Word);
        }

        public static Operand AsMem (int value) { return new Operand(false, value); }

        public static Operand AsMem (Regs reg, int value)
        {
            throw new Exception();
            return new Operand(false, value);
        }
    }
    public static class MathHelp
    {
        public static int SRound (double x)
        {
            var d = x%1;
            return (int) (x + (d == 0 ? 0 : (x > 0 ? 1 : 0) - d));
        }

        public static int LogRound (int x, int n = 2, int i = 0)
        {
            var m = x/n;
            if ( m < 1 ) return i;
            return LogRound(m, n, ++i);
        }

        public static int length (int i)
        {
            var e = LogRound(i, 16);
            return SRound(Math.Log(e, 2));
        }
    }
}
