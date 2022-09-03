using AutoMapper.Execution;
using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers;

using static Expression;
public class ToStringMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType == typeof(string);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var sourceType = sourceExpression.Type;
        var toStringCall = Call(sourceExpression, ExpressionBuilder.ObjectToString);
        return sourceType.IsEnum ? StringToEnumMapper.CheckEnumMember(sourceExpression, sourceType, toStringCall) : toStringCall;
    }
}