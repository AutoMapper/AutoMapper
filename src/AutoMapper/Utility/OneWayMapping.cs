namespace AutoMapper.Utility
{
    /// <summary>
    /// Creates a one way mapping for the specified types
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    public abstract class OneWayMapping<TSource, TDestination> : IMapping
    {
        /// <summary>
        /// Configures the mapping.
        /// </summary>
        public void ConfigureMapping()
        {
            Configure(Mapper.CreateMap<TSource, TDestination>());
        }

        /// <summary>
        /// Configures the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        protected abstract void Configure(IMappingExpression<TSource, TDestination> mapping);
    }
}
