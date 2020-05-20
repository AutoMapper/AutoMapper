using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConstructorParameterMap : DefaultMemberMap
    {
        public ConstructorParameterMap(TypeMap typeMap, ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers,
            bool canResolveValue)
        {
            TypeMap = typeMap;
            Parameter = parameter;
            SourceMembers = sourceMembers.ToList();
            CanResolveValue = canResolveValue;
        }

        public ParameterInfo Parameter { get; }

        public override TypeMap TypeMap { get; }

        public override Type SourceType =>
            CustomMapExpression?.ReturnType
            ?? CustomMapFunction?.ReturnType
            ?? (Parameter.IsOptional 
                ? Parameter.ParameterType 
                : SourceMembers.LastOrDefault()?.GetMemberType());

        public override Type DestinationType => Parameter.ParameterType;

        public override IReadOnlyCollection<MemberInfo> SourceMembers { get; }
        public override string DestinationName => Parameter.Member.DeclaringType + "." + Parameter.Member + ".parameter " + Parameter.Name;

        public bool HasDefaultValue => Parameter.IsOptional;

        public override LambdaExpression CustomMapExpression { get; set; }
        public override LambdaExpression CustomMapFunction { get; set; }

        public override bool CanResolveValue { get; set; }

        public override bool Inline { get; set; }

        public Expression DefaultValue() => Expression.Constant(Parameter.GetDefaultValue());
    }
}