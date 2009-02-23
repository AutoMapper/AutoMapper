using System;
using System.Reflection;
using AutoMapper.ReflectionExtensions;

namespace AutoMapper.Mappers
{
	public class NullableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			IMemberAccessor hasValueProp = context.SourceType.GetAccessor("HasValue", BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.Instance);
			IMemberAccessor valueProp = context.SourceType.GetAccessor("Value", BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.Instance);
			Type sourceType = context.SourceType.GetGenericArguments()[0];

			var hasValue = (bool)hasValueProp.GetValue(context.SourceValue);
			object value = null;

			if (hasValue)
			{
				value = valueProp.GetValue(context.SourceValue);
			}

			var newContext = context.CreateValueContext(value, sourceType);

			return mapper.Map(newContext);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceType.IsNullableType();
		}
	}
}