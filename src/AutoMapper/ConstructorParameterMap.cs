using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    public class ConstructorParameterMap : IMemberMap
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

        public TypeMap TypeMap { get; }

        public Type SourceType =>
            CustomMapExpression?.Type
            ?? CustomMapFunction?.Type
            ?? (Parameter.IsOptional 
                ? Parameter.ParameterType 
                : SourceMembers.Last().GetMemberType());

        public Type DestinationType => Parameter.ParameterType;
        public TypePair Types => new TypePair(SourceType, DestinationType);

        public IEnumerable<MemberInfo> SourceMembers { get; }
        public string DestinationName => Parameter.Name;

        public bool HasDefaultValue => Parameter.IsOptional;

        public LambdaExpression Condition => null;
        public LambdaExpression PreCondition => null;
        public LambdaExpression CustomMapExpression { get; set; }
        public LambdaExpression CustomMapFunction { get; set; }
        public ValueResolverConfiguration ValueResolverConfig => null;
        public ValueConverterConfiguration ValueConverterConfig => null;
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; } = Enumerable.Empty<ValueTransformerConfiguration>();

        public bool CanResolveValue { get; set; }

        public bool Ignored => false;
        public bool Inline { get; set; }
        public bool UseDestinationValue => false;
        public object NullSubstitute => null;
    }
}