using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
        private readonly Func<TKey, Lazy<TValue>> _valueFactory;

        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory, int capacity = 31)
        {
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>(Environment.ProcessorCount, capacity);
            _valueFactory = key => new Lazy<TValue>(() => valueFactory(key));
        }

        public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;
        public TValue GetOrAdd(in TKey key, Func<TKey, Lazy<TValue>> valueFactory) => _dictionary.GetOrAdd(key, valueFactory).Value;

        public TValue this[in TKey key]
        {
            get => _dictionary[key].Value;
            set => _dictionary[key] = new Lazy<TValue>(() => value);
        }

        public bool TryGetValue(in TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool ContainsKey(in TKey key) => _dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => _dictionary.Keys;

        public TValue GetOrDefault(in TKey key)
        {
            TryGetValue(key, out var value);
            return value;
        }

        public void Clear() => _dictionary.Clear();
    }
}