using System;
using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers;

using static Expression;
public class ConvertMapper : IObjectMapper
{
    public static bool IsPrimitive(Type type) => type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    public bool IsMatch(TypePair types) => (types.SourceType == typeof(string) && types.DestinationType == typeof(DateTime)) || 
        (IsPrimitive(types.SourceType) && IsPrimitive(types.DestinationType));
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var convertMethod = typeof(Convert).GetMethod("To" + destExpression.Type.Name, new[] { sourceExpression.Type });
        return Call(convertMethod, sourceExpression);
    }
}