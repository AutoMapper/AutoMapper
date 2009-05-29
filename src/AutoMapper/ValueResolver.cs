namespace AutoMapper
{
	public abstract class ValueResolver<TSource, TDestination> : IValueResolver
	{
		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value == null)
			{
				return new ResolutionResult(source.Value, typeof (TDestination));
			}

			if (!(source.Value is TSource))
			{
				throw new AutoMapperMappingException(string.Format("Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.", typeof(TSource), source.Value.GetType()));
			}

			return new ResolutionResult(ResolveCore((TSource) source.Value), typeof (TDestination));
		}

		protected abstract TDestination ResolveCore(TSource source);
	}
}