using System;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions
{
    public class ExpressionResolutionResult
    {
        private readonly Expression _nonMarkerExpression;
        public Expression ResolutionExpression { get; }
        public Expression NonMarkerExpression => _nonMarkerExpression ?? ResolutionExpression;
        public Type Type { get; }
        public ExpressionResolutionResult(Expression resolutionExpression, Type type = null)
        {
            ResolutionExpression = resolutionExpression;
            Type = type ?? resolutionExpression.Type;
        }
        public ExpressionResolutionResult(Expression resolutionExpression, LambdaExpression nonMarkerExpression) : this(resolutionExpression) =>
            _nonMarkerExpression = nonMarkerExpression;
    }
}