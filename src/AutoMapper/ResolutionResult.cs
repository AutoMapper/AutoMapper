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
		{
			Value = value;
			Type = value == null
			       	? typeof (object)
			       	: value.GetType();
		}

		public object Value { get; private set; }
		public Type Type { get; private set; }
	}
}