namespace AutoMapper
{
	public abstract class ValueResolver<TSource, TDestination> : IValueResolver
	{
		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, typeof(TDestination))
			       	: new ResolutionResult(ResolveCore((TSource) source.Value), typeof(TDestination));
		}

		protected abstract TDestination ResolveCore(TSource source);
	}
}