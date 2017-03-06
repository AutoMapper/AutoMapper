using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Linq;
    using Configuration;
    using static Expression;

    public class MultidimensionalArrayMapper : IObjectMapper
    {
        private static Array Map<TDestination, TSource, TSourceElement>(TSource source, ResolutionContext context, ProfileMap profileMap)
            where TSource : IEnumerable
        {
            if (source == null && profileMap.AllowNullCollections)
                return null;

            var destElementType = TypeHelper.GetElementType(typeof(TDestination));

            if (source != null && typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), destElementType);
                if (elementTypeMap == null)
                    return source as Array;
            }

            var sourceList = (IEnumerable)source ?? new List<TSource>();
            var sourceArray = source as Array;
            var destinationArray = sourceArray == null 
                ? Array.CreateInstance(destElementType, sourceList.Cast<object>().Count()) 
                : Array.CreateInstance(destElementType, Enumerable.Range(0, sourceArray.Rank).Select(sourceArray.GetLength).ToArray());

            var filler = new MultidimensionalArrayFiller(destinationArray);
            foreach (var item in sourceList)
            {
                filler.NewValue(context.Map(item, null, typeof(TSourceElement), destElementType));
            }
            return destinationArray;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(MultidimensionalArrayMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsArray && context.DestinationType.GetArrayRank() > 1 && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, 
                MapMethodInfo.MakeGenericMethod(destExpression.Type, sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type)), 
                sourceExpression, 
                contextExpression,
                Constant(profileMap));
        }

        public class MultidimensionalArrayFiller
        {
            private readonly int[] _indices;
            private readonly Array _destination;

            public MultidimensionalArrayFiller(Array destination)
            {
                _indices = new int[destination.Rank];
                _destination = destination;
            }

            public void NewValue(object value)
            {
                int dimension = _destination.Rank - 1;
                bool changedDimension = false;
                while (_indices[dimension] == _destination.GetLength(dimension))
                {
                    _indices[dimension] = 0;
                    dimension--;
                    if (dimension < 0)
                    {
                        throw new InvalidOperationException("Not enough room in destination array " + _destination);
                    }
                    _indices[dimension]++;
                    changedDimension = true;
                }
                _destination.SetValue(value, _indices);
                if (changedDimension)
                {
                    _indices[dimension + 1]++;
                }
                else
                {
                    _indices[dimension]++;
                }
            }
        }
    }

}