using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;

    public class ExpressionBasedResolver<TSource, TMember> : IMemberResolver
    {
        public LambdaExpression GetExpression { get; }
        private readonly Func<TSource, TMember> _method;

        public ExpressionBasedResolver(Expression<Func<TSource, TMember>> expression)
        {
            GetExpression = expression;
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