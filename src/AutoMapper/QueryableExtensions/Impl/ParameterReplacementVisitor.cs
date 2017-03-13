using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class ParameterReplacementVisitor : ExpressionVisitor
    {
        private readonly Expression _memberExpression;

        public ParameterReplacementVisitor(Expression memberExpression) => _memberExpression = memberExpression;

        protected override Expression VisitParameter(ParameterExpression node) => _memberExpression;
    }
}