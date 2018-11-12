namespace Qs.Enumerators
{
    public enum ValueType : byte
    {
        None = 0,
        Register = 0x01,
        Immediate = 0x2,
        Reg_Imm = 0x3,
    };

    public enum DataType : byte
    {
        Byte = 0x0,
        Word = 0x1,
        DWord = 0x2,
        QWord = 0x3,
    };

    public enum Concate : byte
    {
        Concate = 1,
        NonConcate = 0
    };
    public enum OperandType : byte
    {
        Value = 0,
        Pointer = 1,
    };
    //Optional
    public enum Operation : byte
    {
        Plus = 0,
        Minus = 1,
    }
}
