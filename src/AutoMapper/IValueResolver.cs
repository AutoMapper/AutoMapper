using System;

namespace AutoMapper
{
	public interface IValueResolver
	{
		object Resolve(object model);
		Type GetResolvedValueType();
	}

	public abstract class ValueResolver<TSource, TDestination> : IValueResolver
	{
		public object Resolve(object model)
		{
			return ResolveCore((TSource)model);
		}

		public Type GetResolvedValueType()
		{
			return typeof (TDestination);
		}

		protected abstract TDestination ResolveCore(TSource model);
	}
}