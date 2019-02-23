using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoMapper.Internal
{
    public class FeatureCollectionBase<TValue> : IEnumerable<KeyValuePair<Type, TValue>>
    {
        private IDictionary<Type, TValue> _features = new Dictionary<Type, TValue>();

        public TValue this[Type key]
        {
            get
            {
                return _features.TryGetValue(key, out var feature)
                    ? feature
                    : default(TValue);
            }
            set
            {
                if (value == null)
                {
                    _features.Remove(key);
                }
                else
                {
                    _features[key] = value;
                }
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
            foreach (var feature in _features)
            {
                yield return feature;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void MakeReadOnly()
        {
            _features = new ReadOnlyDictionary<Type, TValue>(_features);
        }
    }
}
