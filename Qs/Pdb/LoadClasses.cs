using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Qs.Enumerators;
using Qs.Parse.Developed;
using Qs.Parse.Expressions;
using Qs.Structures;
using Qs.System;
using Qs.Utils;
using Qs.Utils.Indexation;
using Qs.Utils.Indexation.Find;
using Qs.Utils.Syntax;
using Class = Qs.Structures.Class;

namespace Qs.Pdb
{
    public class LoadClasses
    {
        internal readonly CallManager Caller;
        private readonly Constants Constants;
        internal readonly Optimum Optimum;
        internal readonly List<Class> Prevent = new List<Class>();
        private readonly ByteCodeMapper _byteCodeMapper;
        private readonly BiDictionary<Class, Tree> _summary = new BiDictionary<Class, Tree>();
        private readonly TypeCalc _typeCalc;
        private bool _updateCalled;
        internal VariableGenerator genVars = new VariableGenerator("{", "}");

        public LoadClasses(ByteCodeMapper byteCodeMapper)
        {
            _byteCodeMapper = byteCodeMapper;
            _typeCalc = new TypeCalc(_byteCodeMapper);
            byteCodeMapper.ClassLoader = this;
            Optimum = new Optimum(_byteCodeMapper);
            Constants = new Constants(this);
            Caller = new CallManager(this);
        }

        public BiDictionary<Class, Tree> Summary
        {
            get { return _summary; }
        }

        public ByteCodeMapper ByteCodeMapper
        {
            get { return _byteCodeMapper; }
        }

        private void ProtoSpace(IList<Tree> nameSpace)
        {
            _byteCodeMapper.OpenNameSpace(nameSpace[0].Content);
            for (int i = 1; i < nameSpace.Count; i++)
                switch (nameSpace[i].Kind)
                {
                    case Kind.Class:
                    case Kind.Struct:
                        ProtoClass(nameSpace[i]);
                        break;
                    case Kind.Space:
                        ProtoSpace(nameSpace[i].Children);
                        break;
                }
            _byteCodeMapper.CloseNameSpace();
        }

        public void Add(Tree nameSpaceTree)
        {
            if (_updateCalled)
                throw new InvalidOperationException("Updated was called:");
            if (nameSpaceTree.Kind != Kind.Space)
                throw new BadImageFormatException("Code to added Must be a NameSpace kind");
            ProtoSpace(nameSpaceTree.Children);
        }

        private void ProtoClass(Tree tree)
        {
            List<Tree> children = tree.Children;
            string Base = tree.Kind == Kind.Struct
                ? null
                : children.Count > 1 && children[1].Content == ":" ? children[2].Content : "System.object";
            _byteCodeMapper.OpenClass(Base, children[0].Content);
            if (!_summary.Contain(_byteCodeMapper.CurrentClass))
                _summary.Add(_byteCodeMapper.CurrentClass, children[0].Parent);
            _byteCodeMapper.CloseClass();
        }

        public void Compile()
        {
            _updateCalled = true;
            foreach (var @class in _summary.Where(c => !c.Key.Finalized))
            {
                DownloadClass(@class.Key, @class.Value);
            }
            foreach (var @class in _summary)
            {
                if (!@class.Key.Finalized)
                    LogIn(@class.Key, @class.Value, this, Error.ClassInFinalized);
                _byteCodeMapper.CurrentScop.SetCurrent(@class.Key);
                foreach (
                    var field in
                        @class.Value.Children.Where(
                            field => field.Kind == Kind.Function || field.Kind == Kind.Constructor))
                    field.GeneratedBy.Compile(this, @class.Key, field);
            }
        }


        internal void DownloadClass(Class scop, Tree tree)
        {
            _byteCodeMapper.CurrentScop.SetCurrent(scop);
            if (scop.IsClass)
                if (scop == scop.Base)
                    throw new BadImageFormatException();
                else if (!scop.Base.Finalized)
                {
                    DownloadClass(scop.Base, _summary[scop.Base]);
                    _byteCodeMapper.CurrentScop.SetCurrent(scop);
                }
            scop.Base = scop.Base;
            foreach (Tree field in tree.Children.Where(field => field.Kind == Kind.TypeAssigne))
                SetVariable(scop, field);
            foreach (
                Tree method in
                    tree.Children.Where(method => method.Kind == Kind.Function || method.Kind == Kind.Constructor))
                SetMethodTo(scop, method);
            scop.Finalized = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMethodTo(Class @class, Tree _method)
        {
            int inc = _method.Kind == Kind.Constructor ? 1 : 0;
            string returnType = _method[0].Content;
            Class @return = GetClass(@class, returnType);
            MethodInfo method = _method.Method = new MethodInfo
            {
                Return = @return,
                Parent = @class,
                Name = _method[1 - inc].Content
            };

            @class.Scops.Add(method);
            foreach (Tree child in _method[2 - inc].Children)
            {
                child.Membre = SetVariable(method, child, true);
                child.BaseCall = method;
                child.Type = child.Membre.Return;
                child.Compiled = true;
            }
        }

        public FieldInfo SetVariable(DataPortor scop, Tree tree, bool asParameter = false)
        {
            if (tree.Membre != null && scop.GetField(tree.Membre.Name, SearcheMode.Flaten) == tree.Membre)
                return tree.Membre;
            string type = tree.Children[0].Content;
            string varName = tree.Children[1].Content;
            Class @class = GetClass(scop, type);
            var fieldInfo = new FieldInfo(varName, @class, scop is MethodInfo);
            if (scop is MethodInfo && asParameter)
                fieldInfo = ((MethodInfo) scop).AddParam(fieldInfo);
            else if (scop is MethodInfo || scop is Class)
            {
                fieldInfo = scop.Add(fieldInfo);
            }
            else
                throw new BadImageFormatException();
            return fieldInfo;
        }

        private Class GetClass(DataPortor scop, string type)
        {
            Class @class = _byteCodeMapper.Finder.GetClass(_byteCodeMapper.CurrentScop.Root, type);
            if (@class == null)
            {
                LogIn(scop, null, this, "Class Name :" + type + " doesn't declared");
                return null;
            }
            if (!@class.IsClass && !@class.Finalized && @class.IsFromAssembly(_byteCodeMapper.CurrentScop))
            {
                if (Prevent.Contains(@class))
                    throw new StrongTypingException();
                Prevent.Add((Class) scop);
                DownloadClass(@class, _summary[@class]);
                Prevent.RemoveAt(Prevent.Count - 1);
            }
            return @class;
        }

        public Operand Mov(FieldInfo a, FieldInfo b)
        {
            return null;
        }

        public FieldInfo Add(string func, FieldInfo a, FieldInfo b = null)
        {
            Optimum.Add(func, a.Handle, b != null ? b.Handle : null);
            return null;
        }

        public void Add(string func, Operand a = null, Operand b = null)
        {
            Optimum.Add(func, a, b);
        }

        public FieldInfo Add(string func, Tree a, Operand b)
        {
            return null;
        }

        public FieldInfo Add(string func, Operand a, Tree t)
        {
            return null;
        }

        public FieldInfo Add(string func, Tree a, Tree t)
        {
            return null;
        }

        //public void Add(string func, Operand a = null, Operand t = null)
        //{
        //    Optimum.Add(func, a, t);
        //    new Instruct(func, a, t).Push(_byteCodeMapper.StreamWriter);
        //}

        internal FieldInfo Compile(Scop scop, Tree tree)
        {
            if (tree.Kind == Kind.Hyratachy)
                return scop.GetCField(tree.Content);
            if (tree.Kind == Kind.Variable)
                return tree.Content == "true"
                    ? ConstInfo.True
                    : (tree.Content == "false"
                        ? ConstInfo.False
                        : (tree.Membre ?? scop.GetField(tree.Content, SearcheMode.Deep)));
            if (tree.Kind == Kind.String)
                return tree.Membre = Constants.DeclareConst(tree.Membre == null
                    ? Constants.DeclareString(tree.Content)
                    : (ConstInfo) tree.Membre);

            if (tree.Kind != Kind.Numbre)
                return tree.GeneratedBy != null ? tree.GeneratedBy.Compile(this, scop, tree) : null;

            if (tree.Membre == null)
                tree.Membre = Constants.GetConstNumbre(tree.Content);
            return tree.Membre.Return.SizeOf() <= 4 ? tree.Membre : Constants.DeclareConst((ConstInfo)tree.Membre);
        }

        public void LogIn(Scop scop, Tree tree, object cond, string errorMessage)
        {
        }

        /*
        public FieldInfo Get(Tree t) {
            switch (t.Kind) {
                case Kind.Unair:
                    break;
                case Kind.Numbre:
                    //typeCalc.CalcTypes()
                    //return ConstInfo.Immediate(t.)
                    break;
                case Kind.Variable:
                    break;
                case Kind.String:
                    break;
                case Kind.Expression:
                    break;
                case Kind.Return:
                    break;
                case Kind.Caller:
                    break;
                case Kind.Assigne:
                    break;
                case Kind.Hyratachy:
                    break;
                case Kind.For:
                    break;
                case Kind.If:
                    break;
                case Kind.ElseIf:
                    break;
                case Kind.While:
                    break;
                case Kind.Do:
                    break;
                case Kind.Bloc:
                    break;
                case Kind.Instruction:
                    break;
                case Kind.Parent:
                    break;
                case Kind.Ifs:
                    break;
                case Kind.Param:
                    break;
                case Kind.TypeAssigne:
                    break;
                case Kind.EqAssign:
                    break;
                case Kind.Space:
                    break;
                case Kind.Class:
                    break;
                case Kind.Const:
                    break;
                case Kind.DeclareParams:
                    break;
                case Kind.Function:
                    break;
                case Kind.DeclareParam:
                    break;
                case Kind.KeyWord:
                    break;
                case Kind.Operator:
                    break;
                case Kind.Program:
                    break;
                case Kind.Term:
                    break;
                case Kind.Facteur:
                    break;
                case Kind.Power:
                    break;
                case Kind.Logic:
                    break;
                case Kind.Word:
                    break;
                case Kind.Array:
                    break;
                case Kind.Goto:
                    break;
                case Kind.Label:
                    break;
                case Kind.Register:
                    break;
                case Kind.Constructor:
                    break;
                case Kind.New:
                    break;
                case Kind.Struct:
                    break;
                case Kind.When:
                    break;
                default:
                    throw new ArgumentOutOfRangeException ();
            }
            return null;
        }
        */

        public FieldInfo IsVar(Scop scop, string name)
        {
            DataPortor first = (DataPortor) Finder.GetBackFirst<MethodInfo>(scop) ?? Finder.GetBackFirst<Class>(scop);
            return first != null ? first.GetField(name, SearcheMode.Deep) : null;
        }

        public void Return(Scop scop, FieldInfo @return)
        {
            var met = ((MethodInfo) scop);
            if ((@return == null || @return.Return == Assembly.Void) && met._Return == Assembly.Void) return;
            if (@return.Return == met.Return)
                Optimum.Return(@return);
            else
            {
                LogIn(scop, null, this, "return value incompatible");
                return;
            }
            Optimum.SetGoto("jmp", MethodInfo.ReturnLabel);
        }

        public MethodCallHiretachy GetMethod(Tree val, IList<Tree> Params)
        {
            var @params = new Class[Params.Count];
            for (int j = 0; j < @params.Length; j++)
            {
                if (Params[j].Type == null)
                    if (!_typeCalc.CalcTypes(Params[j]))
                        throw new TypeLoadException();
                @params[j] = Params[j].Type;
            }
            return ByteCodeMapper.CurrentScop.Finder.GetMethod(ByteCodeMapper.Split(val.Content), @params);
        }


        internal FieldInfo GetVariable(string p)
        {
            return _byteCodeMapper.Finder.GetVariable(p);
        }
    }

    public class Error
    {
        public const string ClassInFinalized = "ClassInFinalized";
    }
}