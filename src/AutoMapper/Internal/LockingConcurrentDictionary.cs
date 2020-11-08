using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public struct LockingConcurrentDictionary<TKey, TValue>
    {
        private ConcurrentDictionaryWrapper<TKey, Lazy<TValue>> _dictionary;
        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory, int capacity = 31) =>
            _dictionary = new ConcurrentDictionaryWrapper<TKey, Lazy<TValue>>(key => new Lazy<TValue>(() => valueFactory(key)), capacity);
        public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key).Value;
        public TValue this[in TKey key]
        {
            get => _dictionary[key].Value;
            set => _dictionary[key] = new Lazy<TValue>(() => value);
        }
        public bool ContainsKey(in TKey key) => _dictionary.ContainsKey(key);
        public ICollection<TKey> Keys => _dictionary.Keys;
        public TValue GetOrDefault(in TKey key) => _dictionary.TryGetValue(key, out Lazy<TValue> lazy) ? lazy.Value : default;
    }
    public struct ConcurrentDictionaryWrapper<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
        private readonly Func<TKey, TValue> _valueFactory;
        public ConcurrentDictionaryWrapper(Func<TKey, TValue> valueFactory, int capacity = 31)
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity);
            _valueFactory = key => valueFactory(key);
        }
        public TValue this[in TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }
        public TValue GetOrDefault(in TKey key)
        {
            _dictionary.TryGetValue(key, out var value);
            return value;
        }
        public ICollection<TKey> Keys => _dictionary.Keys;
        public bool ContainsKey(in TKey key) => _dictionary.ContainsKey(key);
        public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key, _valueFactory);
        public bool TryGetValue(in TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
    }
}