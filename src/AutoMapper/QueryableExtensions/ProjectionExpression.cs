namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ProjectionExpression<TSource> : IProjectionExpression
    {
        private readonly IQueryable<TSource> _source;
        private readonly IMappingEngine _mappingEngine;

        public ProjectionExpression(IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            _source = source;
            _mappingEngine = mappingEngine;
        }

        public IQueryable<TResult> To<TResult>(object parameters = null)
        {
            return To<TResult>(parameters, new string[0]);
        }

        public IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand)
        {
            var paramValues = (parameters ?? new object()).GetType()
                .GetProperties()
                .ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));

            return To<TResult>(paramValues, membersToExpand);
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters)
        {
            return To<TResult>(parameters, new string[0]);
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters, params string[] membersToExpand)
        {
            return _source.Select(_mappingEngine.CreateMapExpression<TSource, TResult>(parameters, membersToExpand));
        }

        public IQueryable<TResult> To<TResult>(object parameters = null, params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = membersToExpand.Select(expr =>
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expr);
                return visitor.MemberName;
            })
                .ToArray();
            return To<TResult>(parameters, members);
        }

        public IQueryable<TResult> To<TResult>(System.Collections.Generic.IDictionary<string, object> parameters, params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = membersToExpand.Select(expr =>
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expr);
                return visitor.MemberName;
            })
                .ToArray();
            return _source.Select(_mappingEngine.CreateMapExpression<TSource, TResult>(parameters, members));
        }

        private class MemberVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                MemberName = node.Member.Name;
                return base.VisitMember(node);
            }

            public string MemberName { get; private set; }
        }
    }
}
