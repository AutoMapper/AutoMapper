namespace AutoMapper.Utility
{
    /// <summary>
    /// Creates a two way mapping for the specified types
    /// </summary>
    /// <typeparam name="TFirst">The type of the first.</typeparam>
    /// <typeparam name="TSecond">The type of the second.</typeparam>
    public abstract class TwoWayMapping<TFirst, TSecond> : IMapping
    {
        /// <summary>
        /// Configures the mapping.
        /// </summary>
        public void ConfigureMapping()
        {
            Configure(Mapper.CreateMap<TFirst, TSecond>());
            Configure(Mapper.CreateMap<TSecond, TFirst>());
        }

        /// <summary>
        /// Configures the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        protected abstract void Configure(IMappingExpression<TFirst, TSecond> mapping);
        /// <summary>
        /// Configures the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        protected abstract void Configure(IMappingExpression<TSecond, TFirst> mapping);
    }
}
