using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    public class TypeConverterMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(TypeConverterMapper).GetStaticMethod(nameof(Map));
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
            return default;
        }
        public bool IsMatch(in TypePair context)
        {
            var sourceTypeConverter = GetTypeConverter(context.SourceType);
            var destTypeConverter = GetTypeConverter(context.DestinationType);
            return sourceTypeConverter.CanConvertTo(context.DestinationType) || destTypeConverter.CanConvertFrom(context.SourceType);
        }
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Call(MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        private static TypeConverter GetTypeConverter(Type type) => TypeDescriptor.GetConverter(type);
    }
}