using System.Linq.Expressions;

namespace AutoMapper
{
    using System;

    public interface IMemberResolver
    {
        LambdaExpression GetExpression { get; }
        Type MemberType { get; }
    }
}