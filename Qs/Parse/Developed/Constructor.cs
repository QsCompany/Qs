using System;
using Qs.Enumerators;
using Qs.Pdb;
using Qs.Structures;
using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Constructor : ExtendParse
    {
        public Constructor(BasicParse basicParse)
            : base(EPNames.Constructor, basicParse)
        {            
        }

        public override bool Parse(Tree parent)
        {
            var instruction = base[EPNames.Instruction];
            var expression = base[EPNames.Expression];
            var DeclaredParams = this[EPNames.DeclaredParams];
            Pile.Save(Kind.Constructor);
            var T = new Tree(Pile, parent, Kind.Constructor){GeneratedBy = this};
            return
                T.Set(BasicParse.GetVariable(T) && DeclaredParams.Parse(T) &&
                      (instruction.Parse(T)));
        }

        public override FieldInfo Compile(LoadClasses load, Scop scop, Tree tree)
        {

            var method = new MethodInfo{
                Return = load.ByteCodeMapper.Finder.GetClass(load.ByteCodeMapper.CurrentScop.Root, tree[0].Content),
                Parent = scop,
                IsConstructor = true,
            };
            scop.Scops.Add(method);
            if (method.Return == null || !method.Return.Finalized) throw new BadImageFormatException();
            method.Name = tree[0].Content;
            load.ByteCodeMapper.OpenScop(method);
            var param = tree[1].Children;
            foreach (var pr in param)
            {
                pr.GeneratedBy.Compile(load, method, pr);
                method.AddParam((FieldInfo) pr.Membre);
            }
           return tree[2].GeneratedBy.Compile(load, method, tree[2]);
        }
    }
}