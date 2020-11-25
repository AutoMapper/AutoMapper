using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConstructorMap
    {
        private bool? _canResolve;
        private readonly List<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();
        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        public IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;
        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }
        public bool CanResolve
        {
            get => _canResolve ??= ParametersCanResolve();
            set => _canResolve = value;
        }
        private bool ParametersCanResolve()
        {
            foreach (var param in _ctorParams)
            {
                if (!param.CanResolveValue)
                {
                    return false;
                }
            }
            return true;
        }
        public void AddParameter(ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers, bool canResolve) =>
            _ctorParams.Add(new ConstructorParameterMap(TypeMap, parameter, sourceMembers, canResolve));
    }
}