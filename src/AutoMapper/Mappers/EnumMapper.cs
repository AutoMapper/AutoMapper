using System.ComponentModel;

namespace AutoMapper.Mappers
{
    using System;
    using Internal;
    using System.Reflection;
    using System.Linq;

    public class EnumMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            bool toEnum = false;
            Type enumSourceType = TypeHelper.GetEnumerationType(context.SourceType);
            Type enumDestinationType = TypeHelper.GetEnumerationType(context.DestinationType);

            if (EnumToStringMapping(context.Types, ref toEnum))
            {
                if (context.SourceValue == null)
                {
                    return context.Engine.CreateObject(context);
                }

                if (toEnum)
                {
                    var stringValue = context.SourceValue.ToString();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return context.Engine.CreateObject(context);
                    }

                    return Enum.Parse(enumDestinationType, stringValue, true);
                }
                return Enum.GetName(enumSourceType, context.SourceValue);
            }
            if (EnumToEnumMapping(context.Types))
            {
                if (context.SourceValue == null)
                {
                    if (context.Engine.ShouldMapSourceValueAsNull(context) && context.DestinationType.IsNullableType())
                        return null;

                    return context.Engine.CreateObject(context);
                }

                if (!Enum.IsDefined(enumSourceType, context.SourceValue))
                {
                    return Enum.ToObject(enumDestinationType, context.SourceValue);
                }

                if (!Enum.GetNames(enumDestinationType).Contains(context.SourceValue.ToString()))
                {
                    Type underlyingSourceType = Enum.GetUnderlyingType(enumSourceType);
                    var underlyingSourceValue = Convert.ChangeType(context.SourceValue, underlyingSourceType);

                    return Enum.ToObject(context.DestinationType, underlyingSourceValue);
                }

                return Enum.Parse(enumDestinationType, Enum.GetName(enumSourceType, context.SourceValue), true);
            }
            if (EnumToUnderlyingTypeMapping(context.Types, ref toEnum))
            {
                if (toEnum && context.SourceValue != null)
                {
                    return Enum.Parse(enumDestinationType, context.SourceValue.ToString(), true);
                }

                if (EnumToNullableTypeMapping(context.Types))
                {
                    return ConvertEnumToNullableType(context);
                }

                return Convert.ChangeType(context.SourceValue, context.DestinationType, null);
            }
            return null;
        }

        public bool IsMatch(TypePair context)
        {
            bool toEnum = false;
            return EnumToStringMapping(context, ref toEnum) || EnumToEnumMapping(context) ||
                   EnumToUnderlyingTypeMapping(context, ref toEnum);
        }

        private static bool EnumToEnumMapping(TypePair context)
        {
            // Enum to enum mapping
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return sourceEnumType != null && destEnumType != null;
        }

        private static bool EnumToUnderlyingTypeMapping(TypePair context, ref bool toEnum)
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

        private static bool EnumToStringMapping(TypePair context, ref bool toEnum)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            // Enum to string
            if (sourceEnumType != null)
            {
                return context.DestinationType.IsAssignableFrom(typeof (string));
            }
            if (destEnumType != null)
            {
                toEnum = true;
                return context.SourceType.IsAssignableFrom(typeof (string));
            }
            return false;
        }

        private static bool EnumToNullableTypeMapping(TypePair context)
        {
            if (!context.DestinationType.IsGenericType())
            {
                return false;
            }

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (Nullable<>);
        }

        private static object ConvertEnumToNullableType(ResolutionContext context)
        {
#if !PORTABLE
            var nullableConverter = new NullableConverter(context.DestinationType);

            if (context.IsSourceValueNull)
            {
                return nullableConverter.ConvertFrom(context.SourceValue);
            }

            var destType = nullableConverter.UnderlyingType;
            return Convert.ChangeType(context.SourceValue, destType, null);
#else
            if (context.IsSourceValueNull)
            {
                return null;
            }

            var destType = Nullable.GetUnderlyingType(context.DestinationType);

            return Convert.ChangeType(context.SourceValue, destType, null);
#endif
        }
    }
}