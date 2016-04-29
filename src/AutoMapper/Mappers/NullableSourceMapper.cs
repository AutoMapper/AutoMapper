using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using Configuration;

    public class NullableSourceMapper : IObjectMapper, IObjectMapExpression
    {
        public static TDestination Map<TDestination>(object source, ResolutionContext context)
            where TDestination : class
        {
            return (TDestination) source ?? (TDestination) (context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof (TDestination))
                : ObjectCreator.CreateObject(typeof (TDestination)));
        }

        private static readonly MethodInfo MapMethodInfo = typeof(NullableSourceMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.DestinationType).Invoke(null, new [] {context.SourceValue, context});
        }

        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsNullableType() && !context.DestinationType.IsNullableType();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type), sourceExpression, contextExpression);
        }
    }
}