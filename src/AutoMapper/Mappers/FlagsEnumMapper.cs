using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class FlagsEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EnumParseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
        public bool IsMatch(in TypePair context) => 
            context.IsEnumToEnum() && context.SourceType.Has<FlagsAttribute>() && context.DestinationType.Has<FlagsAttribute>();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
                ToType(
                    Call(EnumParseMethod,
                        Constant(destExpression.Type),
                        Call(sourceExpression, sourceExpression.Type.GetRuntimeMethod("ToString", Type.EmptyTypes)),
                        True
                    ),
                    destExpression.Type
                );
    }
}