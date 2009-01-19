using System;
using System.Reflection;

namespace AutoMapper
{
	internal class PropertyMember : TypeMember
	{
		private readonly PropertyInfo _property;

		public PropertyMember(PropertyInfo property)
		{
			_property = property;
		}

		public override object GetValue(object obj)
		{
			return _property.GetValue(obj, new object[0]);
		}

		public override Type GetMemberType()
		{
			return _property.PropertyType;
		}
	}
}