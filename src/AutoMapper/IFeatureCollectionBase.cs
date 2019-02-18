using System;
using System.Collections.Generic;

namespace AutoMapper
{
    public interface IFeatureCollectionBase<TValue> : IEnumerable<KeyValuePair<Type, TValue>>
    {
        /// <summary>
        /// Gets or sets the feature.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Object.</returns>
        TValue this[Type key] { get; set; }

        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <returns>The feature or null if feature not exists.</returns>
        TFeature Get<TFeature>() where TFeature : TValue;

        /// <summary>
        /// Sets the feature for type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the t feature.</typeparam>
        /// <param name="feature">The feature. To remove a feature, pass null value.</param>
        void Set<TFeature>(TFeature feature) where TFeature : TValue;
    }
}
