namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq.Expressions;

    public class ExpressionResolutionResult
    {
        public Expression ResolutionExpression { get; private set; }
        public Type Type { get; private set; }

        public ExpressionResolutionResult(Expression resolutionExpression, Type type)
        {
            ResolutionExpression = resolutionExpression;
            Type = type;
        }
    }
}