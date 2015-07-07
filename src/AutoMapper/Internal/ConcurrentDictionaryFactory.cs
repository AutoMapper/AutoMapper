#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class DictionaryFactoryOverride : IDictionaryFactory
    {
        public IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
        {
            return new ConcurrentDictionaryImpl<TKey, TValue>(new ConcurrentDictionary<TKey, TValue>());
        }

        private class ConcurrentDictionaryImpl<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

            public ConcurrentDictionaryImpl(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }


            public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
            {
                return _dictionary.AddOrUpdate(key, addValue, updateValueFactory);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                return _dictionary.GetOrAdd(key, valueFactory);
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public bool TryRemove(TKey key, out TValue value)
            {
                return _dictionary.TryRemove(key, out value);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public ICollection<TValue> Values => _dictionary.Values;
            public ICollection<TKey> Keys => _dictionary.Keys;

            public bool ContainsKey(TKey key)
            {
                return _dictionary.ContainsKey(key);
            }
        }
    }
}

#endif