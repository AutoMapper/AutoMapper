namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Reflection;
    using Internal;

    public class ExplicitConversionOperatorMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var implicitOperator = GetExplicitConversionOperator(context);

            return implicitOperator.Invoke(null, new[] {context.SourceValue});
        }

        public bool IsMatch(ResolutionContext context)
        {
            var methodInfo = GetExplicitConversionOperator(context);

            return methodInfo != null;
        }

        private static MethodInfo GetExplicitConversionOperator(ResolutionContext context)
        {
            var sourceTypeMethod = context.SourceType
                .GetDeclaredMethods()
                .Where(mi => mi.IsPublic && mi.IsStatic)
                .Where(mi => mi.Name == "op_Explicit")
                .FirstOrDefault(mi => mi.ReturnType == context.DestinationType);

            var destTypeMethod = context.DestinationType.GetMethod("op_Explicit", new[] {context.SourceType});

            return sourceTypeMethod ?? destTypeMethod;
        }
    }
}