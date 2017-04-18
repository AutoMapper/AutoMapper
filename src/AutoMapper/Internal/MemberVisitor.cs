using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Internal
{
    public class MemberVisitor : ExpressionVisitor
    {
        public static IEnumerable<MemberInfo> GetMemberPath(Expression expression)
        {
            var memberVisitor = new MemberVisitor();
            memberVisitor.Visit(expression);
            return memberVisitor.MemberPath;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            MemberPath = GetMemberPath(node);
            return node;
        }

        private IEnumerable<MemberInfo> GetMemberPath(MemberExpression memberExpression)
        {
            var expression = memberExpression;
            while(expression != null)
            {
                yield return expression.Member;
                expression = expression.Expression as MemberExpression;
            }
        }

        public IEnumerable<MemberInfo> MemberPath { get; private set; }
    }
}