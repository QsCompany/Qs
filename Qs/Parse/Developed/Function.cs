using System;
using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Syntax;

namespace Qs.Parse.Developed
{
    public class Function : ExtendParse
    {

        public Function(BasicParse basicParse)
            : base(EPNames.Function, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            var instruction = base[EPNames.Instruction];
            var expression = base[EPNames.Expression];
            var DeclaredParams = this[EPNames.DeclaredParams];
            Pile.Save(Kind.Function);
            var T = new Tree(BasicParse.Pile, parent, Kind.Function){GeneratedBy = this};
            return T.Set(BasicParse.GetHeritachy(T) && BasicParse.GetVariable(T)
                         && DeclaredParams.Parse(T) && (instruction.Parse(T)));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {
            MethodInfo method;
            if (!Initialize(load, scop, tree, out method)) return null;
            load.Optimum.Reset(method);
            #region Function Head
            var param = tree[2].Children;
            load.Optimum.Add("mov", Reg.esi, Reg.ebp);
            load.Optimum.Add("pop", Reg.ebp);
            Operand op = new Operand(0xFFFF);
            load.Optimum.Add("add", Reg.esp, op);
            for (var i = param.Count-1; i >= 0; i--)
            {
                var pr = param[i];
                load.Optimum.PopParam(pr.GeneratedBy.Compile(load, method, pr));
            }
            load.Optimum.Add("push", Reg.esi);
            #endregion

            var fieldInfo = tree[3].GeneratedBy.Compile(load, method, tree[3]);
         
            #region Function Feet
            load.Optimum.SetLabel(MethodInfo.ReturnLabel);
            load.Optimum.Add("pop", Reg.ebp);
            load.Optimum.Add("sub", Reg.esp, op);
            load.Optimum.Add("ret", (Operand)null);
            #endregion
            op.Imm = method.DataSize;
            load.ByteCodeMapper.CloseScop();
            load.Optimum.Finalize();
            return fieldInfo;
        }

        private bool Initialize(LoadClasses load, Scop scop, Tree tree, out MethodInfo method)
        {
            if (tree.Method == null)
            {
                load.LogIn(scop, tree, this, "Method Name " + tree[1].Content + " not founded");
                method = null;
                return false;
            }
            method = tree.Method;
            if (method.Return == null)
                method.Return = load.ByteCodeMapper.Finder.GetClass(load.ByteCodeMapper.CurrentScop.Root,
                    tree[0].Content);
            if (method.Return == null || !method.Return.Finalized) throw new BadImageFormatException();
            method.Offset = load.ByteCodeMapper.StreamWriter.Offset;
            load.ByteCodeMapper.OpenScop(method);

            return true;
        }
    }
}