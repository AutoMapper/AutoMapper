using System;
using System.Reflection;

namespace AutoMapper
{
	internal class PropertyMember : IValueResolver
	{
		private readonly PropertyInfo _property;

		public PropertyMember(PropertyInfo property)
		{
			_property = property;
		}

		public object Resolve(object obj)
		{
			return _property.GetValue(obj, new object[0]);
		}

		public Type GetResolvedValueType()
		{
			return _property.PropertyType;
		}
	}
}