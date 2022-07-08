using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AutoMapper
{
    using Internal;
    using System.ComponentModel;

    [DebuggerDisplay("{DestinationExpression}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PathMap : MemberMap
    {
        public PathMap(PathMap pathMap, TypeMap typeMap, IncludedMember includedMember) : this(pathMap.DestinationExpression, pathMap.MemberPath, typeMap)
        {
            IncludedMember = includedMember.Chain(pathMap.IncludedMember);
            Resolver = pathMap.Resolver;
            Condition = pathMap.Condition;
            Ignored = pathMap.Ignored;
        }
        public PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap) : base(typeMap)
        {
            MemberPath = memberPath;
            DestinationExpression = destinationExpression;
        }
        public override Type SourceType => Resolver.ResolvedType;
        public LambdaExpression DestinationExpression { get; }
        public MemberPath MemberPath { get; }
        public override Type DestinationType => MemberPath.Last.GetMemberType();
        public override string DestinationName => MemberPath.ToString();
        public override bool CanResolveValue => !Ignored;
        public override bool CanBeSet => ReflectionHelper.CanBeSet(MemberPath.Last);
        public override bool Ignored { get; set; }
        public override LambdaExpression Condition { get; set; }
    }
}