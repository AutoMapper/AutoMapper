using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using Internal;

    [DebuggerDisplay("{DestinationExpression}")]
    public class PathMap : DefaultMemberMap
    {
        public PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap)
        {
            MemberPath = memberPath;
            TypeMap = typeMap;
            DestinationExpression = destinationExpression;
        }

        public override TypeMap TypeMap { get; }

        public override Type SourceType => CustomMapExpression.ReturnType;

        public LambdaExpression DestinationExpression { get; }
        public override LambdaExpression CustomMapExpression { get; set; }
        public MemberPath MemberPath { get; }
        public override Type DestinationType => MemberPath.Last.GetMemberType();
        public override string DestinationName => MemberPath.ToString();

        public override bool CanResolveValue => !Ignored;

        public override bool Ignored { get; set; }
        public override LambdaExpression Condition { get; set; }
    }
}