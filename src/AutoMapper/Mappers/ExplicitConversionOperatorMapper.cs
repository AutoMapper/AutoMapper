using System.Reflection;
using System.Linq;

namespace AutoMapper.Mappers
{
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
	        var sourceTypeMethod = context.SourceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.Name == "op_Explicit")
                .Where(mi => mi.ReturnType == context.DestinationType)
                .FirstOrDefault();

            var destTypeMethod = context.DestinationType.GetMethod("op_Explicit", new[] { context.SourceType });

	        return sourceTypeMethod ?? destTypeMethod;
	    }
	}

}