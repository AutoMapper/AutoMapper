using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;

    public class MultidimensionalArrayMapper : IObjectMapper
    {
        private static Array Map<TDestination, TSource, TSourceElement>(TSource source, ResolutionContext context, IConfigurationProvider configurationProvider)
            where TSource : IEnumerable
        {
            var destElementType = ElementTypeHelper.GetElementType(typeof(TDestination));

            if (typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                var elementTypeMap = configurationProvider.ResolveTypeMap(typeof(TSourceElement), destElementType);
                if (elementTypeMap == null)
                    return source as Array;
            }

            var sourceList = (IEnumerable)source;
            var sourceArray = source as Array;
            var destinationArray = sourceArray == null 
                ? Array.CreateInstance(destElementType, sourceList.Cast<object>().Count()) 
                : Array.CreateInstance(destElementType, Enumerable.Range(0, sourceArray.Rank).Select(sourceArray.GetLength).ToArray());

            var filler = new MultidimensionalArrayFiller(destinationArray);
            foreach (var item in sourceList)
            {
                filler.NewValue(context.Map(item, null, typeof(TSourceElement), destElementType, null));
            }
            return destinationArray;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(MultidimensionalArrayMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context) => 
            context.DestinationType.IsArray 
            && context.DestinationType.GetArrayRank() > 1 
            && context.SourceType.IsEnumerableType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Call(null,
                MapMethodInfo.MakeGenericMethod(destExpression.Type, sourceExpression.Type,
                    ElementTypeHelper.GetElementType(sourceExpression.Type)),
                sourceExpression,
                contextExpression,
                Constant(configurationProvider));

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
                var dimension = _destination.Rank - 1;
                var changedDimension = false;
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
