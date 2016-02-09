using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;

    public class DelegateBasedResolver<TSource, TMember> : IValueResolver
    {
        private readonly Func<ResolutionResult, TMember> _method;

        public DelegateBasedResolver(Func<ResolutionResult, TMember> method)
        {
            _method = method;
        }

        public Type MemberType => typeof(TMember);

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && !(source.Value is TSource))
            {
                throw new ArgumentException($"Expected obj to be of type {typeof(TSource)} but was {source.Value.GetType()}");
            }
            var result = _method(source);
            return source.New(result, typeof(TMember));
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

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if(source.Value != null && !(source.Value is TSource))
            {
                throw new ArgumentException($"Expected obj to be of type {typeof (TSource)} but was {source.Value.GetType()}");
            }
            var result = _method((TSource)source.Value);
            return source.New(result, typeof(TMember));
        }
    }
}