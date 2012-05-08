namespace AutoMapper
{
	public abstract class ValueResolver<TSource, TDestination> : IValueResolver
	{
		public ResolutionResult Resolve(ResolutionResult source)
		{
            if (source.Value != null && !(source.Value is TSource))
            {
                throw new AutoMapperMappingException(string.Format("Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.", source.Value.GetType(), typeof(TSource)));
            }

			return source.New(ResolveCore((TSource) source.Value), typeof (TDestination));
		}

		protected abstract TDestination ResolveCore(TSource source);
	}
}