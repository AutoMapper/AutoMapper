using System;
using System.Collections.Concurrent;

namespace AutoMapper.Internal
{
    public readonly struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionaryWrapper<TKey, Lazy<TValue>> _dictionary;
        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory, int capacity = 31) =>
            _dictionary = new(key => new(() => valueFactory(key)), capacity);
        public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key).Value;
    }
    public readonly struct ConcurrentDictionaryWrapper<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
        private readonly Func<TKey, TValue> _valueFactory;
        public ConcurrentDictionaryWrapper(Func<TKey, TValue> valueFactory, int capacity = 31)
        {
            _dictionary = new(Environment.ProcessorCount, capacity);
            _valueFactory = valueFactory;
        }
        public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key, _valueFactory);
    }
}