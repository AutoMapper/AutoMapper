using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionExtensions;

    public class UnderlyingTypeToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumToObject = Method(() => Enum.ToObject(typeof(object), null));

        public bool IsMatch(TypePair context)
        {
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return destEnumType != null && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(destEnumType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Condition(
                Equal(ToObject(sourceExpression), Constant(null)),
                Default(destExpression.Type),
                ToType(
                    Call(EnumToObject, Constant(Nullable.GetUnderlyingType(destExpression.Type) ?? destExpression.Type),
                        ToObject(sourceExpression)),
                    destExpression.Type
                ));
        }
    }
}