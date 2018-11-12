//using System;

using System;
using Qs.Enumerators;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;
using ValueType = Qs.Enumerators.ValueType;

namespace Qs.Structures
{
    public class BaseFieldInfo : MembreInfo
    {
        
    }

    //public class TempFieldInfo : FieldInfo
    //{
    //    public bool
    //}
    public class ConstInfo : FieldInfo
    {
        public readonly static ConstInfo True = new ConstInfo(Assembly.Bool, new byte[] {0xFF});
        public readonly static ConstInfo False = new ConstInfo(Assembly.Bool, new byte[1] );
        public byte[] Value { get; set; }
        public override FieldInfo Clone()
        {
            var clone = (ConstInfo)base.Clone();
            clone.Value = Value;
            return clone;
        }

        internal ConstInfo(Class type, byte[] value, string name = null)
        {
            Return = type;
            Value = value;
            Name = name;
            
            if (type.SizeOf() <= 4)
            {
                var val = 0;
                if (type == Assembly.UInt)
                    val = (int) BitConverter.ToUInt32(value, 0);
                else if (type == Assembly.Int)
                    val = BitConverter.ToInt32(value, 0);
                else if (type == Assembly.Bool)
                    val = BitConverter.ToBoolean(value, 0) == false ? 0 : 0xff;
                else if (type == Assembly.Byte)
                    val = value[0];
                else if (type == Assembly.Char)
                    val = BitConverter.ToChar(value, 0);
                else if (type == Assembly.Float)
                    val = Float2Int(BitConverter.ToSingle(value, 0));
                else if (type == Assembly.Short)
                    val = BitConverter.ToInt16(value, 0);
                else
                    switch (value.Length)
                    {
                        case 4:
                            val = BitConverter.ToInt32(value, 0);
                            break;
                        case 2:
                            val = BitConverter.ToInt16(value, 0);
                            break;
                        case 1:
                            val = value[0];
                            break;
                        case 3:
                            val = BitConverter.ToInt32(new byte[] {0, value[0], value[1], value[2]}, 0);
                            break;
                    }
                Handle = new Operand(val);
            }
            else
                Handle = new Operand(Reg.sip, 0).AsPointer();
        }

        internal ConstInfo(Class type, int value, string name = null)
        {
            Return = type;
            Handle = new Operand(value);
            Name = name;

            var sizeOf = type.SizeOf ();
            if (sizeOf <= 4) {
                if (type == Assembly.UInt)
                    Value = BitConverter.GetBytes ( (uint) value );
                else if (type == Assembly.Int)
                    Value = BitConverter.GetBytes ( value );
                else if (type == Assembly.Bool)
                    Value = new[] { (byte) ( value == 0 ? 0 : 0xff ) };
                else if (type == Assembly.Byte)
                    Value = BitConverter.GetBytes ( (byte) value );
                else if (type == Assembly.Char)
                    Value = BitConverter.GetBytes ( (char) value );
                else if (type == Assembly.Float)
                    Value = BitConverter.GetBytes ( (float) value );
                else if (type == Assembly.Short)
                    Value = BitConverter.GetBytes ( (short) value );
                else
                    switch (sizeOf) {
                        case 0:
                            Value = new byte[0];
                            break;
                        case 4:
                            Value = BitConverter.GetBytes ( (uint) value );
                            break;
                        case 2:
                            Value = BitConverter.GetBytes ( (short) value );
                            break;
                        case 1:
                            Value = BitConverter.GetBytes ( (byte) value );
                            break;
                        default:
                            Value = BitConverter.GetBytes ( value );
                            Value = new[] { Value[0], Value[01], Value[2] };
                            break;
                    }
                Handle = new Operand ( value );
            } else
                Handle = new Operand(Reg.sip, 0).AsPointer();
            
        }
        public override string ToString()
        {
            return string.Format("({0}) {1}", Return,this.Handle);
        }

    }
    public static class RegInfo
    {
        public readonly static FieldInfo eax;
        public readonly static FieldInfo edx;
        public readonly static FieldInfo ecx;
        public readonly static FieldInfo ebx;
        public readonly static FieldInfo ebp;
        public readonly static FieldInfo esi;
        public static readonly FieldInfo esp;
        public static readonly FieldInfo ip;

        static RegInfo()
        {
            eax = new FieldInfo(Reg.eax);
            ebx = new FieldInfo(Reg.ebx);
            ecx = new FieldInfo(Reg.ecx);
            edx = new FieldInfo(Reg.edx);

            ebp = new FieldInfo(Reg.ebp);
            esi = new FieldInfo(Reg.esi);
            esp = new FieldInfo(Reg.esp);

            ip = new FieldInfo(Reg.ip);
        }
    }
    
    public class FieldInfo:BaseFieldInfo
    {
        private Operand _handle = new Operand(Reg.ebp, 0).AsPointer();
        public Operand Handle
        {
            get { return _handle; }
            set
            {
                _handle = value;
            }
        }
        private bool xisLocal;
        public bool IsLocal
        {
            get
            {
                return xisLocal;
            }
            private set
            {
                _handle.Reg = value ? Reg.esp : Reg.ebp;
                xisLocal = value;
                if (value)
                    _handle.HeadType.Operation = Operation.Minus;
            }
            //protected set
            //{
            //    if (isLocal && !value)
            //    {
            //    } 
            //    isLocal = value;
            //}
        }

        public bool IsRegister
        {
            get { return Handle.HeadType.Value == ValueType.Register; }
        }

        public bool OnLive = true;
        public bool IsByRef;
        public override int Offset
        {
            get { return _offset; }
            set
            {
                Handle.Imm = value;
                _offset = value;
            }
        }

        public virtual FieldInfo Clone()
        {
            return new FieldInfo
            {
                Name = Name,
                _handle = _handle,
                IsByRef = IsByRef,
                _offset = _offset,
                OnLive = OnLive,                
                Return = Return,
                IsLocal = IsLocal,
            };
        }
        public FieldInfo ()
        {
        }
        public static FieldInfo CreateBase(Class @class)
        {
            var e = new FieldInfo{Name = "base", Return = @class.Base};
            e.SetHandle(false, 0);
            return e;
        }
        public static FieldInfo CreateThis (Class type)
        {
            var e = new FieldInfo {Name = "this", Return = type};
            e.SetHandle(false, 0);
            return e;
        }

        public FieldInfo(Reg reg)
        {
            Name = reg.ToString();
            Handle = new Operand(reg);
            Return = Assembly.Int;
        }
        public FieldInfo(string name, Class type,bool isLocal)
        {
            if (name == "this" || name == "base") SetHandle(false, 0);
            Name = name;
            Return = type;
            IsLocal = isLocal;
        }
        public override string ToString()
        {
            return Return + " " + Name;
        }
        public void SetHandle (bool isLocal, int offset)
        {
            if (isLocal != this.xisLocal)
                this.IsLocal = isLocal;
            Offset = offset;
        }

        public int SizeOf ()
        {
            return Return == null ? 0 : Return.fSizeOf();
        }

        private static readonly FieldInfo _immediate = new FieldInfo("const", Assembly.Int,true){Handle = new Operand(0)};
        public static FieldInfo Immediate(int i)
        {
            _immediate.Handle.Imm = i;
            return _immediate.Clone();
        }
        public static FieldInfo Immediate(float i)
        {
            _immediate.Handle.Imm =Float2Int(i);
            return _immediate.Clone();
        }

        public static unsafe int Float2Int(float i)
        {
            void* k = &i;
            return *((int*) &i);
        }

        public static unsafe float Int2Float(int i)
        {
            void* k = &i;
            return *((float*)&i);
        }

        public static FieldInfo Immediate(char i)
        {
            _immediate.Handle.Imm = i;
            return _immediate;
        }
        public static FieldInfo Immediate(short i)
        {
            _immediate.Handle.Imm = i;
            return _immediate;
        }
        public static FieldInfo Immediate(byte i)
        {
            _immediate.Handle.Imm = i;
            return _immediate;
        }

        public virtual Operand GetHandle(int i)
        {
            if (Handle.HeadType.Operand == OperandType.Pointer)
                return Handle + 4;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}