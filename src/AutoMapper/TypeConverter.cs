namespace AutoMapper
{
	public abstract class TypeConverter<TSource, TDestination> : ITypeConverter
	{
		public object Convert(object source)
		{
			return ConvertCore((TSource) source);
		}

		protected abstract TDestination ConvertCore(TSource source);
	}
}