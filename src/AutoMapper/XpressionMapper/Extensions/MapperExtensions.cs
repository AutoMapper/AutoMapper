using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.XpressionMapper.Structures;
using System.Reflection;

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

            if (expression.GetType().GetGenericTypeDefinition() != typeof(Expression<>)
                || typeof(TDestDelegate).GetGenericTypeDefinition() != typeof(Expression<>))
            {
                throw new ArgumentException(Resource.mustBeExpressions);
            }

            Type typeSourceFunc = expression.GetType().GetGenericArguments()[0];
            Type typeDestFunc = typeof(TDestDelegate).GetGenericArguments()[0];

            Dictionary<Type, Type> typeMappings = new Dictionary<Type, Type>()
                                            .AddTypeMappingsFromDelegates(typeSourceFunc, typeDestFunc);

            XpressionMapperVisitor visitor = new XpressionMapperVisitor(mapper == null ? Mapper.Configuration : mapper.ConfigurationProvider, typeMappings);
            Expression remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Resource.cantRemapExpression);

            return (TDestDelegate)Expression.Lambda(typeDestFunc, remappedBody, expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings));
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
        {
            return mapper.MapExpression<TDestDelegate>(expression);
        }

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

            if (expression.GetType().GetGenericTypeDefinition() != typeof(Expression<>)
                || typeof(TDestDelegate).GetGenericTypeDefinition() != typeof(Expression<>))
            {
                throw new ArgumentException(Resource.mustBeExpressions);
            }

            Type typeSourceFunc = expression.GetType().GetGenericArguments()[0];
            Type typeDestFunc = typeof(TDestDelegate).GetGenericArguments()[0];

            Dictionary<Type, Type> typeMappings = new Dictionary<Type, Type>()
                                            .AddTypeMappingsFromDelegates(typeSourceFunc, typeDestFunc);

            XpressionMapperVisitor visitor = new MapIncludesVisitor(mapper == null ? Mapper.Configuration : mapper.ConfigurationProvider, typeMappings);
            Expression remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Resource.cantRemapExpression);

            return (TDestDelegate)Expression.Lambda(typeDestFunc, remappedBody, expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings));
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
        {
            return mapper.MapExpressionAsInclude<TDestDelegate>(expression);
        }

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
        {
            if (collection == null)
                return null;

            return collection.Select(item => mapper.MapExpression<TSourceDelegate, TDestDelegate>(item)).ToList();
        }

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapExpressionList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression
        {
            if (collection == null)
                return null;

            return collection.Select(item => mapper.MapExpression<TDestDelegate>(item)).ToList();
        }

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
        {
            if (collection == null)
                return null;

            return collection.Select(item => mapper.MapExpressionAsInclude<TSourceDelegate, TDestDelegate>(item)).ToList();
        }

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapIncludesList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression
        {
            if (collection == null)
                return null;

            return collection.Select(item => mapper.MapExpressionAsInclude<TDestDelegate>(item)).ToList();
        }

        /// <summary>
        /// Takes a list of parameters from the source lamda expression and returns a list of parameters for the destination lambda expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        public static List<ParameterExpression> GetDestinationParameterExpressions(this LambdaExpression expression, MapperInfoDictionary infoDictionary, Dictionary<Type, Type> typeMappings)
        {
            foreach (ParameterExpression p in expression.Parameters)
            {
                if (!infoDictionary.ContainsKey(p))//possible cases where the parameter has not been used in the body.
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
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            return typeMappings.AddTypeMapping(typeof(TSource), typeof(TDest));
        }

        private static void AddUnderlyimgGenericTypes(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            List<Type> sourceArguments = sourceType.GetUnderlyingGenericTypes();
            List<Type> destArguments = destType.GetUnderlyingGenericTypes();

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
                    typeMappings.AddUnderlyimgGenericTypes(sourceType, destType);
            }

            return typeMappings;
        }

        #region Private Methods
        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates<TSourceDelegate, TDestDelegate>(this Dictionary<Type, Type> typeMappings)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            return typeMappings.AddTypeMappingsFromDelegates(typeof(TSourceDelegate), typeof(TDestDelegate));
        }

        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            List<Type> sourceArguments = sourceType.GetGenericArguments().ToList();
            List<Type> destArguments = destType.GetGenericArguments().ToList();

            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Resource.invalidArgumentCount);

            return sourceArguments.Aggregate(typeMappings, (dic, next) =>
            {
                if (!dic.ContainsKey(next) && next != destArguments[sourceArguments.IndexOf(next)])
                    dic.AddTypeMapping(next, destArguments[sourceArguments.IndexOf(next)]);

                return dic;
            });
        }
        #endregion Private Methods
    }
}
