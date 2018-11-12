namespace Qs.Utils.Indexation
{
    public struct Label<T>
    {
        public int index;
        public T Value;

        public Label (int index, T value)
        {
            this.index = index;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Label<T>) return ((Label<T>)obj).index == index && ((Label<T>)obj).Value .Equals(Value);
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode() & 0xffffeee;
        }
    }
}