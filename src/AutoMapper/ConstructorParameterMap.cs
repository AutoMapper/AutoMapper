using System;

namespace AutoMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, IValueResolver[] sourceResolvers, bool canResolve)
        {
            Parameter = parameter;
            SourceResolvers = sourceResolvers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; private set; }

        public IValueResolver[] SourceResolvers { get; private set; }

        public bool CanResolve { get; set; }

        public object ResolveValue(ResolutionContext context)
        {
            return SourceResolvers.Aggregate(context.SourceValue, (current, resolver) => resolver.Resolve(current, context));
        }

        public void ResolveUsing(params IValueResolver[] resolvers)
        {
            SourceResolvers = resolvers;
        }
    }
}