using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using LazyExpression = Lazy<LambdaExpression>;
    using static Expression;

    public class ConvertMapper : IObjectMapper
    {
        private Dictionary<TypePair, LazyExpression> _converters = GetConverters();

        private static Dictionary<TypePair, LazyExpression> GetConverters()
        {
            var primitiveTypes = new[]
            {
                typeof(string), typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float),
                typeof(double), typeof(decimal), typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong)
            };
            return
                (from sourceType in primitiveTypes
                 from destinationType in primitiveTypes.Concat(from type in primitiveTypes.Where(t => t.IsValueType()) select typeof(Nullable<>).MakeGenericType(type))
                 select new
                 {
                     Key = new TypePair(sourceType, destinationType),
                     Value = new LazyExpression(() => ConvertExpression(sourceType, destinationType), isThreadSafe: false)
                 })
                 .ToDictionary(i => i.Key, i => i.Value);
        }

        static LambdaExpression ConvertExpression(Type sourceType, Type destinationType)
        {
            var sourceParameter = Parameter(sourceType, "source");
            var underlyingDestinationType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            var convertMethod = typeof(Convert).GetDeclaredMethod("To" + underlyingDestinationType.Name, new[] { sourceType });
            var callConvert = Call(convertMethod, sourceParameter);
            return Lambda(callConvert, sourceParameter);
        }

        public bool IsMatch(TypePair types) => _converters.ContainsKey(types);

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var typeMap = new TypePair(sourceExpression.Type, destExpression.Type);
            return _converters[typeMap].Value.ReplaceParameters(sourceExpression);
        }
    }
}