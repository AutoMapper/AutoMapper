using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper
{
    internal struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
        private readonly Func<TKey, Lazy<TValue>> _valueFactory;

        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory)
        {
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            _valueFactory = key => new Lazy<TValue>(() => valueFactory(key));
        }

        public TValue GetOrAdd(TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;

        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key].Value;
            }
            set
            {
                _dictionary[key] = new Lazy<TValue>(() => value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Lazy<TValue> lazy;
            if(_dictionary.TryGetValue(key, out lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => _dictionary.Keys;        
    }
}