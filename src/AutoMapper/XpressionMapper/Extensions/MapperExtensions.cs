using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using System.Reflection;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.XpressionMapper.Extensions
{
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpression<TDestDelegate>(this IMapper mapper, LambdaExpression expression)
            where TDestDelegate : LambdaExpression
        {
            if (expression == null)
                return default(TDestDelegate);

            var typeSourceFunc = expression.GetType().GetGenericArguments()[0];
            var typeDestFunc = typeof(TDestDelegate).GetGenericArguments()[0];

            var typeMappings = new Dictionary<Type, Type>()
                                            .AddTypeMappingsFromDelegates(typeSourceFunc, typeDestFunc);

            var visitor = new XpressionMapperVisitor(mapper == null ? Mapper.Configuration : mapper.ConfigurationProvider, typeMappings);
            var remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Resource.cantRemapExpression);

            return (TDestDelegate)Lambda(typeDestFunc, remappedBody, expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings));
        }

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpression<TSourceDelegate, TDestDelegate>(this IMapper mapper, TSourceDelegate expression)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression 
            => mapper.MapExpression<TDestDelegate>(expression);

        /// <summary>
        /// Maps an expression to be used as an "Include" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpressionAsInclude<TDestDelegate>(this IMapper mapper, LambdaExpression expression)
            where TDestDelegate : LambdaExpression
        {
            if (expression == null)
                return null;

            var typeSourceFunc = expression.GetType().GetGenericArguments()[0];
            var typeDestFunc = typeof(TDestDelegate).GetGenericArguments()[0];

            var typeMappings = new Dictionary<Type, Type>()
                                            .AddTypeMappingsFromDelegates(typeSourceFunc, typeDestFunc);

            XpressionMapperVisitor visitor = new MapIncludesVisitor(mapper == null ? Mapper.Configuration : mapper.ConfigurationProvider, typeMappings);
            var remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Resource.cantRemapExpression);

            return (TDestDelegate)Lambda(typeDestFunc, remappedBody, expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings));
        }

        /// <summary>
        /// Maps an expression to be used as an "Include" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpressionAsInclude<TSourceDelegate, TDestDelegate>(this IMapper mapper, TSourceDelegate expression)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression 
            => mapper.MapExpressionAsInclude<TDestDelegate>(expression);

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapExpressionList<TSourceDelegate, TDestDelegate>(this IMapper mapper, ICollection<TSourceDelegate> collection)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression 
            => collection?.Select(mapper.MapExpression<TSourceDelegate, TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapExpressionList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression 
            => collection?.Select(mapper.MapExpression<TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapIncludesList<TSourceDelegate, TDestDelegate>(this IMapper mapper, ICollection<TSourceDelegate> collection)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression 
            => collection?.Select(mapper.MapExpressionAsInclude<TSourceDelegate, TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapIncludesList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression 
            => collection?.Select(mapper.MapExpressionAsInclude<TDestDelegate>).ToList();

        /// <summary>
        /// Takes a list of parameters from the source lamda expression and returns a list of parameters for the destination lambda expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        public static List<ParameterExpression> GetDestinationParameterExpressions(this LambdaExpression expression, MapperInfoDictionary infoDictionary, Dictionary<Type, Type> typeMappings)
        {
            foreach (var p in expression.Parameters.Where(p => !infoDictionary.ContainsKey(p)))
            {
                infoDictionary.Add(p, typeMappings);
            }

            return expression.Parameters.Select(p => infoDictionary[p].NewParameter).ToList();
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the generic arguments.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping<TSource, TDest>(this Dictionary<Type, Type> typeMappings)
            => typeMappings == null
                ? throw new ArgumentException(Resource.typeMappingsDictionaryIsNull)
                : typeMappings.AddTypeMapping(typeof(TSource), typeof(TDest));

        private static bool HasUnderlyingType(this Type type)
        {
            return (type.IsGenericType() && typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) || type.IsArray;
        }

        private static void AddUnderlyingTypes(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            var sourceArguments = !sourceType.HasUnderlyingType()
                                    ? new List<Type>()
                                    : ElementTypeHelper.GetElementTypes(sourceType).ToList();

            var destArguments = !destType.HasUnderlyingType()
                                    ? new List<Type>()
                                    : ElementTypeHelper.GetElementTypes(destType).ToList();

            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Resource.invalidArgumentCount);

            sourceArguments.Aggregate(typeMappings, (dic, next) =>
            {
                if (!dic.ContainsKey(next) && next != destArguments[sourceArguments.IndexOf(next)])
                    dic.AddTypeMapping(next, destArguments[sourceArguments.IndexOf(next)]);

                return dic;
            });
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the arguments.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            if (sourceType.GetTypeInfo().IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Expression<>))
            {
                sourceType = sourceType.GetGenericArguments()[0];
                destType = destType.GetGenericArguments()[0];
            }

            if (!typeMappings.ContainsKey(sourceType) && sourceType != destType)
            {
                typeMappings.Add(sourceType, destType);
                if (typeof(Delegate).IsAssignableFrom(sourceType))
                    typeMappings.AddTypeMappingsFromDelegates(sourceType, destType);
                else
                    typeMappings.AddUnderlyingTypes(sourceType, destType);
            }

            return typeMappings;
        }

        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            var sourceArguments = sourceType.GetGenericArguments().ToList();
            var destArguments = destType.GetGenericArguments().ToList();

            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Resource.invalidArgumentCount);

            return sourceArguments.Aggregate(typeMappings, (dic, next) =>
            {
                if (!dic.ContainsKey(next) && next != destArguments[sourceArguments.IndexOf(next)])
                    dic.AddTypeMapping(next, destArguments[sourceArguments.IndexOf(next)]);

                return dic;
            });
        }
    }
}
