namespace AutoMapper.Mappers
{
    using System;
    using Internal;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class EnumMapper : IObjectMapper
    {
        private static readonly INullableConverterFactory NullableConverterFactory =
            PlatformAdapter.Resolve<INullableConverterFactory>();

        private static readonly IEnumNameValueMapperFactory EnumNameValueMapperFactory =
            PlatformAdapter.Resolve<IEnumNameValueMapperFactory>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            var toEnum = false;
            var enumSourceType = TypeHelper.GetEnumerationType(context.SourceType);
            var enumDestinationType = TypeHelper.GetEnumerationType(context.DestinationType);

            if (EnumToStringMapping(context, ref toEnum))
            {
                if (context.SourceValue == null)
                {
                    return runner.CreateObject(context);
                }

                if (toEnum)
                {
                    var stringValue = context.SourceValue.ToString();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return runner.CreateObject(context);
                    }

                    return Enum.Parse(enumDestinationType, stringValue, true);
                }
                return Enum.GetName(enumSourceType, context.SourceValue);
            }
            if (EnumToEnumMapping(context))
            {
                if (context.SourceValue == null)
                {
                    if (runner.ShouldMapSourceValueAsNull(context) && context.DestinationType.IsNullableType())
                        return null;

                    return runner.CreateObject(context);
                }

                if (!Enum.IsDefined(enumSourceType, context.SourceValue))
                {
                    return Enum.ToObject(enumDestinationType, context.SourceValue);
                }

                if (FeatureDetector.IsEnumGetNamesSupported)
                {
                    var enumValueMapper = EnumNameValueMapperFactory.Create();

                    if (enumValueMapper.IsMatch(enumDestinationType, context.SourceValue.ToString()))
                    {
                        return enumValueMapper.Convert(enumSourceType, enumDestinationType, context);
                    }
                }

                return Enum.Parse(enumDestinationType, Enum.GetName(enumSourceType, context.SourceValue), true);
            }
            if (EnumToUnderlyingTypeMapping(context, ref toEnum))
            {
                if (toEnum)
                {
                    return Enum.Parse(enumDestinationType, context.SourceValue.ToString(), true);
                }

                if (EnumToNullableTypeMapping(context))
                {
                    return ConvertEnumToNullableType(context);
                }

                return Convert.ChangeType(context.SourceValue, context.DestinationType, null);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            bool toEnum = false;
            return EnumToStringMapping(context, ref toEnum) || EnumToEnumMapping(context) ||
                   EnumToUnderlyingTypeMapping(context, ref toEnum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool EnumToEnumMapping(ResolutionContext context)
        {
            // Enum to enum mapping
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return sourceEnumType != null && destEnumType != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="toEnum"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="toEnum"></param>
        /// <returns></returns>
        private static bool EnumToStringMapping(ResolutionContext context, ref bool toEnum)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool EnumToNullableTypeMapping(ResolutionContext context)
        {
            if (!context.DestinationType.IsGenericType())
            {
                return false;
            }

            var genericType = context.DestinationType.GetGenericTypeDefinition();
            // ReSharper disable once CheckForReferenceEqualityInstead.1
            return genericType.Equals(typeof (Nullable<>));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static object ConvertEnumToNullableType(ResolutionContext context)
        {
            var nullableConverter = NullableConverterFactory.Create(context.DestinationType);

            if (context.IsSourceValueNull)
            {
                return nullableConverter.ConvertFrom(context.SourceValue);
            }

            var destType = nullableConverter.UnderlyingType;
            return Convert.ChangeType(context.SourceValue, destType, null);
        }
    }
}