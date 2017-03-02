using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class UnderlyingTypeToEnumMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source)
        {
            return source == null
                ? default(TDestination)
                : (TDestination)
                Enum.ToObject(Nullable.GetUnderlyingType(typeof(TDestination)) ?? typeof(TDestination), source);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(UnderlyingTypeToEnumMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return destEnumType != null && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(destEnumType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }
    }
}