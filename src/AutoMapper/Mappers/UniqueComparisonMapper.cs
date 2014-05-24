using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class UniqueComparisonMapper<TEnumerable> : IObjectMapper
        where TEnumerable : IList
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var sourceElementType = TypeHelper.GetElementType(context.SourceType);
            var destinationElementType = TypeHelper.GetElementType(context.DestinationType);
            var equivilencyExpression = GetEquivilentExpression(context);

            var sourceEnumerable = context.SourceValue as IEnumerable;
            var destinationCollection = (TEnumerable) context.DestinationValue;
            var destItems = destinationCollection.Cast<object>().ToList();
            var comparisionDictionary = sourceEnumerable.Cast<object>()
                .ToDictionary(s => s, s => destItems.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            foreach (var keypair in comparisionDictionary)
            {
                if (keypair.Value == null)
                    destinationCollection.Add(Mapper.Map(keypair.Key, sourceElementType, destinationElementType));
                else
                    Mapper.Map(keypair.Key, keypair.Value, sourceElementType, destinationElementType);
            }
            foreach (var removedItem in destItems.Except(comparisionDictionary.Values))
                destinationCollection.Remove(removedItem);

            return destinationCollection;
        }

        public virtual bool IsMatch(ResolutionContext context)
        {
            return context.SourceType.IsEnumerableType() && context.SourceValue != null
                && context.DestinationValue is TEnumerable
                && GetEquivilentExpression(context) != null;
        }

        private static IEquivilentExpression GetEquivilentExpression(ResolutionContext context)
        {
            return EquivilentExpressions.GetEquivilentExpression(TypeHelper.GetElementType(context.SourceType), TypeHelper.GetElementType(context.DestinationType));
        }
    }

    public static class EquivilentExpressions
    {
        private static readonly System.Collections.Generic.IDictionary<Type, System.Collections.Generic.IDictionary<Type,IEquivilentExpression>> EquivilentExpressionDictionary = new Dictionary<Type, System.Collections.Generic.IDictionary<Type, IEquivilentExpression>>();

        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            if (EquivilentExpressionDictionary.ContainsKey(destinationType))
                if (EquivilentExpressionDictionary[destinationType].ContainsKey(sourceType))
                    return EquivilentExpressionDictionary[destinationType][sourceType];
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

        private static void SetEquivlientExpressionForSource<TSource>(System.Collections.Generic.IDictionary<Type, IEquivilentExpression> destinationDictionary, IEquivilentExpression equivilentExpression)
        {
            GetOrGenerateSourceDictionary<TSource>(destinationDictionary);
            destinationDictionary[typeof(TSource)] = equivilentExpression;
        }

        private static System.Collections.Generic.IDictionary<Type, IEquivilentExpression> GetOrGenerateDefinitionDictionary<TDestination>()
        {
            if (!EquivilentExpressionDictionary.ContainsKey(typeof (TDestination)))
                EquivilentExpressionDictionary.Add(typeof (TDestination), new Dictionary<Type, IEquivilentExpression>());
            return EquivilentExpressionDictionary[typeof (TDestination)];
        }

        private static void GetOrGenerateSourceDictionary<TSource>(System.Collections.Generic.IDictionary<Type, IEquivilentExpression> destinationDictionary)
        {
            if (!destinationDictionary.ContainsKey(typeof (TSource)))
                destinationDictionary.Add(typeof (TSource), null);
        }
    }

    internal interface IEquivilentExpression
    {
        bool IsEquivlent(object source, object destination);
    }

    internal class EquivilentExpression<TSource,TDestination> : IEquivilentExpression 
        where TSource : class 
        where TDestination : class
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _equivilentExpression;

        public EquivilentExpression(Expression<Func<TSource,TDestination,bool>> equivilentExpression)
        {
            _equivilentExpression = equivilentExpression;
        }

        public bool IsEquivlent(object source, object destination)
        {
            if(!(source is TSource))
                throw new EquivilentExpressionNotOfTypeException(source.GetType(), typeof(TSource));
            if (!(destination is TDestination))
                throw new EquivilentExpressionNotOfTypeException(destination.GetType(), typeof(TDestination));
            return _equivilentExpression.Compile()(source as TSource, destination as TDestination);
        }

        private class EquivilentExpressionNotOfTypeException : Exception
        {
            private readonly Type _objectType;
            private readonly Type _expectedType;

            public EquivilentExpressionNotOfTypeException(Type objectType, Type expectedType)
            {
                _objectType = objectType;
                _expectedType = expectedType;
            }

            public override string Message
            {
                get { return string.Format("{0} does not equal or inherit from {1}", _objectType.Name, _expectedType.Name); }
            }
        }
    }
}