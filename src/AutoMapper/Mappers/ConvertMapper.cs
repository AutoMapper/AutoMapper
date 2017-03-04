using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using LazyExpression = Lazy<LambdaExpression>;
    using static Expression;
    using static ExpressionExtensions;

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
            bool nullableDestination;
            var underlyingDestinationType = UnderlyingType(destinationType, out nullableDestination);
            var convertMethod = typeof(Convert).GetDeclaredMethod("To" + underlyingDestinationType.Name, new[] { sourceType });
            var sourceParameter = Parameter(sourceType, "source");
            Expression convertCall = Call(convertMethod, sourceParameter);
            var lambdaBody = nullableDestination && !sourceType.IsValueType() ?
                                            Condition(Equal(sourceParameter, Constant(null)), Constant(null, destinationType), ToType(convertCall, destinationType)) :
                                            convertCall;
            return Lambda(lambdaBody, sourceParameter);
        }

        private static Type UnderlyingType(Type type, out bool nullable)
        {
            var underlyingDestinationType = Nullable.GetUnderlyingType(type);
            if(underlyingDestinationType == null)
            {
                nullable = false;
                return type;
            }
            else
            {
                nullable = true;
                return underlyingDestinationType;
            }
        }

        public bool IsMatch(TypePair types) => _converters.ContainsKey(types);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var typeMap = new TypePair(sourceExpression.Type, destExpression.Type);
            return _converters[typeMap].Value.ReplaceParameters(sourceExpression);
        }
    }
}