namespace AutoMapper.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ReflectionHelper
	{
        public static object Map(ResolutionContext context, MemberInfo member, object value)
        {
            var memberType = member.GetMemberType();
            return context.Engine.Mapper.Map(value, value?.GetType() ?? memberType, memberType);
        }

        public static bool IsDynamic(this object obj)
        {
            return obj is IDynamicMetaObjectProvider;
        }

        public static bool IsDynamic(this Type type)
        {
            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
        }

        public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
        {
            var property = propertyOrField as PropertyInfo;
            if(property != null)
            {
                property.SetValue(target, value, null);
                return;
            }
            var field = propertyOrField as FieldInfo;
            if(field != null)
            {
                field.SetValue(target, value);
                return;
            }
            throw Expected(propertyOrField);
        }

        private static ArgumentOutOfRangeException Expected(MemberInfo propertyOrField)
        {
            return new ArgumentOutOfRangeException("propertyOrField", "Expected a property or field, not " + propertyOrField);
        }

        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
        {
            var property = propertyOrField as PropertyInfo;
            if(property != null)
            {
                return property.GetValue(target, null);
            }
            var field = propertyOrField as FieldInfo;
            if(field != null)
            {
                return field.GetValue(target);
            }
            throw Expected(propertyOrField);
        }

        public static IEnumerable<MemberInfo> GetMemberPath(Type type, string fullMemberName)
        {
            MemberInfo property = null;
            foreach(var memberName in fullMemberName.Split('.'))
            {
                var currentType = GetCurrentType(property, type);
                yield return property = currentType.GetMember(memberName).Single();
            }
        }

        private static Type GetCurrentType(MemberInfo member, Type type)
        {
            var memberType = member?.GetMemberType() ?? type;
            if(memberType.IsGenericType() && typeof(IEnumerable).IsAssignableFrom(memberType))
            {
                memberType = memberType.GetTypeInfo().GenericTypeArguments[0];
            }
            return memberType;
        }

        public static MemberInfo GetFieldOrProperty(this LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if(memberExpression == null)
            {
                throw new ArgumentOutOfRangeException("expression", "Expected a property/field access expression, not " + expression);
            }
            return (MemberInfo) memberExpression.Member;
        }

        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            bool done = false;

            while (!done)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression) expressionToCheck).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression) expressionToCheck).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        var memberExpression = ((MemberExpression) expressionToCheck);

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(
                                $"Expression '{lambdaExpression}' must resolve to top-level member and not any child object's properties. Use a custom resolver on the child type or the AfterMap option instead.",
                                nameof(lambdaExpression));
                        }

                        MemberInfo member = memberExpression.Member;

                        return member;
                    default:
                        done = true;
                        break;
                }
            }

            throw new AutoMapperConfigurationException(
                "Custom configuration for members is only supported for top-level individual members on a type.");
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is MethodInfo)
                return ((MethodInfo) memberInfo).ReturnType;
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo) memberInfo).PropertyType;
            if (memberInfo is FieldInfo)
                return ((FieldInfo) memberInfo).FieldType;
            return null;
        }
        
		public static IMemberGetter ToMemberGetter(this MemberInfo accessorCandidate)
		{
			if (accessorCandidate == null)
				return null;

			if (accessorCandidate is PropertyInfo)
                return new PropertyGetter((PropertyInfo) accessorCandidate);

			if (accessorCandidate is FieldInfo)
                return new FieldGetter((FieldInfo) accessorCandidate);

			if (accessorCandidate is MethodInfo)
                return new MethodGetter((MethodInfo) accessorCandidate);

			return null;
		}
        
		public static IMemberAccessor ToMemberAccessor(this MemberInfo accessorCandidate)
		{
			var fieldInfo = accessorCandidate as FieldInfo;
			if (fieldInfo != null)
                return accessorCandidate.DeclaringType.IsValueType()
                    ? (IMemberAccessor) new ValueTypeFieldAccessor(fieldInfo)
                    : new FieldAccessor(fieldInfo);

			var propertyInfo = accessorCandidate as PropertyInfo;
			if (propertyInfo != null)
                return accessorCandidate.DeclaringType.IsValueType()
                    ? (IMemberAccessor) new ValueTypePropertyAccessor(propertyInfo)
                    : new PropertyAccessor(propertyInfo);

			return null;
		}

        /// <summary>
        /// if targetType is oldType, method will return newType
        /// if targetType is not oldType, method will return targetType
        /// if targetType is generic type with oldType arguments, method will replace all oldType arguments on newType
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="oldType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static Type ReplaceItemType(this Type targetType, Type oldType, Type newType)
        {
            if (targetType == oldType)
                return newType;

            if (targetType.IsGenericType())
            {
                var genSubArgs = targetType.GetTypeInfo().GenericTypeArguments;
                var newGenSubArgs = new Type[genSubArgs.Length];
                for (int i = 0; i < genSubArgs.Length; i++)
                    newGenSubArgs[i] = ReplaceItemType(genSubArgs[i], oldType, newType);
                return targetType.GetGenericTypeDefinition().MakeGenericType(newGenSubArgs);
            }

            return targetType;
        }
	}
}
