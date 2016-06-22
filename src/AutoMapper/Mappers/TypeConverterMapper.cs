using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Configuration;

    public class TypeConverterMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination ifNull)
        {
            if (source == null)
                return ifNull;
            return GetConverter<TSource, TDestination>(source);
        }

        private static TDestination GetConverter<TSource, TDestination>(TSource source)
        {
            TypeConverter typeConverter = GetTypeConverter(typeof(TSource));
            if (typeConverter.CanConvertTo(typeof(TDestination)))
                return (TDestination)typeConverter.ConvertTo(source, typeof(TDestination));
            if (typeof(TDestination).IsNullableType() &&
                typeConverter.CanConvertTo(Nullable.GetUnderlyingType(typeof(TDestination))))
                return (TDestination)typeConverter.ConvertTo(source, Nullable.GetUnderlyingType(typeof(TDestination)));

            typeConverter = GetTypeConverter(typeof(TDestination));
            if (typeConverter.CanConvertFrom(typeof(TSource)))
                return (TDestination)typeConverter.ConvertFrom(source);

            return default(TDestination);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(TypeConverterMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            var sourceTypeConverter = GetTypeConverter(context.SourceType);
            var destTypeConverter = GetTypeConverter(context.DestinationType);

            return sourceTypeConverter.CanConvertTo(context.DestinationType) ||
                   (context.DestinationType.IsNullableType() &&
                    sourceTypeConverter.CanConvertTo(Nullable.GetUnderlyingType(context.DestinationType)) ||
                    destTypeConverter.CanConvertFrom(context.SourceType));
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, ExpressionExtensions.ToType(DelegateFactory.GenerateConstructorExpression(destExpression.Type), destExpression.Type));
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
    }
}
