namespace AutoMapper
{
    public static class MappingExpressionFeatureCollectionExtensions
    {
        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <param name="features">The features.</param>
        /// <returns>The feature or null if feature not exists.</returns>
        public static TFeature Get<TFeature>(this IMappingExpressionFeatureCollection features)
            where TFeature : IMappingExpressionFeature
        {
            return (TFeature)features[typeof(TFeature)];
        }

        /// <summary>
        /// Sets the feature for type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the t feature.</typeparam>
        /// <param name="features">The features.</param>
        /// <param name="feature">The feature. To remove a feature, pass null value.</param>
        public static void Set<TFeature>(this IMappingExpressionFeatureCollection features, TFeature feature)
            where TFeature : IMappingExpressionFeature
        {
            features[typeof(TFeature)] = feature;
        }
    }
}
