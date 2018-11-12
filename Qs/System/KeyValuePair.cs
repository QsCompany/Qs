namespace Qs.System
{
    public class KeyValuePair<K, V>
    {
        public K Key;
        public V Value;

        public KeyValuePair(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public void Add(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}