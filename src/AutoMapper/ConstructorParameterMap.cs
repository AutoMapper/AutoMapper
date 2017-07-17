using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, MemberInfo[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public MemberInfo[] SourceMembers { get; }

        public bool CanResolve { get; set; }

        public bool DefaultValue { get; set; }

        public LambdaExpression CustomExpression { get; set; }

        public LambdaExpression CustomValueResolver { get; set; }

        public Type DestinationType => Parameter.ParameterType;
    }
}