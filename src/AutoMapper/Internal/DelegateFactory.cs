using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper
{
	internal delegate object LateBoundMethod(object target, object[] arguments);
	internal delegate object LateBoundPropertyGet(object target);
	internal delegate object LateBoundFieldGet(object target);
	internal delegate void LateBoundFieldSet(object target, object value);
	internal delegate void LateBoundPropertySet(object target, object value);


	internal static class DelegateFactory
	{
		public static LateBoundMethod CreateGet(MethodInfo method)
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

		public static LateBoundPropertyGet CreateGet(PropertyInfo property)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

			MemberExpression member = Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property);

			Expression<LateBoundPropertyGet> lambda = Expression.Lambda<LateBoundPropertyGet>(
				Expression.Convert(member, typeof(object)),
				instanceParameter
				);

			return lambda.Compile();
		}

		public static LateBoundFieldGet CreateGet(FieldInfo field)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

			MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);

			Expression<LateBoundFieldGet> lambda = Expression.Lambda<LateBoundFieldGet>(
				Expression.Convert(member, typeof(object)),
				instanceParameter
				);

			return lambda.Compile();
		}

		public static LateBoundFieldSet CreateSet(FieldInfo field)
		{
			var sourceType = field.DeclaringType;
			var method = new DynamicMethod("Set" + field.Name, null, new[] { typeof(object), typeof(object) }, true);
			var gen = method.GetILGenerator();
			
			gen.Emit(OpCodes.Ldarg_0); // Load input to stack
			gen.Emit(OpCodes.Castclass, sourceType); // Cast to source type
			gen.Emit(OpCodes.Ldarg_1); // Load value to stack
			gen.Emit(OpCodes.Unbox_Any, field.FieldType); // Unbox the value to its proper value type
			gen.Emit(OpCodes.Stfld, field); // Set the value to the input field
			gen.Emit(OpCodes.Ret);

			var callback = (LateBoundFieldSet)method.CreateDelegate(typeof(LateBoundFieldSet));

			return callback;
		}

		public static LateBoundPropertySet CreateSet(PropertyInfo property)
		{
			var sourceType = property.DeclaringType;
			var setter = property.GetSetMethod(true);
			var method = new DynamicMethod("Set" + property.Name, null, new[] { typeof(object), typeof(object) }, true);
			var gen = method.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0); // Load input to stack
			gen.Emit(OpCodes.Castclass, sourceType); // Cast to source type
			gen.Emit(OpCodes.Ldarg_1); // Load value to stack
			gen.Emit(OpCodes.Unbox_Any, property.PropertyType); // Unbox the value to its proper value type
			gen.Emit(OpCodes.Callvirt, setter); // Call the setter method
			gen.Emit(OpCodes.Ret);

			var result = (LateBoundPropertySet)method.CreateDelegate(typeof(LateBoundPropertySet));

			return result;
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