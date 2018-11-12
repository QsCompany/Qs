using System.Configuration;
using Qs.Enumerators;
using Qs.Structures;

namespace Qs.Utils.Indexation.Find
{
    public class MethodCallHiretachy : Heritachy<MethodInfo>
    {
        public MethodCallHiretachy(HeritachyType hiHeritachyType) : base(hiHeritachyType) { }
        public MethodCallHiretachy(HeritachyType heritachyType, MethodInfo main = null) : base(heritachyType, main) { }

        public Var GetVar()
        {
            var tmp = HeritachyType;
            var v = tmp.Scop is FieldInfo ? new Var((FieldInfo)tmp.Scop) : new Var(FieldInfo.CreateThis(Assembly.Object));

            while (tmp != null)
            {
                if (tmp.Scop is FieldInfo)
                {
                    v.Push((FieldInfo)tmp.Scop);
                }
                else if (tmp.Genre != Genre.Method) throw new SettingsPropertyWrongTypeException();
                else break;
                tmp = tmp.Children;
            }
            return v;
        }
    }
}