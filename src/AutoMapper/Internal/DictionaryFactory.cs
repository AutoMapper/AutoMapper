using System;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class DictionaryFactory : IDictionaryFactory
    {
        public IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
        {
            return new DictionaryAdapter<TKey, TValue>(new Dictionary<TKey, TValue>());
        }

        private class DictionaryAdapter<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;

            public DictionaryAdapter(Dictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }


            public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
            {
                TValue value = _dictionary.ContainsKey(key) ? updateValueFactory(key, addValue) : addValue;
                _dictionary[key] = value;
                return value;
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];

                var value = valueFactory(key);

                _dictionary[key] = value;

                return value;
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public bool TryRemove(TKey key, out TValue value)
            {
                if (!_dictionary.ContainsKey(key))
                {
                    value = default(TValue);
                    return false;
                }

                value = _dictionary[key];

                _dictionary.Remove(key);

                return true;
            }
        }
    }
}