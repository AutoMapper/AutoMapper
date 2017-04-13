using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using Internal;

    [DebuggerDisplay("{DestinationExpression}")]
    public class PathMap
    {

        public PathMap(TypeMap typeMap)
        {
            TypeMap = typeMap;
        }

        public TypeMap TypeMap { get; }
        public LambdaExpression DestinationExpression { get; set; }
        public LambdaExpression SourceExpression { get; set; }
        public MemberPath MemberPath { get; }

        public MemberInfo DestinationMember => ((MemberExpression)DestinationExpression.Body).Member;
    }
}