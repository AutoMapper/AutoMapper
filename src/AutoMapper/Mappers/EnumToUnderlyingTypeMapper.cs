using System;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using Convert = System.Convert;

namespace AutoMapper.Mappers
{
    using static ExpressionExtensions;

    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        private static readonly MethodInfo ChangeTypeMethod = Method(() => Convert.ChangeType(null, typeof(object)));

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);

            return sourceEnumType != null && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(sourceEnumType));
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Condition(
                Equal(ToObject(sourceExpression), Constant(null)),
                Default(destExpression.Type),
                ToType(
                    Call(ChangeTypeMethod, ToObject(sourceExpression),
                        Constant(Nullable.GetUnderlyingType(destExpression.Type) ?? destExpression.Type)),
                    destExpression.Type
                ));
    }
    
}