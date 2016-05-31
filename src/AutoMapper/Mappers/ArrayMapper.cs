using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using Configuration;

    public class ArrayMapper : IObjectMapExpression
    {
        public static TDestinationElement[] Map<TSource, TSourceElement, TDestinationElement>(TSource source, ResolutionContext context)
            where TSource : IEnumerable
        {
            if (source == null)
                return context.Mapper.ShouldMapSourceCollectionAsNull(context) ? null : new TDestinationElement[0];
            
            if (context.DestinationType.IsAssignableFrom(context.SourceType))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), typeof(TDestinationElement));
                if (elementTypeMap == null)
                    return source as TDestinationElement[];
            }

            var itemContext = new ResolutionContext(context);

            return source.Cast<TSourceElement>()
                .Select(item => (TDestinationElement) itemContext.Map(item, null, typeof(TSourceElement), typeof(TDestinationElement)))
                .ToArray();
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType, (IEnumerable)context.SourceValue), TypeHelper.GetElementType(context.DestinationType, (IEnumerable)context.DestinationValue)).Invoke(null, new [] { context.SourceValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), TypeHelper.GetElementType(destExpression.Type)), sourceExpression, contextExpression );
        }
    }
}