using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Reflection;
    using Configuration;

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
            if(destinationType.IsNullableType())
            {
                destinationType = destinationType.GetTypeOfNullable();
            }
            var sourceTypeMethod = context.SourceType
                .GetDeclaredMethods()
                .FirstOrDefault(mi => mi.IsPublic && mi.IsStatic && mi.Name == "op_Implicit" && mi.ReturnType == destinationType);

            return sourceTypeMethod ?? destinationType.GetDeclaredMethod("op_Implicit", new[] { context.SourceType });
        }


        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var implicitOperator = GetImplicitConversionOperator(new TypePair(sourceExpression.Type, destExpression.Type));
            return Expression.Call(null, implicitOperator, sourceExpression);
        }
    }
}