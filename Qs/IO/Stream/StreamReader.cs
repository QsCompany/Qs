using System;
using System.Collections.Generic;
using Qs.Enumerators;
using Qs.Help;

namespace Qs.IO.Stream
{
    public partial class StreamReader
    {
        public StreamWriter Stream;
        private int _ip;
        private readonly List<int> lOffset = new List<int>();
        private readonly List<int> lIP = new List<int>();

        public void Save()
        {
            lOffset.Add(Offset);
            lIP.Add(IP);
        }
        public void Discart()
        {
            var i = lIP.Count - 1;
            lOffset.RemoveAt(i);
            lIP.RemoveAt(i);
        }
        public void Restore()
        {
            var i = lIP.Count - 1;
            Offset = lOffset[i];
            _ip = lIP[i];
            lOffset.RemoveAt(i);
            lIP.RemoveAt(i);
        }


        public int Offset
        {
            get; internal set;
        }

        public bool IsConnected
        {
            get { return !(Offset >= Stream.Content.Length - 4); }
        }

        public int IP
        {
            get
            {
                return _ip;
            }

            set
            {
                Offset += value / 8;
                _ip = value % 8;
            }
        }

        public void CopyTo(out Array array, int start,int length)
        {
            array = new byte[length];
            Array.Copy(Stream.Content, start, array, 0, length);
        }

        public bool Seek(int offset)
        {
            //if (!this.Stream.Compressed)
            //{
            if (offset >= Stream.Capacity) return false;
            Offset = offset;
            return true;
            //}
            //if (offset >= Stream.Capacity*8) return false;
            //IP += offset;
            //return true;
        }


    }
    public partial class StreamReader
    {
        
        public static implicit operator StreamWriter(StreamReader str)
        {
            return str.Stream.Clone();
        }
        public StreamReader(StreamWriter stream)
        {
            Stream = stream;
        }

        public int read(int length_bits)
        {
            var r = read(length_bits, Offset, IP);
            IP += length_bits;
            return r;
        }

        public int read(int length_bits, int offset,int ip=0)
        {
            if (length_bits > (int)AsmDataType.DWord)
                throw new OverflowException("the data cannot be greater than " + (int)DataType.DWord);
            if (offset + (ip + length_bits) / 8 >= Stream.Content.Length - 1)
                throw new OverflowException("stream data has disconnected " + (int)DataType.DWord);
            return Bit.Decode(Stream.Content.get(ip, length_bits, offset));
        }
        public void reset()
        {
            Offset = 0;
            IP = 0;
        }

        public void shift()
        {
            if (_ip == 0) return;
            _ip = 0;
            Offset++;
        }
    }
}