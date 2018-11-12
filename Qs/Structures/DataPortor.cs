using System.Collections.Generic;

namespace Qs.Structures
{
    public abstract class DataPortor : Scop
    {
        public int Offset;
        public readonly List<FieldInfo> Vars = new List<FieldInfo>();
        protected bool _finalized;
        protected int _dataSize;

        public DataPortor(bool flip)
        {
            this.flip = flip;
        }
        public virtual bool Finalized
        {
            get { return _finalized; }
            set
            {
                if (_finalized) return;
                _finalized = value;
            }
        }
        protected readonly bool flip = false;
        public FieldInfo Add(FieldInfo var)
        {
            if (Finalized) return var;
            foreach (var param in Vars)
                if (string.CompareOrdinal(param.Name, var.Name) == 0)
                    if (param.Return == var.Return) return param;
                    else
                    {
                        param.Return = var.Return;
                        param.IsByRef = var.IsByRef;
                    }

            Vars.Add(var);
            if (flip)
                SetDataSize(_dataSize + var.SizeOf());
            var.SetHandle(this is MethodInfo, DataSize);
            if (!flip)
                SetDataSize(_dataSize + var.SizeOf());
            return var;
        }

        protected virtual FieldInfo GetOne(Class type, string name)
        {
            foreach (var var in Vars)
            {
                if ( var.OnLive || !Equals(var.Return, type) ) continue;
                var.Name = name;
                return var;
            }
            return null;
        }
        public virtual FieldInfo New(Class type, string name)
        {
            if (Finalized) return null;
            var var = new FieldInfo(name, type, this is MethodInfo);
            Add(var);
            return var;
        }

        public int DataSize
        {
            get { return _dataSize; }
        }

        public void SetDataSize(int size)
        {
            if (Finalized) return;
            _dataSize = size;
        }
    }
}