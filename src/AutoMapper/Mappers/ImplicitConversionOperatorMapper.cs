namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class ImplicitConversionOperatorMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var implicitOperator = GetImplicitConversionOperator(context);

            return implicitOperator.Invoke(null, new[] {context.SourceValue});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            var methodInfo = GetImplicitConversionOperator(context);

            return methodInfo != null;
        }

        private static MethodInfo GetImplicitConversionOperator(ResolutionContext context)
        {
            var sourceTypeMethod = context.SourceType
                .GetDeclaredMethods()
                .Where(mi => mi.IsPublic && mi.IsStatic)
                .Where(mi => mi.Name == "op_Implicit")
                .FirstOrDefault(mi => mi.ReturnType == context.DestinationType);

            var destTypeMethod = context.DestinationType.GetMethod("op_Implicit", new[] {context.SourceType});

            return sourceTypeMethod ?? destTypeMethod;
        }
    }
}