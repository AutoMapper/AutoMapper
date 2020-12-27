using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static TypeDescriptor;
    public class TypeConverterMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(TypeConverterMapper).GetStaticMethod(nameof(Map));
        private static object Map(object source, Type sourceType, Type destinationType)
        {
            var typeConverter = GetConverter(sourceType);
            return typeConverter.CanConvertTo(destinationType) ? 
                typeConverter.ConvertTo(source, destinationType) : GetConverter(destinationType).ConvertFrom(source);
        }
        public bool IsMatch(in TypePair context) =>
            GetConverter(context.SourceType).CanConvertTo(context.DestinationType) || GetConverter(context.DestinationType).CanConvertFrom(context.SourceType);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Call(MapMethodInfo, sourceExpression, Constant(sourceExpression.Type), Constant(destExpression.Type));
    }
}