using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class PropertyAccessor : MemberAccessorBase
	{
		private readonly PropertyInfo _propertyInfo;

		public PropertyAccessor(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		public override string Name
		{
			get { return _propertyInfo.Name; }
		}

		public override Type MemberType
		{
			get { return _propertyInfo.PropertyType; }
		}

		public override object GetValue(object source)
		{
			return _propertyInfo.GetValue(source, new object[0]);
		}

		public override void SetValue(object destination, object value)
		{
			_propertyInfo.SetValue(destination, value, new object[0]);
		}
	}
}