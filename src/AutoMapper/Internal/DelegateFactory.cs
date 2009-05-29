using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
	internal delegate object LateBoundMethod(object target, object[] arguments);
	internal delegate object LateBoundProperty(object target);
	internal delegate object LateBoundField(object target);

	internal static class DelegateFactory
	{
		public static LateBoundMethod Create(MethodInfo method)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
			ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

			MethodCallExpression call = Expression.Call(
				Expression.Convert(instanceParameter, method.DeclaringType),
				method,
				CreateParameterExpressions(method, argumentsParameter));

			Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
				Expression.Convert(call, typeof(object)),
				instanceParameter,
				argumentsParameter);

			return lambda.Compile();
		}

		public static LateBoundProperty Create(PropertyInfo property)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

			MemberExpression member = Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property);

			Expression<LateBoundProperty> lambda = Expression.Lambda<LateBoundProperty>(
				Expression.Convert(member, typeof(object)),
				instanceParameter
				);

			return lambda.Compile();
		}

		public static LateBoundField Create(FieldInfo field)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

			MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);

			Expression<LateBoundField> lambda = Expression.Lambda<LateBoundField>(
				Expression.Convert(member, typeof(object)),
				instanceParameter
				);

			return lambda.Compile();
		}

		private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
		{
			return method.GetParameters().Select((parameter, index) =>
				Expression.Convert(
					Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
					parameter.ParameterType)).ToArray();
		}
	}
}