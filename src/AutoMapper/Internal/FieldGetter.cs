using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class FieldGetter : MemberGetter
	{
		private readonly FieldInfo _fieldInfo;
		private readonly string _name;
		private readonly Type _memberType;
		private readonly LateBoundFieldGet _lateBoundFieldGet;
		private readonly LateBoundFieldSet _lateBoundFieldSet;

		public FieldGetter(FieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
			_name = fieldInfo.Name;
			_memberType = fieldInfo.FieldType;
			_lateBoundFieldGet = DelegateFactory.CreateGet(fieldInfo);
			_lateBoundFieldSet = DelegateFactory.CreateSet(fieldInfo);
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
			return _lateBoundFieldGet(source);
		}

	    public bool Equals(FieldGetter other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(other._fieldInfo, _fieldInfo);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != typeof (FieldGetter)) return false;
	        return Equals((FieldGetter) obj);
	    }

	    public override int GetHashCode()
	    {
	        return _fieldInfo.GetHashCode();
	    }
	}

	internal class FieldAccessor : FieldGetter, IMemberAccessor
	{
		private readonly LateBoundFieldSet _lateBoundFieldSet;

		public FieldAccessor(FieldInfo fieldInfo) : base(fieldInfo)
		{
			_lateBoundFieldSet = DelegateFactory.CreateSet(fieldInfo);
		}

		public void SetValue(object destination, object value)
		{
			_lateBoundFieldSet(destination, value);
		}
	}

	internal class ValueTypeFieldAccessor : FieldGetter, IMemberAccessor
	{
		private readonly FieldInfo _lateBoundFieldSet;

		public ValueTypeFieldAccessor(FieldInfo fieldInfo) : base(fieldInfo)
		{
			_lateBoundFieldSet = fieldInfo;
		}

		public void SetValue(object destination, object value)
		{
			_lateBoundFieldSet.SetValue(destination, value);
		}
	}
}