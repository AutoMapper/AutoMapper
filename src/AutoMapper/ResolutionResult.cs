using System;

namespace AutoMapper
{
	public class ResolutionResult
	{
		public ResolutionResult(object value, Type type)
		{
			Value = value;
			Type = value == null
			       	? type
			       	: value.GetType();
		}

		public ResolutionResult(object value)
			: this(value, typeof(object))
		{
		}

		public object Value { get; private set; }
		public Type Type { get; private set; }
	}
}