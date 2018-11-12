using System.Collections.Generic;

namespace Qs.System
{
    public class BiDictionary<K, V> : List<KeyValuePair<K, V>>
    {
        public K this[V val, bool isValue = true]
        {
            get { return get(val).Key; }
            set { get(val).Key = value; }
        }

        public V this[K key]
        {
            get { return get(key).Value; }
            set { get(key).Value = value; }
        }

        public int IndexOf(K key)
        {
            for (var i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Key.Equals(key)) return i;
            }
            return -1;
        }

        public int IndexOf(V value, bool isValue = true)
        {
            for (var i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Value.Equals(value)) return i;
            }
            return -1;
        }

        public KeyValuePair<K, V> get(K key)
        {
            var i = IndexOf(key);
            return i == -1 ? null : base[i];
        }

        public KeyValuePair<K, V> get(V value, bool isValue = true)
        {
            var i = IndexOf(value);
            return i == -1 ? null : base[i];
        }

        public void Add(K key, V value) { Add(new KeyValuePair<K, V>(key, value)); }
        public bool Contain(K key) { return get(key) != null; }
        public bool Contain(V value, bool isValue = true) { return get(value) != null; }
       
    }
}
