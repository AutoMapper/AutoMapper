namespace AutoMapper.Internal
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ReflectionHelper
    {
        public static string GetPropertyName(this LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if(memberExpression == null)
            {
                throw new ArgumentOutOfRangeException("expression", "Expected a property/field access expression, not " + expression);
            }
            return memberExpression.Member.Name;
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
    }
}