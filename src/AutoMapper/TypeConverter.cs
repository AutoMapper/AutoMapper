namespace AutoMapper
{
    /// <summary>
    /// Generic-friendly implementation of <see cref="ITypeConverter{TSource,TDestination}"/>
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public abstract class TypeConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination> 
    {
        public TDestination Convert(ResolutionContext context)
        {
            if (context.SourceValue != null && !(context.SourceValue is TSource))
            {
                throw new AutoMapperMappingException(context,
                    $"Value supplied is of type {typeof (TSource)} but expected {context.SourceValue.GetType()}.\nChange the type converter source type, or redirect the source value supplied to the value resolver using FromMember.");
            }

            return ConvertCore((TSource)context.SourceValue);
        }

        /// <summary>
        /// When overridden in a base class, this method is provided the casted source object extracted from the <see cref="ResolutionContext"/>
        /// </summary>
        /// <param name="source">Source object</param>
        /// <returns>Destination object</returns>
        protected abstract TDestination ConvertCore(TSource source);
    }
}