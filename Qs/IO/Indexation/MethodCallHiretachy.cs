using System.Configuration;
using Qs.Structures;
using Qs.Utils.Indexation;

namespace Qs.IO.Indexation
{
    public class MethodCallHiretachy:Heritachy <Method>
    {
        public MethodCallHiretachy (HeritachyType hiHeritachyType) : base(hiHeritachyType) {}
        public MethodCallHiretachy (HeritachyType heritachyType, Method main = null) : base(heritachyType, main) {}

        public Var GetVar ()
        {
            var tmp = HeritachyType;
            var v = tmp.Scop is FieldInfo ? new Var((FieldInfo)tmp.Scop) : new Var(FieldInfo.CreateThis(Assembly.Object));

            while ( tmp != null ) {
                if ( tmp.Scop is FieldInfo ) {
                    v.Push((FieldInfo) tmp.Scop);
                }else if ( tmp.Genre != Genre.Method ) throw new SettingsPropertyWrongTypeException();
                else break;
                tmp = tmp.Children;
            }
            return v;
        }
    }
}