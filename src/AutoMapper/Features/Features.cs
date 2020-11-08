using AutoMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Features
{
    public class Features<TFeature> : IReadOnlyCollection<TFeature>
    {
        private IDictionary<Type, TFeature> _features;
        public int Count => _features == null ? 0 : _features.Count;
        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeatureToFind"/>.
        /// </summary>
        /// <typeparam name="TFeatureToFind">The type of the feature.</typeparam>
        /// <returns>The feature or null if feature not exists.</returns>
        public TFeatureToFind Get<TFeatureToFind>() where TFeatureToFind : TFeature =>
            _features == null ? default : (TFeatureToFind)_features.GetOrDefault(typeof(TFeatureToFind));
        /// <summary>
        /// Add or update the feature. Existing feature of the same type will be replaced.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public void Set(TFeature feature)
        {
            _features ??= new Dictionary<Type, TFeature>();
            _features[feature.GetType()] = feature;
        }
        public IEnumerator<TFeature> GetEnumerator() =>
            _features == null ? Enumerable.Empty<TFeature>().GetEnumerator() : _features.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}