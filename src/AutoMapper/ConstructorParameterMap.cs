namespace AutoMapper
{
    using System.Linq;
    using System.Reflection;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, IMemberGetter[] sourceResolvers)
        {
            Parameter = parameter;
            SourceResolvers = sourceResolvers;
        }

        public ParameterInfo Parameter { get; private set; }

        public IMemberGetter[] SourceResolvers { get; }

        public ResolutionResult ResolveValue(ResolutionContext context)
        {
            var result = new ResolutionResult(context);

            return SourceResolvers.Aggregate(result, (current, resolver) => resolver.Resolve(current));
        }
    }
}