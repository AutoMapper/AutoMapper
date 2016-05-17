using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class MemberGetter<TSource, TValue> : IMemberGetter
    {
        protected static readonly DelegateFactory DelegateFactory = new DelegateFactory();

        public abstract MemberInfo MemberInfo { get; }
        public abstract string Name { get; }
        public abstract LambdaExpression GetExpression { get; }
        public abstract Type MemberType { get; }
        public abstract object GetValue(object source);

        public abstract IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit);
        public abstract IEnumerable<object> GetCustomAttributes(bool inherit);
        public abstract bool IsDefined(Type attributeType, bool inherit);
    }

    public class NulloMemberGetter : IMemberGetter
    {
        public MemberInfo MemberInfo { get; }
        public string Name { get; }
        public LambdaExpression GetExpression { get; }
        public Type MemberType { get; }
        public object GetValue(object source) => source;
    }
    public class NulloMemberAccessor : NulloMemberGetter, IMemberAccessor
    {
        public MemberInfo MemberInfo { get; }
        public string Name { get; }
        public LambdaExpression GetExpression { get; }
        public Type MemberType { get; }
        public object GetValue(object source) => source;
        public void SetValue(object destination, object value) { }
    }
}