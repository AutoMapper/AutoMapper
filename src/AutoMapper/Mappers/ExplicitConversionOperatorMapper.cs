using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class ExplicitConversionOperatorMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context)
        {
            var methodInfo = GetExplicitConversionOperator(context);

            return methodInfo != null;
        }

        private static MethodInfo GetExplicitConversionOperator(in TypePair context)
        {
            MethodInfo sourceTypeMethod = null;
            foreach (var mi in context.SourceType.GetDeclaredMethods())
            {
                if (mi.IsPublic && mi.IsStatic &&
                    (mi.Name == "op_Explicit" && mi.ReturnType == context.DestinationType))
                {
                    sourceTypeMethod = mi;
                    break;
                }
            }

            var destTypeMethod = context.DestinationType.GetDeclaredMethod("op_Explicit", new[] {context.SourceType});

            return sourceTypeMethod ?? destTypeMethod;
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var implicitOperator = GetExplicitConversionOperator(new TypePair(sourceExpression.Type, destExpression.Type));
            return Expression.Call(null, implicitOperator, sourceExpression);
        }
    }
}