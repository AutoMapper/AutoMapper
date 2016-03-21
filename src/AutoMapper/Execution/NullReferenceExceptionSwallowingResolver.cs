using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper
{
    using System;

    public class NullReferenceExceptionSwallowingResolver : IMemberResolver
    {

        private static readonly ExpressionVisitor Visitor = new IfNotNullVisitor();

        public NullReferenceExceptionSwallowingResolver(IMemberResolver inner)
        {
            Inner = inner;
        }

        public object Resolve(object source, ResolutionContext context)
        {
            try
            {
                return Inner.Resolve(source, context);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public IMemberResolver Inner { get; }
        public LambdaExpression GetExpression => Visitor.Visit(Inner.GetExpression) as LambdaExpression;
        public Type MemberType => Inner.MemberType;

        private class IfNotNullVisitor : ExpressionVisitor
        {
            private readonly IList<MemberExpression> AllreadyUpdated = new List<MemberExpression>(); 
            protected override Expression VisitMember(MemberExpression node)
            {
                if (AllreadyUpdated.Contains(node))
                    return base.VisitMember(node);
                AllreadyUpdated.Add(node);
                var a = DelegateFactory.IfNotNullExpression(node);
                return Visit(a);
            }
        }
    }
}