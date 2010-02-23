using System;

namespace AutoMapper.Mappers
{
    public class EnumMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            bool toEnum = false;
            Type enumSourceType = TypeHelper.GetEnumerationType(context.SourceType);

            if (EnumToStringMapping(context, ref toEnum))
            {
                if (toEnum)
                {
                    return Enum.Parse(context.DestinationType, context.SourceValue.ToString(), true);
                }
                return Enum.GetName(enumSourceType, context.SourceValue);
            }
            if (EnumToEnumMapping(context))
            {
                Type enumDestType = TypeHelper.GetEnumerationType(context.DestinationType);

                if (context.SourceValue == null)
                {
                    return mapper.CreateObject(context);
                }

                if (!Enum.IsDefined(enumSourceType, context.SourceValue))
                {
                    return Enum.ToObject(context.DestinationType, context.SourceValue);
                }

                return Enum.Parse(enumDestType, Enum.GetName(enumSourceType, context.SourceValue), true);
            }
            if (EnumToUnderlyingTypeMapping(context, ref toEnum))
            {
                if (toEnum)
                {
                    return Enum.Parse(context.DestinationType, context.SourceValue.ToString(), true);
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
                return context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(context.SourceType));
            }
            if (destEnumType != null)
            {
                toEnum = true;
                return context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(context.DestinationType));
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