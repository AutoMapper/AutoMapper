using System.Collections.Concurrent;
namespace AutoMapper.Internal;
public readonly struct LockingConcurrentDictionary<TKey, TValue>
{
    private readonly Func<TKey, Lazy<TValue>> _valueFactory;
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
    public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory, int capacity = 31)
    {
        _valueFactory = key => new(()=>valueFactory(key));
        _dictionary = new(Environment.ProcessorCount, capacity);
    }
    public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;
}