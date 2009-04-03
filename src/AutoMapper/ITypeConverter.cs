namespace AutoMapper
{
	public interface ITypeConverter<TSource, TDestination>
	{
		TDestination Convert(TSource source);
	}
}