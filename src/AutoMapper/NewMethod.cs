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

		public object Resolve(object obj)
		{
			if (obj is TSource)
				return GetValue((TSource) obj);
			throw new ArgumentException("Expected obj to be of type " + typeof (TSource) + " but was " + obj.GetType());
		}

		public object GetValue(TSource source)
		{
			return _method(source);
		}

		public Type GetResolvedValueType()
		{
			return _method.Method.ReturnType;
		}
	}
}