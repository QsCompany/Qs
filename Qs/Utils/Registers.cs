using System;
using System.Collections.Generic;
using Qs.Help;

namespace Qs.Utils
{
    public partial class Registers
    {
        public readonly byte[] Regs;
        public int Draps;
        public Registers() 
        {
            Regs = new byte[128];
            Draps = 0x0;
        }

        public static int Concate(int e_l_h_x, int s_p_f_c, int a_b_c_d)
        {
            return ((e_l_h_x & 3) << 4) + ((s_p_f_c & 3) << 2) + (a_b_c_d & 3);
        }

        public static string GetHashName(int i)
        {

            if (i == 64) return "sc";
            var e_l_h_x = (i >> 4) & 3;
            var s_p_f_c = (i >> 2) & 3;
            var a_b_c_d = i & 3;            
            switch (e_l_h_x)
            {
                case 0:
                    switch (s_p_f_c)
                    {
                        case 0:
                            return String.Concat('e', (char) ('a' + a_b_c_d), 'x');
                        case 1:
                            if (a_b_c_d == 0) return "esi";
                            if (a_b_c_d == 1) return "edi";
                            if (a_b_c_d == 2) return "esp";
                            if (a_b_c_d == 3) return "ebp";
                            break;
                        case 2:
                            return String.Concat((char) (a_b_c_d + 'b'), "s");
                        case 3:
                            if (a_b_c_d == 0) return "ip";
                            if (a_b_c_d == 1) return "dip";
                            if (a_b_c_d == 2) return "sip";
                            if (a_b_c_d == 3) return "dd";
                            break;
                    }
                    break;
                case 1:
                    if (s_p_f_c == 0)
                        return String.Concat((char) ('a' + a_b_c_d), 'l');
                    break;
                case 2:
                    if (s_p_f_c == 0)
                        return String.Concat((char) ('a' + a_b_c_d), 'h');
                    break;
                case 3:
                    if (s_p_f_c == 0)
                        return String.Concat((char) ('a' + a_b_c_d), 'x');
                    if (s_p_f_c == 1)
                        if (a_b_c_d == 0) return "si";
                        else if (a_b_c_d == 1) return "di";
                        else if (a_b_c_d == 2) return "sp";
                        else if (a_b_c_d == 3) return "bp";
                    break;
            }
            return "";
        }
      
        public static int GetHasheCode(string s)
        {
            s = s.ToLower();
            if (s == "ip")
                return Concate(0, 3, 0);
            if (s == "sc")
                return 64;
            if (s == "dip")
                return Concate(0, 3, 1);
            if (s == "sip")
                return Concate(0, 3, 2);
            if (s == "dd")
                return Concate(0, 3, 3);
            if (s.Length >= 3)
                if (s[0] != 'e') return -1;
                else
                {
                    switch (s[2])
                    {
                        case 'x':
                            return Concate(0, 0, s[1] - 'a');
                        case 'i':
                            if (s[1] == 's')
                                return Concate(0, 1, 0);
                            if (s[1] == 'd')
                                return Concate(0, 1, 1);
                            break;
                        case 'p':
                            if (s[1] == 's')
                                return Concate(0, 1, 2);
                            if (s[1] == 'b')
                                return Concate(0, 1, 3);
                            break;
                    }
                }
            else
            {
                if (s.Length < 2) return -1;
                switch (s[1])
                {
                    case 'x':
                        return Concate(3, 0, s[0] - 'a');
                    case 'h':
                        return Concate(2, 0, s[0] - 'a');
                    case 'l':
                        return Concate(1, 0, s[0] - 'a');
                    case 'i':
                        if (s[0] == 's')
                            return Concate(3, 1, 0);
                        if (s[0] == 'd')
                            return Concate(3, 1, 1);
                        break;
                    case 'p':
                        if (s[0] == 's')
                            return Concate(3, 1, 2);
                        if (s[0] == 'b')
                            return Concate(3, 1, 3);
                        break;
                    case 's':
                        return Concate(0, 2, s[0] - 'b');
                }
            }
            return -1;
        }

        public int Get(int reg)
        {
            if (reg == 64) return BitConverter.ToInt32(Regs, reg);
            var e_l_h_x = (reg >> 4) & 3;
            var s_p_f_c = (reg >> 2) & 3;
            var a_b_c_d = reg & 3;
            var offset = 16*s_p_f_c + 4*a_b_c_d;
            if (e_l_h_x == 0) return BitConverter.ToInt32(Regs, offset);
            if (e_l_h_x == 3) return BitConverter.ToInt16(Regs, offset);
            return Regs[offset + 2 - e_l_h_x];
        }

        public void Set(int reg, int value)
        {
            var e_l_h_x = (reg >> 4) & 3;
            var s_p_f_c = (reg >> 2) & 3;
            var a_b_c_d = reg & 3;
            var offset = reg == 64 ? reg : 16 * s_p_f_c + 4 * a_b_c_d;
            if (e_l_h_x == 0)
            {
                var r = BitConverter.GetBytes(value);
                Regs[offset] = r[0];
                Regs[offset + 1] = r[1];
                Regs[offset + 2] = r[2];
                Regs[offset + 3] = r[3];
            }
            else if (e_l_h_x == 3)
            {
                var r = BitConverter.GetBytes((short) value);
                Regs[offset] = r[0];
                Regs[offset + 1] = r[1];
            }
            else
                Regs[offset + 2 - e_l_h_x] = (byte) value;
        }

        public int this[int i]
        {
            get { return Get(i); }
            set { Set(i, value); }
        }

        public int this[string name]
        {
            get { return Get(GetHasheCode(name)); }
            set { Set(GetHasheCode(name), value); }
        }

    }

    public partial class Registers
    {
        private static readonly char[] draps = {'c', '_', 'p', '_', 'a', '_', 'z', 's', 't', 'i', 'd', 'o', '_', '_', '_', '_'};

        public void SetDraps(int i, bool value)
        {
            var v = value ? 1 : 0;
            var s = Bit.Bins[i];
            Draps = Draps & (-1 ^ s) | Bit.Bins[i] * v;
        }
        public void SetDraps(int i, int value)
        {
            var v = value != 0 ? 1 : 0;
            var s = Bit.Bins[i];
            Draps = Draps & (-1 ^ s) | Bit.Bins[i]*v;
        }
        public void SetDraps(string name, bool value)
        {
            SetDraps(IndexOf(draps, name[0]), value);
        }
        public void SetDraps(string name, int value)
        {
            SetDraps(IndexOf(draps, name[0]), value);
        }

        public bool GetDraps(int i)
        {
            return (Draps & Bit.Bins[i]) >> i == 1;
        }

        public bool GetDraps(string s)
        {
            var i= IndexOf(draps, s[0]);
            return (Draps & Bit.Bins[i]) >> i == 1;
        }

        private static int IndexOf<T>(IEnumerable<T> lst, T value)
        {
            var i = 0;
            foreach (var l in lst)
                if (l.Equals(value)) return i;
                else i++;
            return -1;
        }
    }
}