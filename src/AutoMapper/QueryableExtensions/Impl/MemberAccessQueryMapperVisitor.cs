namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;

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
                // TODO: remove this is ugly HACK which is required for WebApi OData Support
                // this is caused by OData wrapping a constant expression to a typed LinqParameterContainer
                // in order to improve EntityFrameworks ExpressionTranslation
                // see: http://www.symbolsource.org/MyGet/Metadata/aspnetwebstacknightly/Project/Microsoft.AspNet.WebApi.OData/5.0.0-alpha-130310/Release/Default/System.Web.Http.OData/System.Web.Http.OData/System.Web.Http.OData/OData/Query/Expressions/LinqParameterContainer.cs?ImageName=System.Web.Http.OData
                // NHibernate can work with that expression too:  https://github.com/Pathoschild/webapi.nhibernate-odata
                if (node.Member.DeclaringType.FullName.StartsWith("System.Web.Http.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer") &&
                    node.Member.Name == "TypedProperty")
                {
                    return node;
                }

                var propertyMap = _mappingEngine.GetPropertyMap(node.Member, parentExpr.Type);
               
                var newMember = Expression.MakeMemberAccess(parentExpr, propertyMap.DestinationProperty.MemberInfo);

                return newMember;
            }
            return node;
        }

    }
}