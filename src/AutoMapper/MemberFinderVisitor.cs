using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public class MemberFinderVisitor : ExpressionVisitor
    {
        private Type _sourceType;

        public MemberFinderVisitor(Type sourceType)
        {
            _sourceType = sourceType;
        }

        public MemberExpression Member { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType.IsAssignableFrom(_sourceType))
            {
                Member = node;
            }

            return base.VisitMember(node);
        }
    }
}