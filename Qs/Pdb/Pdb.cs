using System.Collections.Generic;
using Qs.Enumerators;
using Qs.Utils;

namespace Qs.Pdb
{
    public class Pdb
    {
        public readonly ByteCodeMapper _byteCodeMapper;

        public Pdb(ByteCodeMapper byteCodeMapper)
        {
            _byteCodeMapper = byteCodeMapper;
        }

        internal void ProtoSpace(IList<Tree> nameSpace)
        {
            _byteCodeMapper.OpenNameSpace(nameSpace[0].Content);
            for (var i = 1; i < nameSpace.Count; i++)
                if (nameSpace[i].Kind == Kind.Class) ProtoClass(nameSpace[i].Children);
                else if (nameSpace[i].Kind == Kind.Struct) ProtoStruct(nameSpace[i].Children);
                else ProtoSpace(nameSpace[i].Children);
            _byteCodeMapper.CloseNameSpace();
        }

        private void ProtoStruct(List<Tree> @class)
        {
            _byteCodeMapper.OpenClass(null, @class[0].Content);
            foreach (var tree in @class)
                if (tree.Kind == Kind.TypeAssigne) TypeAssign(tree.Children);
                else if (tree.Kind == Kind.Function) ProtoMethod(tree.Children);
                else if (tree.Kind == Kind.Constructor) ProtoConstructor(tree.Children);
            _byteCodeMapper.CloseClass();
            
        }

        private void ProtoClass(IList<Tree> @class)
        {
            var Base = @class.Count > 1 && @class[1].Content == ":" ? @class[2].Content : "System.object";
            _byteCodeMapper.OpenClass(Base, @class[0].Content);
            foreach (var tree in @class)
                if (tree.Kind == Kind.TypeAssigne) TypeAssign(tree.Children);
                else if (tree.Kind == Kind.Function) ProtoMethod(tree.Children);
                else if (tree.Kind == Kind.Constructor) ProtoConstructor(tree.Children);
            _byteCodeMapper.CloseClass();
        }

        private void ProtoConstructor(IList<Tree> constructor)
        {
            _byteCodeMapper.OpenMethod(constructor[0].Content, constructor[0].Content, constructor[1].Children);
            _byteCodeMapper.CloseMethod();
        }
        private void ProtoMethod(List<Tree> method)
        {
            _byteCodeMapper.OpenMethod(method[1].Content, method[0].Content, method[2].Children);
            _byteCodeMapper.CloseMethod();
        }

        internal void TypeAssign(IList<Tree> trees)
        {
            var var = _byteCodeMapper.SetVariable(trees[0].Content, trees[1].Content);
            if (!var.Return.IsClass && var.Return.IsFromAssembly(_byteCodeMapper.CurrentScop))
            {
            }
            if (trees.Count == 2 && _byteCodeMapper.CurrentMethod == null) return;
            var right = trees[2];
            var left = trees[1];
            var tree = new Tree(left.Pile, left.Parent, Kind.EqAssign);
            tree.Children.Add(left);
            tree.Children.Add(right);
            tree.Start = left.Start;
            tree.End = right.End;
            tree.Type = left.Parent.Type;
        }
    }
}
