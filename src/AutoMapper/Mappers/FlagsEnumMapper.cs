using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class FlagsEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumParseMethod = Method(() => Enum.Parse(null, null, true));

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = ElementTypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);

            return sourceEnumType != null
                   && destEnumType != null
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
                ToType(
                    Call(EnumParseMethod,
                        Constant(destExpression.Type),
                        Call(sourceExpression, sourceExpression.Type.GetDeclaredMethod("ToString", Type.EmptyTypes)),
                        Constant(true)
                    ),
                    destExpression.Type
                );
    }
}