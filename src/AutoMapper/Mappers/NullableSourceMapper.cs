using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using Configuration;

    public class NullableSourceMapper : IObjectMapExpression
    {
        public static TDestination Map<TDestination>(TDestination? source)
            where TDestination : struct
        {
            return source.GetValueOrDefault();
        }

        private static readonly MethodInfo MapMethodInfo = typeof(NullableSourceMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.DestinationType).Invoke(null, new [] {context.SourceValue});
        }

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsNullableType() && !context.DestinationType.IsNullableType() && context.DestinationType == context.SourceType.GetTypeOfNullable();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type), sourceExpression);
        }
    }
}