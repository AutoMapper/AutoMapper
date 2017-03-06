using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using System;
    using System.Linq;
    using static Expression;
    using static ExpressionExtensions;

    public class FlagsEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumParseMethod = Method(() => Enum.Parse(null, null, true));

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return sourceEnumType != null
                   && destEnumType != null
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Condition(
                Equal(ToObject(sourceExpression), Constant(null)),
                Default(destExpression.Type),
                ToType(
                    Call(EnumParseMethod, 
                        Constant(Nullable.GetUnderlyingType(destExpression.Type) ?? destExpression.Type),
                        Call(sourceExpression, sourceExpression.Type.GetDeclaredMethod("ToString")),
                        Constant(true)
                    ),
                    destExpression.Type
                ));
        }
    }
}