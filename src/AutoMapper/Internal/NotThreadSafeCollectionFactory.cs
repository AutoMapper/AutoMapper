using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class CollectionFactory : ICollectionFactory
    {
        public IDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue>()
        {
            return new ConcurrentDictionaryImpl<TKey, TValue>(new Dictionary<TKey, TValue>());
        }

        public ISet<T> CreateSet<T>()
        {
#if WINDOWS_PHONE
            throw new PlatformNotSupportedException("ISet not available on Windows Phone");
#else
            return new HashSetImpl<T>(new HashSet<T>());
#endif
        }

        private class ConcurrentDictionaryImpl<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;

            public ConcurrentDictionaryImpl(Dictionary<TKey, TValue> dictionary)
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
    
#if !WINDOWS_PHONE
        private class HashSetImpl<T> : ISet<T>
        {
            private readonly HashSet<T> _hashSet;

            public HashSetImpl(HashSet<T> hashSet)
            {
                _hashSet = hashSet;
            }

            public bool Add(T item)
            {
                return _hashSet.Add(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _hashSet.GetEnumerator();
            }
        }
#endif
    }
}
