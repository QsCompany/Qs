using System;
using System.Collections.Generic;
using System.Linq;
using Qs.Help;
using Qs.IO.Stream;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;
using System.Text;

namespace Qs.Parse.Developed
{
    public class CS<T>
    {
        private T value;
        public static implicit operator T(CS<T> t )
        {
            return t.value;
        }
        public static implicit operator CS<T>(T t)
        {
            return new CS<T> {value = t};
        }

        private CS()
        {
            
        }
    }
    public class inst
    {
        public string fn;
        public readonly Operand destination;
        public readonly Operand source;

        public inst(string fn, Operand destination=null, Operand source=null)
        {
            this.fn = fn;
            this.destination = destination;
            this.source = source;
        }

        public override string ToString()
        {
            return fn + ( destination != null ? " " + destination + ( source != null ? ", " + source : "" ) : "" );
        }
    }
    public partial class Optimum
    {
        
        private readonly List<Instruct> _insts = new List<Instruct>(100);
        //public readonly Add Add;
        private readonly ByteCodeMapper _byteCodeMapper;
        private MethodInfo Method;
        private readonly List<CMethod> _calls = new List<CMethod>(20);

        void _add(string function, Operand destination = null, Operand source = null)
        {
            var item = new Instruct(function, destination, source);
            _insts.Add(item);
            length += item.Length;
        }
        public static bool ShowDism = true;
        public override string ToString()
        {
            if (ShowDism) return Dism;
            return base.ToString();
        }

        public string Dism
        {
            get
            {
                var s = new StringBuilder();
                foreach (var ins in _insts)
                    s.AppendLine(ins.ToString());
                return s.ToString();
            }
        }
        public void Add(string function, Operand destination = null, Operand source = null)
        {
            var item = new Instruct(function, destination, source);
            _insts.Add(item);
            length += item.Length;
        }
        public void Add(string function, FieldInfo destination = null, FieldInfo source = null)
        {            
            var item = new ProgInstruct(function, destination, source);
            _insts.Add(item);
            length += item.Length;
        }
        public void Reset(MethodInfo method)
        {
            _insts.Clear();
            Method = method;
        }
        public Optimum(ByteCodeMapper byteCodeMapper)
        {
            _byteCodeMapper = byteCodeMapper;
            //Add = _add;
        }

        public void PopParam(FieldInfo param)
        {
            if (param.IsByRef)
            {
                PopByRef(param);
                return;
            }
            if (param.Return.IsClass)
            {
                Add("pop", param.Handle.AsMemory);
                return;
            }
            if (param.SizeOf() <= 4)
            {
                Add("pop", param.Handle.AsMemory);
                return;
            }
            Add("pop", RegInfo.eax.Handle);
            Add("mov", RegInfo.ecx.Handle, new Operand(param.SizeOf()));
            Add("lea", param.Handle, RegInfo.eax.Handle);
        }

        private void PopByRef(FieldInfo param)
        {
            Add(Const.pop, RegInfo.eax.Handle);
            Add(Const.mov, new Operand(param.Handle.Imm - 4).AsMemory, RegInfo.eax.Handle);
            if (param.SizeOf() <= 4)
            {
                Add(Const.mov, param.Handle.AsMemory, RegInfo.eax.Handle);
                return;
            }
            Add("mov", RegInfo.ecx.Handle, param.SizeOf());
            Add("lea", param.Handle, RegInfo.eax.Handle);

        }

        public void PushParam(FieldInfo p)
        {
            if (!p.IsByRef)
            {
                if (p.SizeOf() <= 4)
                {
                    if (p is ConstInfo)
                        Add("push", p.Handle);
                    else
                        Add("push", p.Handle.AsMemory);
                    return;
                }
            }
            Add("push", p.Handle);
        }

        public void UpdateRefParam(FieldInfo p)
        {
            Add("mov", RegInfo.ebx.Handle, new Operand(p.Handle.Imm - 4).AsPointer());
            if (p.SizeOf() <= 4)
            {
                Add("mov", RegInfo.ebx.Handle.AsMemory, p.Handle.AsMemory);
                return;
            }
            Add("mov", RegInfo.eax.Handle, p.SizeOf());
            Add("lea", RegInfo.ebx.Handle, p.Handle);
        }

        public void Return(FieldInfo p)
        {
            Add("mov", RegInfo.eax.Handle, p.SizeOf() <= 4 ? p.Handle.AsMemory : p.Handle);
        }

        public void GetReturn(FieldInfo p)
        {
            if (p != null)
                if (p.SizeOf() > 4)
                {
                    Add("mov", RegInfo.ecx.Handle, p.SizeOf());
                    Add("lea", p.Handle, RegInfo.eax.Handle);
                }
                else Add("mov", p.Handle.AsMemory, RegInfo.eax.Handle);
        }

        public void Assign(FieldInfo l, FieldInfo r)
        {
            if (l.SizeOf() != r.SizeOf()) throw new Exception();
            if (l.SizeOf() <= 4) Add("mov", l, r);
            else
            {
                //TODO : Rectifier ce code pour structures et class (50% just)
                Add("mov", RegInfo.ecx.Handle, l.SizeOf());
                Add("lea", l.Handle.AsValue(), r.Handle.AsValue());
            }
        }

        internal void Call(MethodInfo method,CS<int> instLocation=null)
        {
            var i = instLocation == null ? _byteCodeMapper.StreamWriter.Offset:(int) instLocation;
            if (method.Offset == 0 && !(method is CPUMethodInfo)) _calls.Add(new CMethod(i, method));
            _add("call", method.Offset != 0 ? new Operand(method.Offset - i) : new Operand(-1));
        }
        public List<Instruct> Instructs
        {
            get { return _insts; }
        }

    }
    internal class CMethod
    {
        public readonly int location;
        public readonly MethodInfo Method;

        public CMethod(int location, MethodInfo method)
        {
            this.location = location;
            Method = method;
        }
    }
    public partial class Optimum
    {
        private readonly Dictionary<string, AsmLabel> _labels = new Dictionary<string, AsmLabel>();
        private readonly VariableGenerator _genVars = new VariableGenerator("{", "}");
        private readonly Dictionary<int, AsmGoto> gotos = new Dictionary<int, AsmGoto>();
        private readonly Dictionary<CS<int>, AsmLabel> labels = new Dictionary<CS<int>, AsmLabel>();
        public AsmLabel SetLabel(string labelName, bool calledFromLabelLocation)
        {
            AsmLabel asmLabel;
            if(!string.IsNullOrEmpty(labelName))
                if (_labels.TryGetValue(labelName, out asmLabel))
                    return calledFromLabelLocation ? SetLabel(asmLabel) : asmLabel;
            asmLabel = new AsmLabel(labelName ?? _genVars.GetNew());
            _labels.Add(asmLabel.Name, asmLabel);
            return calledFromLabelLocation ? SetLabel(asmLabel) : asmLabel;
        }

        public AsmLabel SetLabel(AsmLabel l)
        {
            labels.Add(_insts.Count, l);
            if (!_labels.ContainsValue(l)) _labels.Add(l.Name, l);
            return l;
        }

        public AsmLabel SetGoto(string func,string label)
        {
            return SetGoto(func, SetLabel(label, false));
        }

        public AsmLabel SetGoto(string func, AsmLabel tolabel, LoadClasses load = null, Operand methodPtr = null)
        {
            var v = new AsmGoto(_insts.Count, tolabel);
            _add(func, new Operand(0xffff));
            tolabel.Goto.Add(v);
            gotos.Add(v.GotoLocation, v);
            if (load != null)
                load.Add(func, methodPtr);
            return tolabel;
        }

        private int length;

        public virtual void Finalize()
        {
            var gotoEnum = gotos.GetEnumerator();
            var gotoStat = gotoEnum.MoveNext();
            var labelEnum = labels.GetEnumerator();
            var labelStat = labelEnum.MoveNext();
            var s = _byteCodeMapper.StreamWriter;
            Method.Offset = s.Offset;
            for (var i = 0; i < _insts.Count; i++)
            {
                var inst = _insts[i];
                debLabel:
                if (labelStat && i == labelEnum.Current.Key)
                {
                    labelEnum.Current.Value.ReSet(s.Offset);
                    labelEnum.Current.Value.Freezed = true;
                    labelStat = labelEnum.MoveNext();
                    goto debLabel;
                }
                debGoto:
                if (gotoStat && i == gotoEnum.Current.Key)
                {
                    finalizeGoto(gotoEnum.Current.Value, inst);
                    gotoStat = gotoEnum.MoveNext();
                    goto debGoto;
                }
                inst.Push(s);
            }
            foreach (var asmGoto in gotos.Where(r => !r.Value.Freezed).Select(r => r.Value))
            {
                var inst = Instruct.Pop(s, asmGoto.GotoLocation);
                finalizeGoto(asmGoto, inst, asmGoto.GotoLocation);
                if (!asmGoto.Freezed)
                    throw new Exception();
                inst.Push(s, asmGoto.GotoLocation);
            }
            methodFin = s.Offset;
            Test(s);
        }

        private void Test(StreamReader sr)
        {
            sr.Seek(Method.Offset);
            var _i = 0;
            var g = new List<Instruct>();
            while (sr.Offset < methodFin)
            {
                g.Add(_insts[_i]);
                _insts[_i] = Instruct.Pop(sr);                
                _i++;
            }
        }

        private int methodFin;
        private void finalizeGoto(AsmGoto asmGoto, Instruct inst,CS<int> instlocation=null )
        {
            var e = inst.Destination.Clone();
            var o = inst.Length;
            var i = instlocation == null ? _byteCodeMapper.StreamWriter.Offset : (int) instlocation;
            inst.Destination.Imm = asmGoto.Label.Freezed ? asmGoto.Label.Location - i : length;
            
            if (o != inst.Length && instlocation!=null)
            {
                inst.Destination.HeadType.Immediate = e.HeadType.Immediate;
            }
            asmGoto.GotoLocation = i;
            asmGoto.Freezed = asmGoto.Label.Freezed;
        }
    }

    internal class Call
    {

        //public FieldInfo _Call(MethodInfo method);
        public FieldInfo _Call(MethodInfo method, FieldInfo l, FieldInfo r)
        {
            return method.ISCPUMethod ? CallCPUMethod(method, l, r) : CallNonCPUMethod(method, l, r);
        }

        private FieldInfo CallCPUMethod(MethodInfo method, FieldInfo l, FieldInfo r)
        {
            throw new NotImplementedException();
        }

        private FieldInfo CallNonCPUMethod(MethodInfo method, FieldInfo l, FieldInfo r)
        {
            return null;
        }
    }
}