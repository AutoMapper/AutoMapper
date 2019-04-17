using AutoMapper.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoMapper.Features
{
    public class Features<TValue> : IEnumerable<TValue>
    {
        private IDictionary<Type, TValue> _features = new Dictionary<Type, TValue>();

        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <returns>The feature or null if feature not exists.</returns>
        public TFeature Get<TFeature>() where TFeature : TValue => (TFeature)_features.GetOrDefault(typeof(TFeature));

        /// <summary>
        /// Add or update the feature. Existing feature of the same type will be replaced.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public void AddOrUpdate(TValue feature) => _features[feature.GetType()] = feature;

        public void AddOrUpdateRange(IEnumerable<TValue> features)
        {
            foreach (var feature in features)
            {
                AddOrUpdate(feature);
            }
        }

        public IEnumerator<TValue> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected void MakeReadOnly() => _features = new ReadOnlyDictionary<Type, TValue>(_features);

        public void ForAll(Action<TValue> action)
        {
            foreach (var feature in this)
            {
                action(feature);
            }
        }
    }
}