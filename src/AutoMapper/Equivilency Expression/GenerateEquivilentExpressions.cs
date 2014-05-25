using System;

namespace AutoMapper.EquivilencyExpression
{
    internal class GenerateEquivilentExpressions : IGenerateEquivilentExpressions
    {
        internal static IGenerateEquivilentExpressions BadValue { get; private set; }

        static GenerateEquivilentExpressions()
        {
            BadValue = new GenerateEquivilentExpressions();
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }
    }
}