using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using Configuration;

    public class NullableSourceMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource? source, TDestination destination, ResolutionContext context) where TSource : struct
            => (source == null) ? context.Mapper.CreateObject<TDestination>() : context.Mapper.Map((TSource)source, destination);

        private static readonly MethodInfo MapMethodInfo = typeof(NullableSourceMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsNullableType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(Nullable.GetUnderlyingType(sourceExpression.Type), destExpression.Type), sourceExpression, destExpression, contextExpression);
        }
    }
}