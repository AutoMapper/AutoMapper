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

        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <returns>The feature or null if feature not exists.</returns>
        public TFeature Get<TFeature>()
            where TFeature : TValue
        {
            return (TFeature)this[typeof(TFeature)];
        }

        /// <summary>
        /// Sets the feature for type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the t feature.</typeparam>
        /// <param name="feature">The feature. To remove a feature, pass null value.</param>
        public void Set<TFeature>(TFeature feature)
            where TFeature : TValue
        {
            this[typeof(TFeature)] = feature;
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
