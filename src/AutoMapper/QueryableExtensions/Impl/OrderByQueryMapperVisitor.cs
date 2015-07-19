namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;

    public class OrderByQueryMapperVisitor : ExpressionVisitor
    {
        private readonly ExpressionVisitor _rootVisitor;

        public OrderByQueryMapperVisitor(ExpressionVisitor rootVisitor)
        {
            _rootVisitor = rootVisitor;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
    }
}