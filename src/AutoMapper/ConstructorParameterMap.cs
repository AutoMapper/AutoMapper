using System;

namespace AutoMapper
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Mappers;

    public class ConstructorParameterMap
    {
        internal readonly ConstructorMap _ctorMap;

        public ConstructorParameterMap(ConstructorMap ctorMap, ParameterInfo parameter, IMemberGetter[] sourceMembers, bool canResolve)
        {
            _ctorMap = ctorMap;
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public IMemberGetter[] SourceMembers { get; }

        public bool CanResolve { get; set; }
        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.MemberType;
        public Type DestinationType => Parameter.ParameterType;
        
    }
}