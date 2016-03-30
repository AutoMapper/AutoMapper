using System;

namespace AutoMapper
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ConstructorParameterMap
    {
        private Func<ResolutionContext, object> _resolverFunc;
        private bool _sealed;

        public ConstructorParameterMap(ParameterInfo parameter, IMemberGetter[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public IMemberGetter[] SourceMembers { get; }

        public bool CanResolve { get; set; }
        public LambdaExpression CustomExpression { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.MemberType;
        public Type DestinationType => Parameter.ParameterType;

        internal void Seal()
        {
            if (_sealed)
                return;

            Func<ResolutionContext, object> valueResolverFunc;

            if (CustomExpression != null)
            {
                var newParam = Expression.Parameter(typeof(object), "m");
                var expr = new ConvertingVisitor(CustomExpression.Parameters[0], newParam).Visit(CustomExpression.Body);
                expr = new IfNotNullVisitor().Visit(expr);
                var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(expr, typeof(object)), newParam);
                Func<object, object> mapFunc = lambda.Compile();

                valueResolverFunc = ctxt => mapFunc(ctxt.SourceValue);
            }
            else
            {

                var innerResolver =
                    SourceMembers.Aggregate<IMemberGetter, LambdaExpression>(
                        (Expression<Func<ResolutionContext, object>>)
                            (ctxt => ctxt.SourceValue),
                        (expression, resolver) =>
                            (LambdaExpression)new ExpressionConcatVisitor(resolver.GetExpression).Visit(expression));

                var outerResolver =
                    (Expression<Func<ResolutionContext, object>>)
                        Expression.Lambda(Expression.Convert(innerResolver.Body, typeof(object)),
                            innerResolver.Parameters);

                valueResolverFunc = outerResolver.Compile();
            }

            _resolverFunc = valueResolverFunc;
            _sealed = true;
        }

        public object ResolveValue(ResolutionContext context)
        {
            return _resolverFunc(context);
        }

        public void ResolveUsing(IValueResolver resolver)
        {
            SourceResolvers = new[] { resolver };
        }
    }
}