using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using System;
    using System.Linq;

    public class FlagsEnumMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TDestination>(TSource source, ResolutionContext context)
            where TDestination : struct
        {
            Type enumDestType = TypeHelper.GetEnumerationType(typeof(TDestination));

            if (source == null)
            {
                return (TDestination)(context.ConfigurationProvider.AllowNullDestinationValues
                        ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                        : ObjectCreator.CreateObject(typeof(TDestination)));
            }

            return (TDestination)Enum.Parse(enumDestType, source.ToString(), true);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FlagsEnumMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, context.DestinationType).Invoke(null, new [] { context.SourceValue, context});
        }

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return sourceEnumType != null
                   && destEnumType != null
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, contextExpression);
        }
    }
}