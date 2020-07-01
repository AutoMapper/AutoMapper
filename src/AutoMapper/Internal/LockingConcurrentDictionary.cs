using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
        private readonly Func<TKey, Lazy<TValue>> _valueFactory;

        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory)
        {
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            _valueFactory = key => new Lazy<TValue>(() => valueFactory(key));
        }

        public TValue GetOrAdd(TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;
        public TValue GetOrAdd(TKey key, Func<TKey, Lazy<TValue>> valueFactory) => _dictionary.GetOrAdd(key, valueFactory).Value;

        public TValue this[TKey key]
        {
            get => _dictionary[key].Value;
            set => _dictionary[key] = new Lazy<TValue>(() => value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => _dictionary.Keys;        
    }
}