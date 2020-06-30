using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using LazyExpression = Lazy<LambdaExpression>;
    using static Expression;
    using static ExpressionFactory;

    public class ConvertMapper : IObjectMapper
    {
        private readonly Dictionary<TypePair, LazyExpression> _converters = GetConverters();

        private static Dictionary<TypePair, LazyExpression> GetConverters()
        {
            var primitiveTypes = new[]
            {
                typeof(string), typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float),
                typeof(double), typeof(decimal), typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong)
            };
            return
                (from sourceType in primitiveTypes
                 from destinationType in primitiveTypes
                 select new
                 {
                     Key = new TypePair(sourceType, destinationType),
                     Value = new LazyExpression(() => ConvertExpression(sourceType, destinationType), isThreadSafe: false)
                 })
                 .ToDictionary(i => i.Key, i => i.Value);
        }

        static LambdaExpression ConvertExpression(Type sourceType, Type destinationType)
        {
            var convertMethod = typeof(Convert).GetRuntimeMethod("To" + destinationType.Name, new[] { sourceType });
            var sourceParameter = Parameter(sourceType, "source");
            return Lambda(Call(convertMethod, sourceParameter), sourceParameter);
        }

        public bool IsMatch(TypePair types) => _converters.ContainsKey(types);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var typeMap = new TypePair(sourceExpression.Type, destExpression.Type);
            return _converters[typeMap].Value.ReplaceParameters(sourceExpression);
        }
    }
}