using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using System;
    using Internal;

    [DebuggerDisplay("{DestinationExpression}")]
    public class PathMap
    {
        public PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap)
        {
            MemberPath = memberPath;
            TypeMap = typeMap;
            DestinationExpression = destinationExpression;
        }

        public TypeMap TypeMap { get; }
        public LambdaExpression DestinationExpression { get; }
        public LambdaExpression SourceExpression { get; set; }
        public MemberPath MemberPath { get; }
        public MemberInfo DestinationMember => MemberPath.Last;
        public bool Ignored { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}