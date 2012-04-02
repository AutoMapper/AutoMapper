using System.Reflection;
using System.Linq;

namespace AutoMapper.Mappers
{
	public class ImplicitConversionOperatorMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
		    var implicitOperator = GetImplicitConversionOperator(context);

		    return implicitOperator.Invoke(null, new[] {context.SourceValue});
		}

		public bool IsMatch(ResolutionContext context)
		{
            var methodInfo = GetImplicitConversionOperator(context);

		    return methodInfo != null;
		}

	    private static MethodInfo GetImplicitConversionOperator(ResolutionContext context)
	    {
	        var sourceTypeMethod = context.SourceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.Name == "op_Implicit")
                .Where(mi => mi.ReturnType == context.DestinationType)
                .FirstOrDefault();

	        var destTypeMethod = context.DestinationType.GetMethod("op_Implicit", new[] {context.SourceType});

	        return sourceTypeMethod ?? destTypeMethod;
	    }
	}

}