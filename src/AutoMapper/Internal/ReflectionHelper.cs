using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
	internal static class ReflectionHelper
	{
        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            bool done = false;

            while (!done)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        var memberExpression = ((MemberExpression)expressionToCheck);

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression), "lambdaExpression");
                        }

                        MemberInfo member = memberExpression.Member;

                        return member;
                    default:
                        done = true;
                        break;
                }
            }

            return null;
        }
        
        public static IMemberAccessor GetAccessor(this Type targetType, string accessorName, BindingFlags bindingFlags)
		{
			MemberInfo[] members = targetType.GetMember(accessorName, bindingFlags);
			return
				members.FirstOrDefault(member => member is PropertyInfo || member is FieldInfo)
				.ToMemberAccessor();
		}

		public static IMemberAccessor ToMemberAccessor(this MemberInfo accessorCandidate)
		{
			if (accessorCandidate == null)
				return null;

			if (accessorCandidate is PropertyInfo)
				return new PropertyAccessor((PropertyInfo)accessorCandidate);

			if (accessorCandidate is FieldInfo)
				return new FieldAccessor((FieldInfo)accessorCandidate);

			return null;
		}
	}
}
