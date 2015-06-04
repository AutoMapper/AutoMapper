namespace AutoMapper
{
    /// <summary>
    /// Custom mapping action
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public interface IMappingAction<TSource, TDestination>
    {
        /// <summary>
        /// Implementors can modify both the source and destination objects
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        void Process(TSource source, TDestination destination);
    }
}