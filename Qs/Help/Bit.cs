using System;

namespace Qs.Help
{
    public static partial class Bit
    {
        public static byte set(this byte b, byte value, int start, int length)
        {
            var end = 7 - start;
            start = end - length + 1;
            
            var delimerl =  256 - Bins[end + 1] + Bins[start] - 1;
            var delimerr = -1 ^ delimerl;

            var tb = b & delimerl;
            var tb1 = (value << start) & delimerr;

            return (byte)((tb | tb1) & 0xff);
        }

        public static void setByte(this byte[] mem, byte value, int start, int len, int Offset = 0)
        {
            lock (mem)
            {
                var t = start + len - 8;
                if (t <= 0)
                {
                    mem[Offset] = mem[Offset].set(value, start, len);
                    return;
                }
                var l = 8 - start;
                mem[Offset] = mem[Offset].set((byte) (value >> t), start, l);
                mem[Offset + 1] = mem[Offset + 1].set(value, 0, t);
            }
        }

        public static void set(this byte[] mem,byte[] value, int start, int len, int Offset = 0)
        {
            lock (mem)
            {
                var rep = len/8;
                for (var i = 0; i < rep & i < rep; i++)
                    mem.setByte(value[i], start, 8, Offset + i);
                if (len%8 != 0 & value.Length > rep)
                    mem.setByte(value[rep], start, len%8, Offset + rep);
            }
        }
    }

    public static partial class Bit
    {
        public static int get(this byte b, int start, int length)
        {
            var end = 7 - start;
            var _start = end - length + 1;
            var delimer = Bins[end + 1] - Bins[_start];
            var result = (b & delimer) >> _start;
            return result;
        }

        public static byte getByte(this byte[] mem, int start, int len, int Offset = 0)
        {
            if (len > 8) throw new Exception("len must be <=8");
            var t = -8 + start + len;
            if (t <= 0)
                return (byte)mem[Offset].get(start, len);
            var a1 = mem[Offset].get(start, 8 - start) << t;
            var a2 = mem[Offset + 1].get(0, t);
            return (byte)(a1 | a2);
        }

        public static byte[] get(this byte[] mem, int start, int len, int Offset = 0)
        {
            Offset += start / 8;
            start = start % 8;

            var rst = len % 8;
            var qu = len / 8;
            var e = new byte[qu + (rst == 0 ? 0 : 1)];

            for (var i = 0; i < qu; i += 1)
                e[i] = mem.getByte(start, 8, Offset + i);
            if (rst != 0)
                e[qu] = mem.getByte(start, rst, Offset + qu);
            return e;
        }
    }

    public static partial class Bit
    {
        public static int[] Bins =
        {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000,
            0x10000, 0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000,
            0x8000000, 0x10000000, 0x20000000, 0x40000000, -2147483648
        };

        public static byte[] Coder(object num)
        {
            if (num is byte)
                return BitConverter.GetBytes((byte)num);
            if (num is int)
                return BitConverter.GetBytes((int)num);
            if (num is short)
                return BitConverter.GetBytes((short)num);
            if (num is ushort)
                return BitConverter.GetBytes((short)((ushort)num));
            if (num is uint)
                return BitConverter.GetBytes((int)((uint)num));
            throw new Exception("convertion Impossible");
        }

        public static int Decode(byte[] bytes)
        {
            switch (bytes.Length)
            {
                case 1:
                    return bytes[0];
                case 2:
                    return BitConverter.ToInt16(bytes, 0);
                case 4:
                    return BitConverter.ToInt32(bytes, 0);
                default:
                    throw new Exception("Error");
            }
        }
    }
}