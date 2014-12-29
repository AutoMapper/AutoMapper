using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public interface IEquivilentExpression
    {
        bool IsEquivlent(object source, object destination);
    }

    public interface IToSingleSourceEquivalentExpression
    {
        Expression ToSingleSourceExpression(object destination);
    }
}