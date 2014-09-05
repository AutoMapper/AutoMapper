namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using AutoMapper.Impl;

    public class ConstantExpressionReplacementVisitor : ExpressionVisitor
    {
        private readonly System.Collections.Generic.IDictionary<string, object> _paramValues;

        public ConstantExpressionReplacementVisitor(System.Collections.Generic.IDictionary<string, object> paramValues)
        {
            _paramValues = paramValues;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!node.Member.DeclaringType.Name.Contains("<>"))
                return base.VisitMember(node);

            if (!_paramValues.ContainsKey(node.Member.Name))
                return base.VisitMember(node);

            return Expression.Convert(
                Expression.Constant(_paramValues[node.Member.Name]),
                node.Member.GetMemberType());
        }
    }
}