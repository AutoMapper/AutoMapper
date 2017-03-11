using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberAccessQueryMapperVisitor : ExpressionVisitor
    {
        private readonly ExpressionVisitor _rootVisitor;
        private readonly IConfigurationProvider _config;

        public MemberAccessQueryMapperVisitor(ExpressionVisitor rootVisitor, IConfigurationProvider config)
        {
            _rootVisitor = rootVisitor;
            _config = config;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var parentExpr = _rootVisitor.Visit(node.Expression);
            if (parentExpr != null)
            {
                var propertyMap = _config.GetPropertyMap(node.Member, parentExpr.Type);
               
                var newMember = Expression.MakeMemberAccess(parentExpr, propertyMap.DestinationProperty);

                return newMember;
            }
            return node;
        }

    }
}