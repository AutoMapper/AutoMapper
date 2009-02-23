using System;

namespace AutoMapper.Mappers
{
	public class NewOrDefaultMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			object valueToAssign;

			if (context.DestinationType == typeof(string))
			{
				valueToAssign = mapper.FormatValue(context.CreateValueContext(null));
			}
			else if (context.DestinationType.IsArray)
			{
				Type elementType = context.DestinationType.GetElementType();
				Array arrayValue = Array.CreateInstance(elementType, 0);
				valueToAssign = arrayValue;
			}
			else
			{
				valueToAssign = context.DestinationValue ?? mapper.CreateObject(context.DestinationType);
			}

			return valueToAssign;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceValue == null;
		}
	}
}