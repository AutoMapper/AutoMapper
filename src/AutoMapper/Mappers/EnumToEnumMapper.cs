using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Mappers.Internal;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class EnumToEnumMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            var sourceEnumType = ElementTypeHelper.GetEnumerationType(typeof(TSource));
            var destEnumType = ElementTypeHelper.GetEnumerationType(typeof(TDestination));

            if (!Enum.IsDefined(sourceEnumType, source))
            {
                return (TDestination)Enum.ToObject(destEnumType, source);
            }

            if (!Enum.GetNames(destEnumType).Contains(source.ToString()))
            {
                var underlyingSourceType = Enum.GetUnderlyingType(sourceEnumType);
                var underlyingSourceValue = System.Convert.ChangeType(source, underlyingSourceType);

                return (TDestination)Enum.ToObject(destEnumType, underlyingSourceValue);
            }

            return (TDestination)Enum.Parse(destEnumType, Enum.GetName(sourceEnumType, source), true);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumToEnumMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = ElementTypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);
            return sourceEnumType != null && destEnumType != null;
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), 
                sourceExpression);
    }
}