namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;

    public class MemberAccessQueryMapperVisitor : ExpressionVisitor
    {
        private readonly ExpressionVisitor _rootVisitor;
        private readonly IMapper _mapper;

        public MemberAccessQueryMapperVisitor(ExpressionVisitor rootVisitor, IMapper mapper)
        {
            _rootVisitor = rootVisitor;
            _mapper = mapper;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression parentExpr = _rootVisitor.Visit(node.Expression);
            if (parentExpr != null)
            {
                var propertyMap = _mapper.GetPropertyMap(node.Member, parentExpr.Type);

                var newMember = Expression.MakeMemberAccess(parentExpr, propertyMap.DestinationProperty.MemberInfo);

                return newMember;
            }
            return node;
        }

    }
}