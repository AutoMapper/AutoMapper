using AutoMapper.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoMapper.Features
{
    public class Features<TFeature> : IEnumerable<TFeature>
    {
        private IDictionary<Type, TFeature> _features = new Dictionary<Type, TFeature>();

        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeatureToFind"/>.
        /// </summary>
        /// <typeparam name="TFeatureToFind">The type of the feature.</typeparam>
        /// <returns>The feature or null if feature not exists.</returns>
        public TFeatureToFind Get<TFeatureToFind>() where TFeatureToFind : TFeature => (TFeatureToFind)_features.GetOrDefault(typeof(TFeatureToFind));

        /// <summary>
        /// Add or update the feature. Existing feature of the same type will be replaced.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public void Set(TFeature feature) => _features[feature.GetType()] = feature;

        public IEnumerator<TFeature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void MakeReadOnly() => _features = new ReadOnlyDictionary<Type, TFeature>(_features);
    }
}