using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class FlagsEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumParseMethod = Method(() => Enum.Parse(null, null, true));

        public bool IsMatch(TypePair context) => 
            context.IsEnumToEnum() && context.SourceType.Has<FlagsAttribute>() && context.DestinationType.Has<FlagsAttribute>();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
                ToType(
                    Call(EnumParseMethod,
                        Constant(destExpression.Type),
                        Call(sourceExpression, sourceExpression.Type.GetRuntimeMethod("ToString", Type.EmptyTypes)),
                        Constant(true)
                    ),
                    destExpression.Type
                );
    }
}