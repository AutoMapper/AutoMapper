using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using static AutoMapper.Internal.ExpressionFactory;
using System.Reflection;

namespace AutoMapper
{
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
    }
}

namespace AutoMapper.Execution
{
    using static Expression;

    public static class PathMapExtension
    {
        internal static Expression HandlePath(this PathMap pathMap, TypeMapPlanBuilder planBuilder)
        {
            var destination = ((MemberExpression)pathMap.DestinationExpression.ConvertReplaceParameters(planBuilder.Destination))
                .Expression;
            var createInnerObjects = CreateInnerObjects(destination);
            var setFinalValue = new PropertyMap(pathMap).CreatePropertyMapFunc(planBuilder);
            return Block(createInnerObjects, setFinalValue);
        }

        private static Expression CreateInnerObjects(this Expression destination) => Block(destination.GetMembers()
            .Select(NullCheck)
            .Reverse()
            .Concat(new[] { Empty() }));

        private static Expression NullCheck(MemberExpression memberExpression)
        {
            var setter = GetSetter(memberExpression);
            var ifNull = setter == null
                ? (Expression)
                Throw(Constant(new NullReferenceException(
                    $"{memberExpression} cannot be null because it's used by ForPath.")))
                : Assign(setter, DelegateFactory.GenerateConstructorExpression(memberExpression.Type));
            return memberExpression.IfNullElse(ifNull);
        }
    }
}