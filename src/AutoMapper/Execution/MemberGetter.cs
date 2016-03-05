namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class MemberGetter : IMemberGetter
    {
        protected static readonly DelegateFactory DelegateFactory = new DelegateFactory();

        public abstract MemberInfo MemberInfo { get; }
        public abstract string Name { get; }
        public abstract Type MemberType { get; }
        public abstract object GetValue(object source);

        public object Resolve(object source, ResolutionContext context) => source == null ? null : GetValue(source);

        public abstract IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit);
        public abstract IEnumerable<object> GetCustomAttributes(bool inherit);
        public abstract bool IsDefined(Type attributeType, bool inherit);
    }
}