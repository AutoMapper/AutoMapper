using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Internal
{
    public class MapMemberVisitor : ExpressionVisitor
    {
        public static IEnumerable<MapMemberInfo> GetMemberPath(Expression expression)
        {
            var memberVisitor = new MapMemberVisitor();
            memberVisitor.Visit(expression);
            return memberVisitor.MemberPath;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _members.AddRange
            (
                node.GetMemberExpressions()
                    .Select(e => new MapMemberInfo(e.Member, e.Expression.Type))
            );

            return node;
        }

        private readonly List<MapMemberInfo> _members = new List<MapMemberInfo>();
        public IEnumerable<MapMemberInfo> MemberPath => _members;
    }
}
