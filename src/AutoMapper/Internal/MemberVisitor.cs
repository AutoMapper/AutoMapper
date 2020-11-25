using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Internal
{
    public class MemberVisitor : ExpressionVisitor
    {
        public static MemberInfo[] GetMemberPath(Expression expression)
        {
            var memberVisitor = new MemberVisitor();
            memberVisitor.Visit(expression);
            return memberVisitor.MemberPath.ToArray();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _members.AddRange(node.GetMemberExpressions().Select(e => e.Member));
            return node;
        }

        private readonly List<MemberInfo> _members = new List<MemberInfo>();
        public IReadOnlyCollection<MemberInfo> MemberPath => _members;
    }
}