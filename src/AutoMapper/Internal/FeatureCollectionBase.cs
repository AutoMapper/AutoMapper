using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Internal
{
    public class FeatureCollectionBase<TValue> : IEnumerable<KeyValuePair<Type, TValue>>
    {
        private readonly Lazy<ConcurrentDictionary<Type, TValue>> _features = new Lazy<ConcurrentDictionary<Type, TValue>>(() => new ConcurrentDictionary<Type, TValue>());
        private bool _readOnly;

        public TValue this[Type key]
        {
            get
            {
                return _features.IsValueCreated && _features.Value.TryGetValue(key, out var feature)
                    ? feature
                    : default(TValue);
            }
            set
            {
                if (_readOnly)
                {
                    throw new NotSupportedException("Features collection is read only.");
                }

                if (value == null)
                {
                    if (!_features.IsValueCreated)
                    {
                        return;
                    }
                    _features.Value.TryRemove(key, out var _);
                }

                _features.Value.AddOrUpdate(key, value, (_, __) => value);
            }
        }

        public IEnumerator<KeyValuePair<Type, TValue>> GetEnumerator()
        {
            if (_features.IsValueCreated)
            {
                foreach (var feature in _features.Value)
                {
                    yield return feature;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void MakeReadOnly()
        {
            _readOnly = true;
        }
    }
}
