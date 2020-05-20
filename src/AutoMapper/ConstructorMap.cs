using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConstructorMap
    {
        private readonly IList<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();

        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        public IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;

        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }

        public bool CanResolve => CtorParams.All(param => param.CanResolveValue);

        public void AddParameter(ParameterInfo parameter, MemberInfo[] resolvers, bool canResolve) =>
            _ctorParams.Add(new ConstructorParameterMap(TypeMap, parameter, resolvers, canResolve));
    }
}