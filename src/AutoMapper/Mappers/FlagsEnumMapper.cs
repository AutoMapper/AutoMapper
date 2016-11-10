using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using System;
    using System.Linq;

    public class FlagsEnumMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source, Func<TDestination> ifNull)
        {
            if (source == null)
                return ifNull();

            Type enumDestType = TypeHelper.GetEnumerationType(typeof(TDestination));
            return (TDestination)Enum.Parse(enumDestType, source.ToString(), true);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FlagsEnumMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return sourceEnumType != null
                   && destEnumType != null
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, Expression.Constant(CollectionMapperExtensions.Constructor(destExpression.Type)));
        }
    }
}