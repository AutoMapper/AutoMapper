using System.Linq.Expressions;

namespace AutoMapper
{
    using System;

    public interface IMemberResolver : IValueResolver
    {
        Type MemberType { get; }
    }

    public interface IDelegateResolver : IMemberResolver
    {
        LambdaExpression Expression { get; }
    }
}