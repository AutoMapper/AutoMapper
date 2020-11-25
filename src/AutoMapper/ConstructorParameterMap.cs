using AutoMapper.Internal;
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
        private readonly MemberInfo[] _sourceMembers;
        private Type _sourceType;
        public ConstructorParameterMap(TypeMap typeMap, ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers, bool canResolveValue)
        {
            TypeMap = typeMap;
            Parameter = parameter;
            _sourceMembers = sourceMembers.ToArray();
            CanResolveValue = canResolveValue;
        }
        public ParameterInfo Parameter { get; }
        public override TypeMap TypeMap { get; }
        public override Type SourceType
        {
            get => _sourceType ??= 
                CustomMapExpression?.ReturnType ?? 
                CustomMapFunction?.ReturnType ??
                (_sourceMembers.Length > 0 ? _sourceMembers[_sourceMembers.Length - 1].GetMemberType() : Parameter.ParameterType);
            protected set => _sourceType = value;
        }
        public override Type DestinationType => Parameter.ParameterType;
        public override MemberInfo[] SourceMembers => _sourceMembers;
        public override string DestinationName => Parameter.Name;
        public bool HasDefaultValue => Parameter.IsOptional;
        public override LambdaExpression CustomMapExpression { get; set; }
        public override LambdaExpression CustomMapFunction { get; set; }
        public override bool CanResolveValue { get; set; }
        public override bool Inline { get; set; }
        public Expression DefaultValue() => Parameter.GetDefaultValue();
        public override string ToString() => Parameter.Member.DeclaringType + "." + Parameter.Member + ".parameter " + Parameter.Name;
    }
}