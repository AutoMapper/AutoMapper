using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    internal class UserDefinedEquivilentExpressions : IGenerateEquivilentExpressions
    {
        private readonly IDictionary<Type, IDictionary<Type, IEquivilentExpression>> _equivilentExpressionDictionary = new Dictionary<Type, IDictionary<Type, IEquivilentExpression>>();

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivlelentExpression(sourceType, destinationType) != null;
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivlelentExpression(sourceType, destinationType);
        }

        private IEquivilentExpression GetEquivlelentExpression(Type srcType, Type destType)
        {
            if (_equivilentExpressionDictionary.ContainsKey(destType))
                if (_equivilentExpressionDictionary[destType].ContainsKey(srcType))
                    return _equivilentExpressionDictionary[destType][srcType];
            return null;
        }

        internal void AddEquivilencyExpression<TSource, TDestination>(Expression<Func<TSource, TDestination, bool>> equivilentExpression)
            where TSource : class
            where TDestination : class
        {
            var destinationDictionary = GetOrGenerateDefinitionDictionary<TDestination>();
            SetEquivlientExpressionForSource<TSource>(destinationDictionary, new EquivilentExpression<TSource, TDestination>(equivilentExpression));
        }

        private void SetEquivlientExpressionForSource<TSource>(IDictionary<Type, IEquivilentExpression> destinationDictionary, IEquivilentExpression equivilentExpression)
        {
            GetOrGenerateSourceDictionary<TSource>(destinationDictionary);
            destinationDictionary[typeof(TSource)] = equivilentExpression;
        }

        private IDictionary<Type, IEquivilentExpression> GetOrGenerateDefinitionDictionary<TDestination>()
        {
            if (!_equivilentExpressionDictionary.ContainsKey(typeof(TDestination)))
                _equivilentExpressionDictionary.Add(typeof(TDestination), new Dictionary<Type, IEquivilentExpression>());
            return _equivilentExpressionDictionary[typeof(TDestination)];
        }

        private void GetOrGenerateSourceDictionary<TSource>(IDictionary<Type, IEquivilentExpression> destinationDictionary)
        {
            if (!destinationDictionary.ContainsKey(typeof(TSource)))
                destinationDictionary.Add(typeof(TSource), null);
        }
    }
}