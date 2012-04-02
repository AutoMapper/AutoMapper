namespace AutoMapper
{
	public interface ITypeConverter<in TSource, out TDestination>
	{
		TDestination Convert(ResolutionContext context);
	}

	public abstract class TypeConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination> 
	{
		public TDestination Convert(ResolutionContext context)
		{
			if (context.SourceValue != null && !(context.SourceValue is TSource))
			{
				throw new AutoMapperMappingException(context, string.Format("Value supplied is of type {0} but expected {1}.\nChange the type converter source type, or redirect the source value supplied to the value resolver using FromMember.", typeof(TSource), context.SourceValue.GetType()));
			}

			return ConvertCore((TSource)context.SourceValue);
		}

		protected abstract TDestination ConvertCore(TSource source);
	}
}