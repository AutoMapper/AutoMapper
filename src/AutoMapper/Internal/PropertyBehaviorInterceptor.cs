using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Interceptor;

namespace AutoMapper
{
	internal class PropertyBehaviorInterceptor : IInterceptor
	{
		private readonly IDictionary<string, object> _propertyValues = new Dictionary<string, object>();

        private static string GetPropertyName(IInvocation info)
		{
			return info.Method.Name.Replace("set_", "").Replace("get_", "");
		}

        private static bool IsSetterCall(IInvocation info)
		{
			return IsPropertyCall(info) && info.Arguments.Length > 0;
		}

        private static bool IsGetterCall(IInvocation info)
		{
			return IsPropertyCall(info) && info.Arguments.Length == 0;
		}

        private static bool IsPropertyCall(IInvocation info)
		{
            return info.Method.IsSpecialName
                   && (info.Method.Attributes & MethodAttributes.HideBySig) != 0;
		}

	    public void Intercept(IInvocation invocation)
	    {
            object toReturn = null;

            if (IsSetterCall(invocation))
            {
                _propertyValues[GetPropertyName(invocation)] = invocation.Arguments[0];
            }
            else if (IsGetterCall(invocation))
            {
                toReturn = _propertyValues[GetPropertyName(invocation)];
            }

	        invocation.ReturnValue = toReturn;
        }
	}
}