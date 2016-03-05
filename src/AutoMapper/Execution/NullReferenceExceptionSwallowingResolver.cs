using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper
{
    using System;

    public class NullReferenceExceptionSwallowingResolver : IMemberResolver
    {
        private readonly IMemberResolver _inner;
        private static readonly ExpressionVisitor Visitor = new IfNotNullVisitor();

        public NullReferenceExceptionSwallowingResolver(IMemberResolver inner)
        {
            _inner = inner;
        }

        public object Resolve(object source, ResolutionContext context)
        {
            try
            {
                return _inner.Resolve(source, context);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public LambdaExpression GetExpression => Visitor.Visit(_inner.GetExpression) as LambdaExpression;
        public Type MemberType => _inner.MemberType;

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