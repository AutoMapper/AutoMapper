using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AutoMapper
{
    using Internal;
    using System.ComponentModel;

    [DebuggerDisplay("{DestinationExpression}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PathMap : DefaultMemberMap
    {
        public PathMap(PathMap pathMap, TypeMap typeMap, IncludedMember includedMember) : this(pathMap.DestinationExpression, pathMap.MemberPath, typeMap)
        {
            IncludedMember = includedMember;
            CustomMapExpression = pathMap.CustomMapExpression;
            Condition = pathMap.Condition;
            Ignored = pathMap.Ignored;
        }

        public PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap)
        {
            MemberPath = memberPath;
            TypeMap = typeMap;
            DestinationExpression = destinationExpression;
        }

        public override TypeMap TypeMap { get; }

        public override Type SourceType => CustomMapExpression.ReturnType;
        public override IncludedMember IncludedMember { get; set; }
        public LambdaExpression DestinationExpression { get; }
        public override LambdaExpression CustomMapExpression { get; set; }
        public MemberPath MemberPath { get; }
        public override Type DestinationType => MemberPath.Last.GetMemberType();
        public override string DestinationName => MemberPath.ToString();

        public override bool CanResolveValue => !Ignored;
        public override bool CanBeSet => ReflectionHelper.CanBeSet(MemberPath.Last);
        public override bool Ignored { get; set; }
        public override LambdaExpression Condition { get; set; }
    }
}