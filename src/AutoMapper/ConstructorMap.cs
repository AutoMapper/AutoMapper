using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    public class ConstructorMap
    {
        private readonly LateBoundParamsCtor _runtimeCtor;
        public ConstructorInfo Ctor { get; private set; }
        public IEnumerable<ConstructorParameterMap> CtorParams { get; private set; }

        public ConstructorMap(ConstructorInfo ctor, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            Ctor = ctor;
            CtorParams = ctorParams;

            _runtimeCtor = DelegateFactory.CreateCtor(ctor, CtorParams);
        }

        public object ResolveValue(ResolutionContext context)
        {
            var ctorArgs = CtorParams
                        .Select(p => p.ResolveValue(context))
                        .Select(result => result.Value)
                        .ToArray();

            return _runtimeCtor(ctorArgs);
        }
    }
}