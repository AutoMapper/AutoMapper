using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl.QueryMapper
{
    public class MemberAccessQueryMapperVisitor : ExpressionVisitor
    {
        private readonly ExpressionVisitor _rootVisitor;
        private readonly IMappingEngine _mappingEngine;

        public MemberAccessQueryMapperVisitor(ExpressionVisitor rootVisitor, IMappingEngine mappingEngine)
        {
            _rootVisitor = rootVisitor;
            _mappingEngine = mappingEngine;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression parentExpr = _rootVisitor.Visit(node.Expression);
            if (parentExpr != null)
            {
                var propertyMap = _mappingEngine.GetPropertyMap(node.Member, parentExpr.Type);

                var newMember = Expression.MakeMemberAccess(parentExpr, propertyMap.DestinationProperty.MemberInfo);

                return newMember;
            }
            return node;
        }

    }
}