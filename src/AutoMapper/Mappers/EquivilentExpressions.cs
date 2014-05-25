using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public static class EquivilentExpressions
    {
        private static readonly IDictionary<Type, IDictionary<Type,IEquivilentExpression>> EquivilentExpressionDictionary = new Dictionary<Type, IDictionary<Type, IEquivilentExpression>>();
        private static readonly IList<IGenerateEquivilentExpressions> GenerateEquivilentExpressions = new List<IGenerateEquivilentExpressions>();

        public static IList<IGenerateEquivilentExpressions> GenerateEquality
        {
            get { return GenerateEquivilentExpressions; }
        }

        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            if (EquivilentExpressionDictionary.ContainsKey(destinationType))
                if (EquivilentExpressionDictionary[destinationType].ContainsKey(sourceType))
                    return EquivilentExpressionDictionary[destinationType][sourceType];
            var generate =
                GenerateEquivilentExpressions.FirstOrDefault(g => g.CanGenerateEquivilentExpression(sourceType, destinationType));
            if (generate != null)
                return generate.GeneratEquivilentExpression(sourceType, destinationType);
            return null;
        }

        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression) 
            where TSource : class 
            where TDestination : class
        {
            AddEquivilencyExpression(equivilentExpression);
            return mappingExpression;
        }

        private static void AddEquivilencyExpression<TSource, TDestination>(Expression<Func<TSource, TDestination, bool>> equivilentExpression) 
            where TSource : class 
            where TDestination : class
        {
            var destinationDictionary = GetOrGenerateDefinitionDictionary<TDestination>();
            SetEquivlientExpressionForSource<TSource>(destinationDictionary, new EquivilentExpression<TSource,TDestination>(equivilentExpression));
        }

        private static void SetEquivlientExpressionForSource<TSource>(IDictionary<Type, IEquivilentExpression> destinationDictionary, IEquivilentExpression equivilentExpression)
        {
            GetOrGenerateSourceDictionary<TSource>(destinationDictionary);
            destinationDictionary[typeof(TSource)] = equivilentExpression;
        }

        private static IDictionary<Type, IEquivilentExpression> GetOrGenerateDefinitionDictionary<TDestination>()
        {
            if (!EquivilentExpressionDictionary.ContainsKey(typeof (TDestination)))
                EquivilentExpressionDictionary.Add(typeof (TDestination), new Dictionary<Type, IEquivilentExpression>());
            return EquivilentExpressionDictionary[typeof (TDestination)];
        }

        private static void GetOrGenerateSourceDictionary<TSource>(IDictionary<Type, IEquivilentExpression> destinationDictionary)
        {
            if (!destinationDictionary.ContainsKey(typeof (TSource)))
                destinationDictionary.Add(typeof (TSource), null);
        }
    }

    public interface IGenerateEquivilentExpressions
    {
        bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType);
        IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType);
    }

    public class GenerateEquivilentExpressionsBasedOnProperties : IGenerateEquivilentExpressions
    {
        private readonly IEnumerable<Func<PropertyInfo, PropertyInfo, bool>> _propertiesMatch;
        private readonly IDictionary<KeyValuePair<Type,Type>,IDictionary<PropertyInfo,PropertyInfo>> _matchingProperties = new Dictionary<KeyValuePair<Type, Type>, IDictionary<PropertyInfo, PropertyInfo>>(); 

        public GenerateEquivilentExpressionsBasedOnProperties(params Func<PropertyInfo, PropertyInfo, bool>[] propertiesMatch)
        {
            _propertiesMatch = propertiesMatch;
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetPropertyInfoMatching(sourceType, destinationType).Any();
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            var a = GetPropertyInfoMatching(sourceType, destinationType);
            var sourceExpression = Expression.Parameter(sourceType, "source");
            var destinationExpression = Expression.Parameter(destinationType, "destination");

            var firstPair = a.First();
            var exp = Expression.Equal(Expression.Property(sourceExpression, firstPair.Key), Expression.Property(destinationExpression, firstPair.Value));
            foreach (var propertyInfo in a.Skip(1))
            {
                var exp2 = Expression.Equal(Expression.Property(sourceExpression, propertyInfo.Key), Expression.Property(destinationExpression, propertyInfo.Value));
                exp = Expression.And(exp, exp2);
            }

            var expr = Expression.Lambda(exp, sourceExpression, destinationExpression);
            var genericExpressionType = typeof (EquivilentExpression<,>).MakeGenericType(sourceType,destinationType);
            var equivilientExpression = Activator.CreateInstance(genericExpressionType, expr) as IEquivilentExpression;
            return equivilientExpression;
        }

        private Dictionary<PropertyInfo, PropertyInfo> GetPropertyInfoMatching(Type sourceType, Type destinationType)
        {
            var a = new Dictionary<PropertyInfo, PropertyInfo>();
            foreach (var sourceProperty in sourceType.GetProperties())
                foreach (
                    var destinationProperty in
                        destinationType.GetProperties()
                            .Where(destinationProperty => _propertiesMatch.Any(p => p(sourceProperty, destinationProperty))))
                    a.Add(sourceProperty, destinationProperty);
            return a;
        }
    }
}