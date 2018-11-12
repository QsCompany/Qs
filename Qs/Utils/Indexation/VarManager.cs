using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation.Find;
using Qs.Utils.Syntax;

namespace Compiler.Compiler.Assembly
{
    using System;
    using System.Collections.Generic;

    public class VarManager
    {
        private readonly ByteCodeMapper _this;
        public readonly List<FieldInfo> AuxVariables = new List<FieldInfo>();

        public VarManager (ByteCodeMapper byteCodeMapper)
        {
            _this = byteCodeMapper;
        }

        public void DisactiveVariable(FieldInfo baseField)
        {
            DisactiveVariable(baseField.Name);
        }

        public void DisactiveVariable(string fieldName)
        {
            foreach (var auxVariable in AuxVariables)
                if (auxVariable.Name.Equals(fieldName)) auxVariable.OnLive = false;
        }

        public FieldInfo GetNewVariable(string type, string name = null)
        {
            return GetNewVariable(_this.CurrentScop.Finder.GetClass(type), name);
        }

        public FieldInfo GetNewVariable(Class type, string name = null)
        {
            if (type == null) throw new Exception("UnExpectedType");
            var f = type.FullName;
            foreach (var auxVariable in AuxVariables)
                if (!auxVariable.OnLive && auxVariable.Return.FullName == f)
                {
                    auxVariable.OnLive = true;
                    auxVariable.Name = name ?? auxVariable.Name;
                    return auxVariable;
                }
            var e = SetVariable(type, name ?? "<var" + AuxVariables.Count + ">");
            e.OnLive = true;
            AuxVariables.Add(e);
            return e;
        }

        public FieldInfo New_Get_Variable(Class type, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return GetNewVariable(type);
            return GetVariable(name) ?? SetVariable(type, name);
        }

        public Heritachy<Var> GetVariable(string name)
        {
            return _this.CurrentScop.Finder.GetVariable(ByteCodeMapper.Split(name));
        }

        public Operand GetVariableOffset(Tree val)
        {
            switch (val.Kind)
            {
                case Kind.Numbre:
                    return new Operand(int.Parse(val.Content));
                case Kind.Label:
                    int i;
                    return
                        new Operand(_this.CurrentScop.LabelsInstruction.TryGetValue(val.Content, out i)
                            ? _this.StreamWriter.Offset - i
                            : 0);
                case Kind.Variable:
                case Kind.Hyratachy:
                    var var = GetVariable(val.Content);
                    if (var == null) throw new KeyNotFoundException();
                    return var.main.Handle;
                case Kind.Register:
                    Reg imn;
                    if (Enum.TryParse(val.Content, true, out imn)) return new Operand(imn);
                    break;
            }
            return default(Operand);
        }

        public MethodCallHiretachy GetMethod(Tree val, IList<Tree> Params)
        {
            var @params = new Class[Params.Count];
            for (var j = 0; j < @params.Length; j++)
            {
                //if (Params[j].Type == null) 
                //    if (!TypeCalc.CalcTypes(Params[j])) 
                //        throw new TypeLoadException();
                @params[j] = Params[j].Type;
            }
            return _this.CurrentScop.Finder.GetMethod(ByteCodeMapper.Split(val.Content), @params);
        }

        public FieldInfo SetVariable(Class v, string varName)
        {
            if (v == null) throw new Exception("UnExpectedType");
            var f = new FieldInfo { Return = v, Name = varName };
            //f.SetHandle(this._this.CurrentScop.Current is Method, ((DataPortor)this._this.CurrentScop.Current).DataSize);
            if (!f.IsLocal) if (f.Return.Equals(_this.CurrentClass)) throw new Exception("'" + varName + "': member names cannot be the same as their enclosing type");
            
            _this.CurrentScop.CurrentDataPortor.Add(f);
            return f;
        }

        public FieldInfo SetVariable(string type, string varName)
        {
            var @class = _this.CurrentScop.Finder.GetClass(ByteCodeMapper.Split(type));
            return SetVariable(@class, varName);
        }


    }
}