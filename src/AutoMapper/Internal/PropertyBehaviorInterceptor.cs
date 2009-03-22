using System.Collections.Generic;
using System.Reflection;
using LinFu.DynamicProxy;

namespace AutoMapper
{
	internal class PropertyBehaviorInterceptor : IInterceptor
	{
		private readonly IDictionary<string, object> _propertyValues = new Dictionary<string, object>();

		public object Intercept(InvocationInfo info)
		{
			object toReturn = null;

			if (IsSetterCall(info))
			{
				_propertyValues[GetPropertyName(info)] = info.Arguments[0];
			}
			else if (IsGetterCall(info))
			{
				toReturn = _propertyValues[GetPropertyName(info)];
			}

			return toReturn;
		}

		private static string GetPropertyName(InvocationInfo info)
		{
			return info.TargetMethod.Name.Replace("set_", "").Replace("get_", "");
		}

		private static bool IsSetterCall(InvocationInfo info)
		{
			return IsPropertyCall(info) && info.Arguments.Length > 0;
		}

		private static bool IsGetterCall(InvocationInfo info)
		{
			return IsPropertyCall(info) && info.Arguments.Length == 0;
		}

		private static bool IsPropertyCall(InvocationInfo info)
		{
			return info.TargetMethod.IsSpecialName
			       && (info.TargetMethod.Attributes & MethodAttributes.HideBySig) != 0;
		}
	}
}