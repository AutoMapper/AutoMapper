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
    }
}
