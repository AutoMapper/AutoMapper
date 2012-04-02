using System;
using System.Linq;

namespace AutoMapper.Mappers
{
    public class EnumMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            bool toEnum = false;
            Type enumSourceType = TypeHelper.GetEnumerationType(context.SourceType);
        	Type enumDestinationType = TypeHelper.GetEnumerationType(context.DestinationType);

            if (EnumToStringMapping(context, ref toEnum))
            {
                if (context.SourceValue == null)
                {
                    return mapper.CreateObject(context);
                }

                if (toEnum)
                {
                    var stringValue = context.SourceValue.ToString();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return mapper.CreateObject(context);
                    }

                    return Enum.Parse(enumDestinationType, stringValue, true);
                }
                return Enum.GetName(enumSourceType, context.SourceValue);
            }
            if (EnumToEnumMapping(context))
            {
                if (context.SourceValue == null)
                {
                    return mapper.CreateObject(context);
                }

                if (!Enum.IsDefined(enumSourceType, context.SourceValue))
                {
					return Enum.ToObject(enumDestinationType, context.SourceValue);
                }

#if !SILVERLIGHT
                if (!Enum.GetNames(enumDestinationType).Contains(context.SourceValue.ToString()))
                {
                    Type underlyingSourceType = Enum.GetUnderlyingType(enumSourceType);
                    var underlyingSourceValue = Convert.ChangeType(context.SourceValue, underlyingSourceType);

                    return Enum.ToObject(context.DestinationType, underlyingSourceValue);
                }
#endif

				return Enum.Parse(enumDestinationType, Enum.GetName(enumSourceType, context.SourceValue), true);
            }
            if (EnumToUnderlyingTypeMapping(context, ref toEnum))
            {
                if (toEnum)
                {
                    return Enum.Parse(enumDestinationType, context.SourceValue.ToString(), true);
                }
                return Convert.ChangeType(context.SourceValue, context.DestinationType, null);
            }
            return null;
        }

        public bool IsMatch(ResolutionContext context)
        {
            bool toEnum = false;
            return EnumToStringMapping(context, ref toEnum) || EnumToEnumMapping(context) || EnumToUnderlyingTypeMapping(context, ref toEnum);
        }

        private static bool EnumToEnumMapping(ResolutionContext context)
        {
            // Enum to enum mapping
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return sourceEnumType != null && destEnumType != null;
        }

        private static bool EnumToUnderlyingTypeMapping(ResolutionContext context, ref bool toEnum)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            // Enum to underlying type
            if (sourceEnumType != null)
            {
				return context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(sourceEnumType));
            }
            if (destEnumType != null)
            {
                toEnum = true;
				return context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(destEnumType));
            }
            return false;
        }

        private static bool EnumToStringMapping(ResolutionContext context, ref bool toEnum)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            // Enum to string
            if (sourceEnumType != null)
            {
                return context.DestinationType.IsAssignableFrom(typeof(string));
            }
            if (destEnumType != null)
            {
                toEnum = true;
                return context.SourceType.IsAssignableFrom(typeof(string));
            }
            return false;
        }
    }
}