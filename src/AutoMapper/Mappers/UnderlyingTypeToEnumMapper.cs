using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class UnderlyingTypeToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumToObject = Method(() => Enum.ToObject(typeof(object), null));

        public bool IsMatch(TypePair context)
        {
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);

            return destEnumType != null && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(destEnumType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
                ToType(
                    Call(EnumToObject, Constant(destExpression.Type),
                        ToObject(sourceExpression)),
                    destExpression.Type
                );
    }
}