using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Reflection;

    public class ExplicitConversionOperatorMapper : IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            var implicitOperator = GetExplicitConversionOperator(context.Types);

            return implicitOperator.Invoke(null, new[] {context.SourceValue});
        }

        public bool IsMatch(TypePair context)
        {
            var methodInfo = GetExplicitConversionOperator(context);

            return methodInfo != null;
        }

        private static MethodInfo GetExplicitConversionOperator(TypePair context)
        {
            var sourceTypeMethod = context.SourceType
                .GetDeclaredMethods()
                .Where(mi => mi.IsPublic && mi.IsStatic)
                .Where(mi => mi.Name == "op_Explicit")
                .FirstOrDefault(mi => mi.ReturnType == context.DestinationType);

            var destTypeMethod = context.DestinationType.GetMethod("op_Explicit", new[] {context.SourceType});

            return sourceTypeMethod ?? destTypeMethod;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var implicitOperator = GetExplicitConversionOperator(new TypePair(sourceExpression.Type, destExpression.Type));
            return Expression.Call(null, implicitOperator, sourceExpression);
        }
    }
}