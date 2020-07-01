using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class ImplicitConversionOperatorMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            var methodInfo = GetImplicitConversionOperator(context);

            return methodInfo != null;
        }

        private static MethodInfo GetImplicitConversionOperator(TypePair context)
        {
            var destinationType = context.DestinationType;
            var sourceTypeMethod = context.SourceType
                .GetDeclaredMethods()
                .FirstOrDefault(mi => mi.IsPublic && mi.IsStatic && mi.Name == "op_Implicit" && mi.ReturnType == destinationType);

            return sourceTypeMethod ?? destinationType.GetRuntimeMethod("op_Implicit", new[] { context.SourceType });
        }


        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var implicitOperator = GetImplicitConversionOperator(new TypePair(sourceExpression.Type, destExpression.Type));
            return Expression.Call(null, implicitOperator, sourceExpression);
        }
    }
}