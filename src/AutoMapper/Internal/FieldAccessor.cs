using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class FieldAccessor : MemberAccessorBase
	{
		private readonly FieldInfo _fieldInfo;
		private readonly LateBoundField _lateBoundField;

		public FieldAccessor(FieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
			_lateBoundField = DelegateFactory.Create(fieldInfo);
		}

		public override string Name
		{
			get { return _fieldInfo.Name; }
		}

		public override Type MemberType
		{
			get { return _fieldInfo.FieldType; }
		}

		public override object GetValue(object source)
		{
			return _lateBoundField(source);
		}

		public override void SetValue(object destination, object value)
		{
			_fieldInfo.SetValue(destination, value);
		}

	    public bool Equals(FieldAccessor other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(other._fieldInfo, _fieldInfo);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != typeof (FieldAccessor)) return false;
	        return Equals((FieldAccessor) obj);
	    }

	    public override int GetHashCode()
	    {
	        return _fieldInfo.GetHashCode();
	    }
	}
}