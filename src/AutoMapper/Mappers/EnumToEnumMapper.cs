using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static Enum;
    public class EnumToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(EnumToEnumMapper).GetMethod(nameof(Map));
        public static TDestination Map<TSource, TDestination>(TSource source) where TDestination : struct
        {
            if (!IsDefined(typeof(TSource), source))
            {
                return ToDestination();
            }
            if (TryParse(source.ToString(), ignoreCase: true, out TDestination destination))
            {
                return destination;
            }
            return ToDestination();
            TDestination ToDestination() => (TDestination)ToObject(typeof(TDestination), source);
        }
        public bool IsMatch(TypePair context) => context.IsEnumToEnum();
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression) =>
            Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression);
    }
}