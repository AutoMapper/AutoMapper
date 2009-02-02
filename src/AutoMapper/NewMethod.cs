using System;

namespace AutoMapper
{
	public class NewMethod<TSource> : IValueResolver
	{
		private readonly Func<TSource, object> _method;

		public NewMethod(Func<TSource, object> method)
		{
			_method = method;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value == null)
				return source;

			if (! (source.Value is TSource))
				throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " + source.Value.GetType());

			return new ResolutionResult(_method((TSource) source.Value));
		}
	}
}