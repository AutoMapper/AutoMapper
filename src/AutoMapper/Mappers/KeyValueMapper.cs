using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers;

using Execution;
public class KeyValueMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => IsKeyValue(context.SourceType) && IsKeyValue(context.DestinationType);
    public static bool IsKeyValue(Type type) => type.IsGenericType(typeof(KeyValuePair<,>));
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var sourceArguments = sourceExpression.Type.GenericTypeArguments;
        var destinationType = destExpression.Type;
        var destinationArguments = destinationType.GenericTypeArguments;
        var keys = new TypePair(sourceArguments[0], destinationArguments[0]);
        var values = new TypePair(sourceArguments[1], destinationArguments[1]);
        var mapKeys = configuration.MapExpression(profileMap, keys, ExpressionBuilder.Property(sourceExpression, "Key"));
        var mapValues = configuration.MapExpression(profileMap, values, ExpressionBuilder.Property(sourceExpression, "Value"));
        return Expression.New(destinationType.GetConstructor(destinationArguments), mapKeys, mapValues);
    }
}