namespace AutoMapper
{
    /// <summary>
    /// Converts source type to destination type instead of normal member mapping
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public interface ITypeConverter<in TSource, TDestination>
    {
        /// <summary>
        /// Performs conversion from source to destination type
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <param name="context">Resolution context</param>
        /// <returns>Destination object</returns>
        TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
    }
}
