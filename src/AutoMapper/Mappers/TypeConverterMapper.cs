using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if !PORTABLE
namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Configuration;

    public class TypeConverterMapper : IObjectMapExpression
    {
        private static TDestination Map<TSource, TDestination>(TSource source, ResolutionContext context)
        {
            if (source == null)
            {
                return (TDestination)(context.ConfigurationProvider.AllowNullDestinationValues
                 ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                 : ObjectCreator.CreateObject(typeof(TDestination)));
            }
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

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, context.DestinationType).Invoke(null, new[] { context.SourceValue, context });
        }

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
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, contextExpression);
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
    }
}
#endif