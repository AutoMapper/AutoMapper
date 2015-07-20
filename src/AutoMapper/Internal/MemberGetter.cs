namespace AutoMapper.Internal
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

        public ResolutionResult Resolve(ResolutionResult source)
        {
            return source.Value == null
                ? source.New(source.Value, MemberType)
                : source.New(GetValue(source.Value), MemberType);
        }

        public abstract IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit);
        public abstract IEnumerable<object> GetCustomAttributes(bool inherit);
        public abstract bool IsDefined(Type attributeType, bool inherit);
    }
}