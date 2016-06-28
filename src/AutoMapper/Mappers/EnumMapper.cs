using System.ComponentModel;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using System.Linq;
    
    public class StringToEnumMapper : IObjectMapper
    {
        public static TDestination Map<TDestination>(string source)
        {
            if (string.IsNullOrEmpty(source))
                return default(TDestination);
            return (TDestination)Enum.Parse(typeof(TDestination), source, true);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(StringToEnumMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return destEnumType != null && context.SourceType == typeof(string);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type), sourceExpression);
        }
    }

    public class EnumToEnumMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null)
                return default(TDestination);

            var sourceEnumType = TypeHelper.GetEnumerationType(typeof(TSource));
            var destEnumType = TypeHelper.GetEnumerationType(typeof(TDestination));

            if (!Enum.IsDefined(sourceEnumType, source))
            {
                return (TDestination)Enum.ToObject(destEnumType, source);
            }

            if (!Enum.GetNames(destEnumType).Contains(source.ToString()))
            {
                Type underlyingSourceType = Enum.GetUnderlyingType(sourceEnumType);
                var underlyingSourceValue = Convert.ChangeType(source, underlyingSourceType);

                return (TDestination)Enum.ToObject(destEnumType, underlyingSourceValue);
            }

            return (TDestination)Enum.Parse(destEnumType, Enum.GetName(sourceEnumType, source), true);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumToEnumMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return sourceEnumType != null && destEnumType != null;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }
    }

    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            bool toEnum = false;
            Type enumDestinationType = TypeHelper.GetEnumerationType(typeof(TDestination));
            var typePair = new TypePair(typeof (TSource), typeof (TDestination));
            EnumToUnderlyingTypeMapping(typePair, ref toEnum);

            if (toEnum && source != null)
            {
                return (TDestination)Enum.Parse(enumDestinationType, source.ToString(), true);
            }

            if (EnumToNullableTypeMapping(typePair))
            {
                return ConvertEnumToNullableType<TSource, TDestination>(source);
            }

            return (TDestination)Convert.ChangeType(source, typeof(TDestination), null);
        }

        internal static bool EnumToNullableTypeMapping(TypePair context)
        {
            if (!context.DestinationType.IsGenericType())
            {
                return false;
            }

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof(Nullable<>);
        }

        private static TDestination ConvertEnumToNullableType<TSource, TDestination>(TSource source)
        {
            if (source == null)
            {
                return default(TDestination);
            }

            var destType = Nullable.GetUnderlyingType(typeof(TDestination));
            return (TDestination)Convert.ChangeType(source, destType, null);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumToUnderlyingTypeMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            bool toEnum = false;
            return EnumToUnderlyingTypeMapping(context, ref toEnum);
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

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }
    }
    
}