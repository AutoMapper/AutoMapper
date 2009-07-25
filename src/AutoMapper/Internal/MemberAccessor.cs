using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal abstract class MemberAccessor : IMemberAccessor
	{
		public abstract string Name { get; }
		public abstract Type MemberType { get; }
		public abstract object GetValue(object source);
		public abstract void SetValue(object destination, object value);

		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, MemberType)
			       	: new ResolutionResult(GetValue(source.Value), MemberType);
		}

        public static IMemberAccessor Create(MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return new FieldAccessor(fieldInfo);

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return new PropertyAccessor(propertyInfo);

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
                return new MethodAccessor(methodInfo);

            return null;
        }
	}
}