using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
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

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
        }
    }
}