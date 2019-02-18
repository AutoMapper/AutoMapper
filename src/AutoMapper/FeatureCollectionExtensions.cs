namespace AutoMapper
{
    public static class FeatureCollectionExtensions
    {
        /// <summary>
        /// Gets the feature of type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <param name="features">The features.</param>
        /// <returns>The feature or null if feature not exists.</returns>
        public static TFeature Get<TFeature>(this IFeatureCollection features)
            where TFeature : IFeature
        {
            return (TFeature)features[typeof(TFeature)];
        }

        /// <summary>
        /// Sets the feature for type <typeparamref name="TFeature"/>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the t feature.</typeparam>
        /// <param name="features">The features.</param>
        /// <param name="feature">The feature. To remove a feature, pass null value.</param>
        public static void Set<TFeature>(this IFeatureCollection features, TFeature feature)
            where TFeature : IFeature
        {
            features[typeof(TFeature)] = feature;
        }
    }
}
