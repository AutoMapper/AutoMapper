using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Configuration;

    public class TypeConverterMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source)
        {
            var typeConverter = GetTypeConverter(typeof(TSource));

            if (typeConverter.CanConvertTo(typeof(TDestination)))
            {
                return (TDestination)typeConverter.ConvertTo(source, typeof(TDestination));
            }

            if (typeof(TDestination).IsNullableType() &&
                typeConverter.CanConvertTo(Nullable.GetUnderlyingType(typeof(TDestination))))
            {
                return (TDestination)typeConverter.ConvertTo(source, Nullable.GetUnderlyingType(typeof(TDestination)));
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

            return sourceTypeConverter.CanConvertTo(context.DestinationType) ||
                   (context.DestinationType.IsNullableType() &&
                    sourceTypeConverter.CanConvertTo(Nullable.GetUnderlyingType(context.DestinationType)) ||
                    destTypeConverter.CanConvertFrom(context.SourceType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
    }
}
