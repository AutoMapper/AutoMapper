using System;

namespace AutoMapper.EquivilencyExpression
{
    public interface IGenerateEquivilentExpressions
    {
        bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType);
        IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType);
    }
}