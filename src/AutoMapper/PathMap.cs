using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using System;
    using Internal;

    [DebuggerDisplay("{DestinationExpression}")]
    public class PathMap : IMemberMap
    {
        public PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap)
        {
            MemberPath = memberPath;
            TypeMap = typeMap;
            DestinationExpression = destinationExpression;
        }

        public TypeMap TypeMap { get; }

        public Type SourceType => CustomMapExpression.ReturnType;

        public IEnumerable<MemberInfo> SourceMembers { get; } = Enumerable.Empty<MemberInfo>();
        public LambdaExpression DestinationExpression { get; }
        public LambdaExpression CustomMapExpression { get; set; }
        public MemberPath MemberPath { get; }
        public Type DestinationType => MemberPath.Last.GetMemberType();
        public string DestinationName => MemberPath.ToString();
        public TypePair Types => new TypePair(SourceType, DestinationType);

        public bool CanResolveValue => !Ignored;

        public bool Ignored { get; set; }
        public bool Inline { get; set; } = true;
        public bool UseDestinationValue => false;
        public object NullSubstitute => null;
        public LambdaExpression PreCondition => null;
        public LambdaExpression Condition { get; set; }
        public LambdaExpression CustomMapFunction => null;
        public ValueResolverConfiguration ValueResolverConfig => null;
        public ValueConverterConfiguration ValueConverterConfig => null;
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; } = Enumerable.Empty<ValueTransformerConfiguration>();
    }
}