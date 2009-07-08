using System;
using System.Reflection;

namespace AutoMapper
{
	public class PropertyNameResolver<TSource> : IValueResolver
	{
		private readonly string _propertyName;
		private PropertyInfo _propertyInfo;

		public PropertyNameResolver(string propertyName)
		{
			_propertyName = propertyName;
			_propertyInfo = typeof(TSource).GetProperty(_propertyName);
		}


		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value == null)
				return source;

			if (!(source.Value is TSource))
				throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " + source.Value.GetType());

			object result;
			try
			{
				result = _propertyInfo.GetValue(source.Value, null);
			}
			catch (NullReferenceException)
			{
				result = null;
			}

			return new ResolutionResult(result);
		}

	}
}
