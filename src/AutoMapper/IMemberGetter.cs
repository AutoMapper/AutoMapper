using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    using System.Reflection;

    public interface IMemberGetter : IMemberResolver
    {
        MemberInfo MemberInfo { get; }
        string Name { get; }
        object GetValue(object source);
    }

    public class MemberGetter : IMemberGetter
    {
        public object Resolve(object source, ResolutionContext context)
        {
            return source;
        }

        public LambdaExpression GetExpression { get; }
        public Type MemberType { get; } = typeof (object);
        public MemberInfo MemberInfo { get; }
        public string Name { get; }
        public object GetValue(object source)
        {
            return source;
        }
    }
}