using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Qs.Enumerators;
using Qs.IO.Stream;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;
using Class = Qs.Structures.Class;

namespace Qs.Pdb
{
    public class Constants
    {
        private readonly LoadClasses load;
        //private readonly List<ConstInfo> Consts = new List<ConstInfo>();
        private readonly StreamWriter stream = new StreamWriter(false);
        private readonly Dictionary<ConstInfo, List<int>> Consts = new Dictionary<ConstInfo, List<int>>();
        private Operand ConstOffset = new Operand(Reg.sip, 0, DataType.Byte).AsPointer();

        public Constants(LoadClasses load)
        {
            this.load = load;
        }

        public ConstInfo DeclareConst(Tree tree)
        {
            if (tree.Membre != null) Parse(tree);
            return null;
        }

        public ConstInfo DeclareConst(Class type, string name = null, int value = 0)
        {
            return DeclareConst(new ConstInfo(type, value, name));
        }

        public ConstInfo DeclareConst(Class type, string name = null, byte[] value = null)
        {
            return DeclareConst(new ConstInfo(type, value, name));
        }

        public static ConstInfo GetConstNumbre(string value, string name = null)
        {
            Class type;
            byte[] _bytes;
            switch (value.Length > 2 ? value[value.Length - 2] : Char.MinValue)
            {
                case 'u':
                    switch (value[value.Length - 1])
                    {
                            
                        case 's':
                            value = value.Substring(0, value.Length - 2);
                            ushort us;
                            if (ushort.TryParse(value, out us)) _bytes = BitConverter.GetBytes(us);
                            else return null;
                            type = Assembly.UShort;
                            break;
                        case 'l':
                            value = value.Substring(0, value.Length - 2);
                            ulong ul;
                            if (ulong.TryParse(value, out ul)) _bytes = BitConverter.GetBytes(ul);
                            else return null;
                            type = Assembly.ULong;
                            break;
                        default :
                            throw new Exception("");
                    }
                    break;
                default:
                    float f;
                    double d;
                    switch (value[value.Length - 1])
                    {
                        case 'f':
                            value = value.Substring(0, value.Length - 1);
                            if (!float.TryParse(value, out f))
                                return null;
                            _bytes = BitConverter.GetBytes(f);
                            type = Assembly.Float;
                            break;
                        case 'd':
                            value = value.Substring(0, value.Length - 1);
                            if (!double.TryParse(value, out d))
                                return null;
                            _bytes = BitConverter.GetBytes(d);
                            type = Assembly.Double;
                            break;
                        case 'l':
                            value = value.Substring(0, value.Length - 1);
                            long l;
                            if (!long.TryParse(value, out l))
                                return null;
                            _bytes = BitConverter.GetBytes(l);
                            type = Assembly.Long;
                            break;
                        case 's':
                            value = value.Substring(0, value.Length - 1);
                            short s;
                            if (!short.TryParse(value, out s))
                                return null;
                            _bytes = BitConverter.GetBytes(s);
                            type = Assembly.Short;
                            break;
                        case 'b':
                            value = value.Substring(0, value.Length - 1);
                            byte b;
                            if (!byte.TryParse(value, out b))
                                return null;
                            _bytes = BitConverter.GetBytes(b);
                            type = Assembly.Byte;
                            break;
                        case 'u': 
                            value = value.Substring(0, value.Length - 1);
                            uint u;
                            if (!uint.TryParse(value, out u))
                                return null;
                            _bytes = BitConverter.GetBytes(u);
                            type = Assembly.UInt;
                            break;
                        default:
                            int i;
                            if (int.TryParse(value, out i))
                            {
                                type = Assembly.Int;
                                _bytes = BitConverter.GetBytes(i);
                            }
                            else if (float.TryParse(value, out f))
                            {
                                type = Assembly.Float;
                                _bytes = BitConverter.GetBytes(f);
                            }
                            else if (double.TryParse(value, out d))
                            {
                                _bytes = BitConverter.GetBytes(d);
                                type = Assembly.Double;
                            }
                            else
                                return null;
                            break;
                    }
                    break;
            }
            return new ConstInfo(type, _bytes, name);
        }

        public ConstInfo DeclareString(string value, string name = null)
        {
            return
                DeclareConst(new ConstInfo(Assembly.String, global::System.Text.Encoding.Unicode.GetBytes(value), name));
        }

        public ConstInfo DeclareConst(ConstInfo ci)
        {
            //TODO:ConstOffset.ValueType== Qs.Enumerators.ValueType.Reg_Imm;
            //TODO:ConstOffset.AsMemory=true;
            var length = ci.Value.Length;
            foreach (var @cons in Consts)
            {
                var @const = @cons.Key;
                var Def = false;
                if (@const == ci) return ci;
                if (@const.Value.Length == length)
                    for (var i = length - 1; i >= 0 && !Def; i--)
                        if (ci.Value[i] != @const.Value[i])
                            Def = true;
                if (!Def) return ci;
            }
            Consts.Add(ci, new List<int>());
            ci.Handle = ConstOffset;
            ConstOffset += ci.SizeOf();
            stream.push(ci.Value);
            return ci;
        }
        public IEnumerable<ConstInfo> GetConstAt(string ci)
        {
            return
                (Consts.Where(@const => (string.Compare(@const.Key.Name, ci, StringComparison.Ordinal) == 0))
                    .Select(@const => @const.Key));
        }

        public ConstInfo GetConstAt(ConstInfo ci, int ip)
        {
            List<int> lst;
            if (!Consts.TryGetValue(ci, out lst))
                load.LogIn(load.ByteCodeMapper.CurrentScop.Current, null, this,
                    ci.Name + " Constant Not Implemented In Constant Garbage");
            lst.Add(ip);
            return ci;
        }

        public static MembreInfo Parse(Tree tree)
        {
            return tree.Kind == Kind.Numbre ? ParseNumbre(tree) : (tree.Kind == Kind.String ? ParseString(tree) : null);
        }

        private static MembreInfo ParseNumbre(Tree tree)
        {
            double z;
            if (!double.TryParse(tree.Content, NumberStyles.Any, CultureInfo.InvariantCulture, out z))
                return null;
            z = z > 0 ? z : -z;
            var e = z%1;
            tree.Type = Math.Abs(e) < 1e-9
                ? z > int.MaxValue ? Assembly.Long : Assembly.Int
                : z > float.MaxValue ? Assembly.Double : Assembly.Float;
            return tree.Membre = new ConstInfo(tree.Type, getBytes(z, tree.Type));
        }

        private static unsafe byte[] getBytes(string s)
        {
            if (s == null) return null;
            var charArray = s.ToCharArray();
            var t = new byte[charArray.Length*2];
            for (var i = 0; i < charArray.Length; i++)
            {
                var c = charArray[i];
                var u = (byte*) &c;
                t[2*i] = *u;
                t[2*i + 1] = *(++u);
            }
            return t;
        }

        private static byte[] getBytes(double s, Class type)
        {
            var l = type.SizeOf();
            switch (l)
            {
                case 1:
                    return new[]{(byte) s};
                case 2:
                    return BitConverter.GetBytes((short) s);
                case 3:
                    var bytes = BitConverter.GetBytes((int)s);
                    return new[]{bytes[0], bytes[1], bytes[2]};
                case 4:
                    return BitConverter.GetBytes((int)s);
                case 8:
                    return BitConverter.GetBytes(s);
            }
            return new byte[l];
        }

        private static MembreInfo ParseString(Tree tree)
        {
            var length = tree.End - tree.Start - 3;
            tree.Type =
                (tree.Membre =
                    new ConstInfo(Assembly.String, getBytes(length < 0 ? null : tree.Content.Substring(1, length))))
                    .Return;
            return tree.Membre;
        }
    }
}