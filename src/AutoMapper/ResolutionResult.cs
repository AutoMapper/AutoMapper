using System;

namespace AutoMapper
{
	public class ResolutionResult
	{
		private readonly object _value;
		private readonly Type _type;

		public ResolutionResult(object value, Type type)
		{
			_value = value;
			_type = value == null
			       	? type
			       	: value.GetType();
		}

		public ResolutionResult(object value)
			: this(value, typeof(object))
		{
		}

		public object Value { get { return _value; } }
		public Type Type { get { return _type; } }
	}
}