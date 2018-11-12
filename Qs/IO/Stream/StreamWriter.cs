using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security;
using Qs.Help;

namespace Qs.IO.Stream
{
    public class Stream
    {
        public readonly int ShiftCapacity;
        public readonly bool Compressed;
        public volatile byte[] Content;
        protected int _capacity;
        protected Stream(int shiftCapacity, bool compressed,byte[] content=null)
        {
            ShiftCapacity = shiftCapacity;
            Compressed = compressed;
            Content = content;
        }
    }
    public class StreamWriter:Stream
    {
        private int _offset;
        private int _ip;
        
        private readonly List<int> lOffset = new List<int>();
        private readonly List<int> lIP = new List<int>();

        public StreamWriter Clone(){
            return new StreamWriter(Compressed,ShiftCapacity,0){_capacity=_capacity,Content=Content};
        }

        public static implicit operator StreamReader(StreamWriter str)
        {
            return new StreamReader(str);
        }
        

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
        public int Capacity
        {
            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private set
            {
                if (value <= _capacity) return;
                
                var p = new byte[value];
                lock (p)
                {
                    lock (Content)
                    {
                        Array.Copy(Content, 0, p, 0, _capacity);
                        Content = p;
                        _capacity = value;
                    }
                }
            }
            [SecurityCritical]
            get { return _capacity; }
        }

        public int Offset
        {
            [SecurityCritical]
            get { return _offset; }
            [SecurityCritical]
            internal set
            {
                if (value >= _capacity - 100) Capacity += ShiftCapacity;
                _offset = value;
            }
        }
        public int IP
        {
            [SecurityCritical]
            get { return _ip; }
            [SecurityCritical]
            internal set
            {
                Offset += value/8;
                _ip = value%8;
            }
        }


        public void shift()
        {
            if (IP == 0) return;
            IP = 0;
            Offset++;
        }
        
        public void Reset(bool deepClean = false)
        {
            if (deepClean)
                for (var i = 0; i <= Offset; i++)
                    Content[i] = 0;
            IP = 0;
            Offset = 0;
        }
        
        public void push(byte[] bytes, int length_Bits)
        {
            Content.set(bytes, IP, length_Bits, Offset);
            IP += length_Bits;
        }
        public void push(byte[] bytes)
        {
            var length_Bits = bytes.Length*8;
            Content.set(bytes, IP, length_Bits, Offset);
            IP += length_Bits;
        }
        public void push(byte[] bytes, int length_Bits, int offset, int ip = 0)
        {
            Content.set(bytes, ip, length_Bits, offset);
        }
        
        public void push(byte value, int length)
        {
            Content.setByte(value, IP, length, Offset);
            IP += length;
        }

        public StreamWriter(ref byte[] array,int shiftCapacity,bool compressed):base(shiftCapacity,compressed,array)
        {

        }

        public StreamWriter(bool compressed = true, int shiftCapacity = 1024, int initialSize = 1024)
            : base(shiftCapacity, compressed)
        {
            _offset = 0;
            _ip = 0;
            Content = new byte[initialSize];
            _capacity = initialSize;
        }

        public StreamWriter(global::System.IO.Stream stream, int shiftCapacity, bool compressed)
            : base(shiftCapacity, compressed)
        {
            Content = new byte[stream.Length];
            stream.Read(Content, 0, (int) stream.Length);
        }

        public void Save(global::System.IO.Stream stream, bool append, bool allBits)
        {
            if (!append)
                stream.Flush();
            stream.Write(Content, 0, allBits ? _capacity : _offset);
        }

        public void Load(global::System.IO.Stream stream, bool append)
        {
            var e = new byte[stream.Length];
            stream.Read(e, 0, e.Length);
            if (!append)
                Reset(true);
            Capacity += e.Length - _offset + 100;
            push(e, e.Length*8);
        }

        public void CopyTo(out byte[] array)
        {
            array = new byte[Offset];
            Array.Copy(Content, array, Offset);
        }

        public bool Seek(int offset)
        {
            if (offset >= Capacity) return false;
            Offset = offset;
            return true;
        }

        public void Reserve(int length)
        {
            push(new byte[length / 8 + (length % 8 == 0 ? 0 : 1)], length);
        }
    }
}
