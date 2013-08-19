using AutoMapper.Internal;

namespace AutoMapper
{
	public abstract class ValueFormatter<T> : IValueFormatter
	{
		public string FormatValue(ResolutionContext context)
		{
			if (context.SourceValue == null)
				return null;

			if (!(context.SourceValue is T))
				return context.SourceValue.ToNullSafeString();

			return FormatValueCore((T)context.SourceValue);
		}

		protected abstract string FormatValueCore(T value);
	}
}