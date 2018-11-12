using System;
using System.Collections.Generic;
using System.Data;
using Qs.IO.Stream;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Indexation.Find;
using Qs.Utils.Syntax;

namespace Qs.Pdb
{
    public partial class ByteCodeMapper
    {
        public void SetStruct(Class c)
        {
            if (c.IsClass) return;
            foreach (var @struct in Structs)
                if (@struct == c) return;
            Structs.Add(c);
        }
        public void SetDependVarStruct(Class c,FieldInfo fld)
        {
            List<FieldInfo> fields;
            if (!IncompleteVar.TryGetValue(c, out fields))
                IncompleteVar.Add(c, new List<FieldInfo>(){fld});
            else if (!fields.Contains(fld)) fields.Add(fld);
        }

        private bool Contain(Class struct1, Class inStruct2)
        {
            foreach (var @var in inStruct2.Vars) 
                if (@var.Return == struct1) return true;
            return false;
        }

        public List<Class> Redononce()
        {
            var redononce = new List<Class>();
            foreach (var @struct in Structs)
                if (!redononce.Contains(@struct))
                    foreach (var c in Structs)
                        if (!redononce.Contains(c) && Contain(c, @struct) && Contain(@struct, c))
                        {
                            redononce.Add(@struct);
                            redononce.Add(c);
                            break;
                        }
            return redononce;
        }
    }
    public partial class ByteCodeMapper
    {
        public readonly CurrentScop CurrentScop;
        public readonly List<Namespace> Usings = new List<Namespace>();
        private readonly List<Class> Structs = new List<Class>();
        internal readonly Dictionary<Class, List<FieldInfo>> IncompleteVar = new Dictionary<Class, List<FieldInfo>>();
        public readonly StreamReader StreamReader;
        public readonly StreamWriter StreamWriter;

        public readonly Finder Finder;

        public ByteCodeMapper(CurrentScop currentScop)
        {
            CurrentScop = currentScop ?? CurrentScop.Initialize("global");
            StreamWriter = new StreamWriter(false);
            StreamReader = new StreamReader(StreamWriter);
            StreamWriter.push(new byte[32]);
            Finder = new Finder(currentScop);
        }


        public MethodInfo CurrentMethod
        {
            get { return CurrentScop.CurrentMethod; }
        }

        public Namespace CurrentSpace
        {
            get { return Finder.GetBackFirst<Namespace>(CurrentScop._Current); }
        }

        public Class CurrentClass
        {
            get { return CurrentScop.CurrentClass; }
        }
    }
    public partial class ByteCodeMapper
    {
        public FieldInfo SetVariable(DataPortor scop, Tree tree)
        {
            var type = tree.Children[0].Content;
            var varName = tree.Children[1].Content;

            var @class = Finder.GetClass(CurrentScop.Root, type);
            if (!@class.IsClass && !@class.Finalized && @class.IsFromAssembly(CurrentScop))
            {
                if (ClassLoader.Prevent.Contains(@class)) throw new StrongTypingException();
                ClassLoader.Prevent.Add((Class) scop);
                ClassLoader.DownloadClass(@class, ClassLoader.Summary[@class]);
                ClassLoader.Prevent.RemoveAt( ClassLoader.Prevent.Count - 1);
            }
            var fieldInfo = new FieldInfo(varName, @class, scop is MethodInfo);
            if (scop is MethodInfo || scop is Class)
                scop.Add(fieldInfo);
            else throw new BadImageFormatException();
            return fieldInfo;
        }
    }
    public partial class ByteCodeMapper
    {
        public LoadClasses ClassLoader;

        public void OpenNameSpace(string @namespace)
        {
            Scop _new;
            CurrentScop.Add(EScop.Namespace, @namespace, out _new);
            Usings.Add((Namespace) _new);
        }

        public void OpenScop(Scop scop)
        {
            CurrentScop.SetCurrent(scop);
            if (scop is Namespace) Usings.Add((Namespace) scop);
        }

        public void CloseScop()
        {
            if (CurrentScop._Current is Namespace)
                Usings.RemoveAt(Usings.Count - 1);
            CurrentScop.Current = CurrentScop.Current.Parent;
        }

        public void CloseNameSpace()
        {
            Usings.RemoveAt(Usings.LastIndexOf((Namespace) CurrentScop.Out(EScop.Namespace)));
        }

        public void OpenClass(string @base, string className)
        {
            CurrentScop.Add(@base == null ? null : CurrentScop.Finder.GetClass(@base), className);
        }

        public void CloseClass()
        {
            CurrentScop.Out(EScop.Class);
        }
        
        public FieldInfo SetVariable(string type, string varName)
        {
            var @class = Finder.GetClass(type);
            var fieldInfo = new FieldInfo(varName, @class, CurrentScop.Current is MethodInfo);
            if (CurrentScop.Current is MethodInfo)
                CurrentMethod.Add(fieldInfo);
            else if (CurrentScop.Current is Class)
                CurrentClass.Add(fieldInfo);
            else throw new BadImageFormatException();
            return fieldInfo;
        }

        public FieldInfo DownVariable(string type, string varName)
        {
            var @class = Finder.GetClass(type);
            var fieldInfo = new FieldInfo(varName, @class, CurrentScop.Current is MethodInfo);
            if (CurrentScop.Current is MethodInfo)
                CurrentMethod.Add(fieldInfo);
            else if (CurrentScop.Current is Class)
                CurrentClass.Add(fieldInfo);
            else throw new BadImageFormatException();
            return fieldInfo;
        }

        public static string[] Split(string hers)
        {
            return hers.Split(new[]{'.'}, StringSplitOptions.RemoveEmptyEntries);
        }


        public void OpenMethod(string Name, string returnType, IList<Tree> parameters)
        {
            var exist = CurrentScop.Add(EScop.Method, Name);
            CurrentScop.LabelsInstruction.Clear();
            CurrentScop.JumpInstruction.Clear();
            if (exist)
            {
                CurrentMethod.Offset = StreamWriter.Offset;
                CurrentScop.CurrentMethod.Offset = StreamWriter.Offset;
                return;
            }

            #region Ajouter Les Variable Local

            foreach (var tree in parameters)
                CurrentMethod.AddParam(SetVariable(tree.Children[0].Content, tree.Children[1].Content));

            #endregion

            #region Make Prototype

            CurrentScop.CurrentMethod.Return = CurrentScop.Finder.GetClass(returnType);
            CurrentScop.CurrentMethod.IsConstructor = Name == returnType;
            CurrentScop.CurrentMethod.Offset = StreamWriter.Offset;

            #endregion
        }
        public void CloseMethod()
        {
            if (CurrentScop.LabelsInstruction.Count != 0 || CurrentScop.JumpInstruction.Count != 0)
            {
                var loff = StreamWriter.Offset;
                Instruct c;
                foreach (var i in CurrentScop.JumpInstruction)
                {
                    StreamReader.Seek(i.Key);
                    StreamWriter.Seek(i.Key);
                    var add = CurrentScop.LabelsInstruction[i.Value];
                    c = Instruct.Pop(StreamReader);
                    c.Destination.Imm = add - i.Key - c.Length;
                    c.Push(StreamWriter);
                }
                StreamWriter.Seek(CurrentScop.EPSInc);
                StreamReader.Seek(CurrentScop.EPSInc);
                c = Instruct.Pop(StreamReader);
                c.Source.Imm = CurrentScop.CurrentMethod.DataSize;
                c.Push(StreamWriter);
                StreamWriter.Seek(loff);
                CurrentScop.JumpInstruction.Clear();
                CurrentScop.LabelsInstruction.Clear();
            }
            CurrentScop.CurrentMethod.MethodSize = StreamWriter.Offset - CurrentMethod.Offset;
            CurrentMethod.MethodSize = StreamWriter.Offset - CurrentMethod.Offset;
            CurrentScop.Out(EScop.Method);
        }
    }
}