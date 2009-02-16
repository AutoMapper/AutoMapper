using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class FieldAccessor : MemberAccessorBase
	{
		private readonly FieldInfo _fieldInfo;

		public FieldAccessor(FieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
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
			return _fieldInfo.GetValue(source);
		}

		public override void SetValue(object destination, object value)
		{
			_fieldInfo.SetValue(destination, value);
		}
	}
}