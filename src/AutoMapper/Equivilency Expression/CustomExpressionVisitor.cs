using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.EquivilencyExpression
{
    internal class CustomExpressionVisitor : ExpressionVisitor
    {
        readonly ParameterExpression _parameter;
        private readonly IEnumerable<PropertyMap> _propertyMaps;

        internal CustomExpressionVisitor(ParameterExpression parameter, IEnumerable<PropertyMap> propertyMaps)
        {
            _parameter = parameter;
            _propertyMaps = propertyMaps;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo)
            {
                var matchPM = _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.MemberInfo == node.Member);
                if (matchPM == null)
                    throw new Exception("No matching PropertyMap");
                var sourceValueResolvers = matchPM.GetSourceValueResolvers();
                if (!sourceValueResolvers.All(r => r is IMemberGetter))
                    throw new Exception("Not all member getters");

                var memberGetters = sourceValueResolvers.OfType<IMemberGetter>();
                var memberExpression = Expression.Property(Visit(node.Expression), memberGetters.First().MemberInfo as PropertyInfo);

                foreach (var memberGetter in memberGetters.Skip(1))
                    memberExpression = Expression.Property(memberExpression, memberGetter.MemberInfo as PropertyInfo);
                return memberExpression;
            }
            return base.VisitMember(node);
        }
    }
}