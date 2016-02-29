using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;

    public class DelegateBasedResolver<TSource, TMember> : IMemberResolver
    {
        private readonly Func<object, ResolutionContext, TMember> _method;

        public DelegateBasedResolver(Func<object, ResolutionContext, TMember> method)
        {
            _method = method;
        }

        public Type MemberType => typeof(TMember);

        public object Resolve(object source, ResolutionContext context)
        {
            if (source != null && !(source is TSource))
            {
                throw new ArgumentException($"Expected obj to be of type {typeof(TSource)} but was {source.GetType()}");
            }
            var result = _method(source, context);
            return result;
        }
    }
    public class ExpressionBasedResolver<TSource, TMember> : IExpressionResolver
    {
        public LambdaExpression Expression { get; }
        private readonly Func<TSource, TMember> _method;

        public ExpressionBasedResolver(Expression<Func<TSource, TMember>> expression)
        {
            Expression = expression;
            _method = expression.Compile();
        }

        public Type MemberType => typeof(TMember);

        public object Resolve(object source, ResolutionContext context)
        {
            if(source != null && !(source is TSource))
            {
                throw new ArgumentException($"Expected obj to be of type {typeof (TSource)} but was {source.GetType()}");
            }
            var result = _method((TSource)source);
            return result;
        }
    }
}