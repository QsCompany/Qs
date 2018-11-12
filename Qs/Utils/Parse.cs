using System;
using System.Globalization;
using Qs.Help;

namespace Qs.Utils
{
    public static class Parse
    {
        public static int MemOperandType (string s)
        {
            var r = 0;
            Nbit a = new Nbit(4, -1), b = new Nbit(28, 0);
            var d = s.Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < d.Length; i++)
                if ( IsNumbre(d[i], out r) ) b.Value = r;
                else a.Value = Registers.GetHasheCode(d[i]);
            return a.Value << 28 | b.Value;
        }

        public static int MemOperandType (int reg, int offset)
        {
            Nbit a = new Nbit(4, reg), b = new Nbit(28, offset);
            return a.Value << 28 | b.Value;
        }

        public static bool IsNumbre (string s, out int i)
        {
            if ( s.StartsWith("0x") ) return int.TryParse(s.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out i);
            return int.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out i);
        }
    }
}