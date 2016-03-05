using System.Linq.Expressions;

namespace AutoMapper
{
    using System;

    public interface IMemberResolver : IValueResolver
    {
        Type MemberType { get; }
    }

    public interface IExpressionResolver : IMemberResolver
    {
        LambdaExpression Expression { get; }
    }
}