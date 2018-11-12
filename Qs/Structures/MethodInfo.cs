using System;
using System.Collections.Generic;
using System.Text;
using Qs.IntelligentC.Optimization;
using Qs.Parse.Developed;
using Qs.Parse.Utils;
using Qs.Utils.Indexation;
using Qs.Utils.Syntax;

namespace Qs.Structures
{
    public class MethodInfo : DataPortor
    {
        public override FieldInfo New (Class type, string name) { return GetOne(type, name) ?? base.New(type, name); }

        private readonly List <Instruct> Instructs = new List <Instruct>();
        public readonly List <FieldInfo> Params = new List <FieldInfo>();
        public readonly List<Label<MethodInfo>> Calls = new List<Label<MethodInfo>>();

        internal readonly List<TempFieldInfo> TempVars = new List<TempFieldInfo>();
        internal readonly List<TempFieldInfo> TempVarsCloned = new List<TempFieldInfo>();

        internal bool IsBasic;

        public bool IsConstructor = false;
        public bool IsStatic { get; set; }
        public Class Return { get; set; }

        public int MethodSize;
        public Class _Return;
        public void Add(Instruct instruct) { Instructs.Add(instruct); }

        public void Add(int index, MethodInfo value)
        {
            Calls.Add(new Label <MethodInfo>(index,value));
        }
        public MethodInfo() : base(true) { }
        public FieldInfo AddParam(FieldInfo f)
        {
            if (Finalized) return f;
            foreach (var param in Params)
                if (string.CompareOrdinal(param.Name, f.Name) == 0)
                    if (param.Return == f.Return) return f;
                    else throw new BadImageFormatException();
            
            Params.Add(new FieldInfo(f.Name, f.Return,true)
            {
                Handle = f.Handle,
                IsByRef = f.IsByRef,
                Offset = f.Offset,
                OnLive = f.OnLive,
            });
            return base.Add(f);
        }
        public override bool Equals (string name) { return string.Compare(name, Name, StringComparison.Ordinal) == 0; }

        public void JumpLocation(string jne)
        {
        }

        private readonly Dictionary<string, AsmLabel> Labels = new Dictionary<string, AsmLabel>();

        //public AsmLabel SetLabel(LoadClasses c, string labelName, bool calledFromLabelLocation)
        //{

        //    AsmLabel asmLabel;
        //    if (!Labels.TryGetValue(labelName ?? "", out asmLabel))
        //    {
        //        asmLabel = new AsmLabel(labelName ?? c.genVars.GetNew());
        //        Labels.Add(asmLabel.Name, asmLabel);
        //    }
        //    if (calledFromLabelLocation)
        //    {
        //        asmLabel.Location = c.ByteCodeMapper.StreamWriter.IP;
        //        asmLabel.Freezed = true;
        //    }
        //    return asmLabel;
        //}

        //public AsmLabel SetLabel(LoadClasses c, AsmLabel l)
        //{
        //    l.Location = c.ByteCodeMapper.StreamWriter.IP;
        //    l.Freezed = true;
        //    return l;
        //}

        //public AsmLabel SetGoto(LoadClasses c, string name)
        //{
        //    AsmLabel l;
        //    if (!Labels.TryGetValue(name, out l))
        //        l = SetLabel(c, name, false);
        //    l.Goto.Add(new AsmGoto(c.ByteCodeMapper.StreamWriter.IP, l));
        //    return l;
        //}

        //public AsmLabel SetGoto(LoadClasses c, AsmLabel l)
        //{
        //    l.Goto.Add(new AsmGoto(c.ByteCodeMapper.StreamWriter.IP, l));
        //    return l;
        //}

        private string s;
        public override string ToString()
        {
            if (s != null) return s;
            var sb = new StringBuilder(100);
            sb.Append(Return!=null?Return.FullName:"object?").Append(" ").Append(Name).Append("(");
            var i1 = Params.Count - 1;
            for (var i = 0; i < i1; i++)
                sb.Append(Params[i]).Append(", ");
            if (i1 >= 0)
                sb.Append(Params[i1]);
            return s = sb.Append(")").ToString();
        }

        //public bool AsCPUFunction { get; set; }
        public static readonly AsmLabel ReturnLabel =new AsmLabel("return");

        public bool ISCPUMethod
        {
            get
            {
                if (Params.Count > 2)
                    return false;
                if (!Descripter.CPUFunction.Contain(Name)) return false;
                return Params[0].Return.IsCPUType && Params[1].Return.IsCPUType;
            }
        }
    }
    
}