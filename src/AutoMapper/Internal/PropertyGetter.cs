using System;
using System.Reflection;
using AutoMapper;

namespace AutoMapper.Impl
{
	public class PropertyGetter : MemberGetter
	{
		private readonly PropertyInfo _propertyInfo;
		private readonly string _name;
		private readonly Type _memberType;
		private readonly LateBoundPropertyGet _lateBoundPropertyGet;

		public PropertyGetter(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
			_name = _propertyInfo.Name;
			_memberType = _propertyInfo.PropertyType;
			if (_propertyInfo.GetGetMethod(true) != null)
				_lateBoundPropertyGet = DelegateFactory.CreateGet(propertyInfo);
			else
			    _lateBoundPropertyGet = src => null;
		}

		public override MemberInfo MemberInfo
		{
			get { return _propertyInfo; }
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
			return _lateBoundPropertyGet(source);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return _propertyInfo.GetCustomAttributes(attributeType, inherit);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return _propertyInfo.GetCustomAttributes(inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return _propertyInfo.IsDefined(attributeType, inherit);
		}

		public bool Equals(PropertyGetter other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._propertyInfo, _propertyInfo);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(PropertyGetter)) return false;
			return Equals((PropertyGetter)obj);
		}

		public override int GetHashCode()
		{
			return _propertyInfo.GetHashCode();
		}
	}

	public class PropertyAccessor : PropertyGetter, IMemberAccessor
	{
		private readonly LateBoundPropertySet _lateBoundPropertySet;
		private readonly bool _hasSetter;

		public PropertyAccessor(PropertyInfo propertyInfo)
			: base(propertyInfo)
		{
			_hasSetter = propertyInfo.GetSetMethod(true) != null;
			if (_hasSetter)
			{
				_lateBoundPropertySet = DelegateFactory.CreateSet(propertyInfo);
			}
		}

		public bool HasSetter
		{
			get { return _hasSetter; }
		}

		public virtual void SetValue(object destination, object value)
		{
			_lateBoundPropertySet(destination, value);
		}
	}

	public class ValueTypePropertyAccessor : PropertyGetter, IMemberAccessor
	{
		private readonly MethodInfo _lateBoundPropertySet;
		private readonly bool _hasSetter;

		public ValueTypePropertyAccessor(PropertyInfo propertyInfo)
			: base(propertyInfo)
		{
			var method = propertyInfo.GetSetMethod(true);
			_hasSetter = method != null;
			if (_hasSetter)
			{
				_lateBoundPropertySet = method;
			}
		}

		public bool HasSetter
		{
			get { return _hasSetter; }
		}

		public void SetValue(object destination, object value)
		{
			_lateBoundPropertySet.Invoke(destination, new[] { value });
		}
	}
}