using System;

namespace Qs.Enumerators
{
    [Flags]
    public enum Kind : ulong
    {
        Null = 0x0,
        Unair = 0x1,
        Numbre = 0x2,
        Variable = 0x4,
        String = 0x8,
        Expression = 0x10,
        Return = 0x20,
        Caller = 0x40,
        Assigne = 0x80,
        Hyratachy = 0x100,
        For = 0x200,
        If = 0x400,
        ElseIf = 0x800,
        While = 0x1000,
        Do = 0x2000,
        Bloc = 0x4000,
        Instruction = 0x8000,
        Parent = 0x10000,
        Ifs = 0x20000,
        Param = 0x40000,
        TypeAssigne = 0x80000,
        EqAssign = 0x100000,
        Space = 0x200000,
        Class = 0x400000,
        Const = 0x800000,
        DeclareParams = 0x1000000,
        Function = 0x2000000,
        DeclareParam = 0x4000000,
        KeyWord = 0x8000000,
        Operator = 0x10000000,
        Program = 0x20000000,
        Term = 0x40000000,
        Facteur = 0x80000000,
        Power = 0x100000000,
        Logic = 0x200000000,
        Word = 0x400000000,
        Array = 0x800000000,
        Goto = 0x1000000000,
        Label = 0x2000000000,
        Register = 0x4000000000,
        Constructor = 0x8000000000,
        New = 0x16000000000,
        Struct,
        When,
        SHyratachy
    }
}