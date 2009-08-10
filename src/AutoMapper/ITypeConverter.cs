namespace AutoMapper
{
	public interface ITypeConverter<TSource, TDestination>
	{
		TDestination Convert(TSource source);
	}

    public interface IWithContextTypeConverter<TSource, TDestination>
    {
        TDestination Convert(ResolutionContext resolutionContext, TSource childId);
    }
}