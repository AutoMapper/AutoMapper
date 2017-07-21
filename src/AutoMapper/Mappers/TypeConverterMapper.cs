using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
#if NETSTANDARD1_3 || NET45 || NET40
    public class TypeConverterMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source)
        {
            var typeConverter = GetTypeConverter(typeof(TSource));

            if (typeConverter.CanConvertTo(typeof(TDestination)))
            {
                return (TDestination)typeConverter.ConvertTo(source, typeof(TDestination));
            }

            typeConverter = GetTypeConverter(typeof(TDestination));
            if (typeConverter.CanConvertFrom(typeof(TSource)))
            {
                return (TDestination)typeConverter.ConvertFrom(source);
            }

            return default(TDestination);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(TypeConverterMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            var sourceTypeConverter = GetTypeConverter(context.SourceType);
            var destTypeConverter = GetTypeConverter(context.DestinationType);

            return sourceTypeConverter.CanConvertTo(context.DestinationType) || destTypeConverter.CanConvertFrom(context.SourceType);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type),
                sourceExpression);

        private static TypeConverter GetTypeConverter(Type type) => TypeDescriptor.GetConverter(type);
    }
#endif
}
