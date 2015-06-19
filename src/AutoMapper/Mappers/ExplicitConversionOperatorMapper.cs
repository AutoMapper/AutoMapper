namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class ExplicitConversionOperatorMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var implicitOperator = GetExplicitConversionOperator(context);

            return implicitOperator.Invoke(null, new[] {context.SourceValue});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            var methodInfo = GetExplicitConversionOperator(context);

            return methodInfo != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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