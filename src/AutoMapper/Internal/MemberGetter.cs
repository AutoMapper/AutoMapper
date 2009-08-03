using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal abstract class MemberGetter : IMemberGetter
	{
		public abstract string Name { get; }
		public abstract Type MemberType { get; }
		public abstract object GetValue(object source);

		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, MemberType)
			       	: new ResolutionResult(GetValue(source.Value), MemberType);
		}

        public static IMemberGetter Create(MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return new FieldGetter(fieldInfo);

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return new PropertyGetter(propertyInfo);

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
                return new MethodGetter(methodInfo);

            return null;
        }
	}

	internal static class MemberAccessor
	{
		public static IMemberAccessor Create(MemberInfo memberInfo)
		{
			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
				return new FieldAccessor(fieldInfo);

			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				return new PropertyAccessor(propertyInfo);

			return null;
		}
	}
}