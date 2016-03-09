using System.Linq.Expressions;

namespace AutoMapper
{
    using System;

    public interface IMemberResolver : IValueResolver
    {
        LambdaExpression GetExpression { get; }
        Type MemberType { get; }
    }

    public interface IExpressionResolver : IMemberResolver
    {
        LambdaExpression Expression { get; }
    }
}