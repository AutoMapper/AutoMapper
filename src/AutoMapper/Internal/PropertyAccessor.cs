using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class PropertyAccessor : MemberAccessorBase
	{
		private readonly PropertyInfo _propertyInfo;
		private readonly string _name;
		private readonly Type _memberType;
		private readonly LateBoundProperty _lateBoundProperty;

		public PropertyAccessor(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
			_name = _propertyInfo.Name;
			_memberType = _propertyInfo.PropertyType;
			_lateBoundProperty = DelegateFactory.Create(propertyInfo);
		}

		public override string Name
		{
			get { return _name; }
		}

		public override Type MemberType
		{
			get { return _memberType; }
		}

		public override object GetValue(object source)
		{
			return _lateBoundProperty(source);
		}

		public override void SetValue(object destination, object value)
		{
			_propertyInfo.SetValue(destination, value, new object[0]);
		}

	    public bool Equals(PropertyAccessor other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(other._propertyInfo, _propertyInfo);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != typeof (PropertyAccessor)) return false;
	        return Equals((PropertyAccessor) obj);
	    }

	    public override int GetHashCode()
	    {
	        return _propertyInfo.GetHashCode();
	    }
	}
}