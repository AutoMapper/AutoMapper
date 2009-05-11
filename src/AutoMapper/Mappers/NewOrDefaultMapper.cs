using System;

namespace AutoMapper.Mappers
{
	public class NewOrDefaultMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			object valueToAssign;

			var mapNullSourceValuesAsNull = MapSourceValuesAsNull(context, mapper);

			if (mapNullSourceValuesAsNull)
			{
				valueToAssign = null;
			}
			else if (context.DestinationType == typeof(string))
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

		private bool MapSourceValuesAsNull(ResolutionContext context, IMappingEngineRunner mapper)
		{
			var typeMap = context.GetContextTypeMap();
			if (typeMap != null)
				return mapper.ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

			return mapper.ConfigurationProvider.MapNullSourceValuesAsNull;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceValue == null;
		}
	}
}