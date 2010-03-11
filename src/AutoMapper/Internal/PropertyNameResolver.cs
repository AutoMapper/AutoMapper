using System;
using System.Reflection;

namespace AutoMapper
{
	public class PropertyNameResolver : IValueResolver
	{
	    private readonly Type _sourceType;
	    private readonly string _propertyName;
		private PropertyInfo _propertyInfo;

		public PropertyNameResolver(Type sourceType, string propertyName)
		{
		    _sourceType = sourceType;
		    _propertyName = propertyName;
            _propertyInfo = sourceType.GetProperty(_propertyName);
		}


		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value == null)
				return source;

		    var valueType = source.Value.GetType();
		    if (!(_sourceType.IsAssignableFrom(valueType)))
                throw new ArgumentException("Expected obj to be of type " + _sourceType + " but was " + valueType);

			object result;
			try
			{
				result = _propertyInfo.GetValue(source.Value, null);
			}
			catch (NullReferenceException)
			{
				result = null;
			}

			return source.New(result);
		}

	}
}
