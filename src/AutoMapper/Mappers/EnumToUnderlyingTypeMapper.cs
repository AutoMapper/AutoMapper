using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;

    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null)
            {
                return default(TDestination);
            }

            var destinationType = Nullable.GetUnderlyingType(typeof(TDestination)) ?? typeof(TDestination);

            return (TDestination)Convert.ChangeType(source, destinationType, null);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumToUnderlyingTypeMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);

            return sourceEnumType != null && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(sourceEnumType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }
    }
    
}