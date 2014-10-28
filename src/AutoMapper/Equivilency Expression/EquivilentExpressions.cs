using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public static class EquivilentExpressions
    {
        private static readonly UserDefinedEquivilentExpressions UserDefinedEquivilentExpressions = new UserDefinedEquivilentExpressions();
        private static readonly IList<IGenerateEquivilentExpressions> GenerateEquivilentExpressions = new List<IGenerateEquivilentExpressions>{UserDefinedEquivilentExpressions};

        /// <summary>
        /// Equality List for Generating Equality Comparisons between two types
        /// </summary>
        public static ICollection<IGenerateEquivilentExpressions> GenerateEquality
        {
            get { return GenerateEquivilentExpressions; }
        }

        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            var generate = GenerateEquality.FirstOrDefault(g => g.CanGenerateEquivilentExpression(sourceType, destinationType));
            if (generate == null)
                return null;
            return generate.GeneratEquivilentExpression(sourceType, destinationType);
        }

        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivilentExpression">Equivilent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression) 
            where TSource : class 
            where TDestination : class
        {
            UserDefinedEquivilentExpressions.AddEquivilencyExpression(equivilentExpression);
            return mappingExpression;
        }
    }
}