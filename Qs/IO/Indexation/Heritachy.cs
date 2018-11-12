namespace Qs.IO.Indexation
{
    public class Heritachy<T> where T :class 
    {
        public T main;
        public readonly HeritachyType HeritachyType;

        public Heritachy (HeritachyType hiHeritachyType)
        {
            HeritachyType = hiHeritachyType;
            var tmp = hiHeritachyType;
            while ( tmp.Children != null ) tmp = tmp.Children;
            main = (object) tmp.Scop as T;
        }

        public Heritachy (HeritachyType heritachyType, T main = null)
        {
            this.main = main;
            HeritachyType = heritachyType;
        }

        public static implicit operator T (Heritachy <T> _this)
        {
            return _this != null ? _this.main : null;
        }
    }
}