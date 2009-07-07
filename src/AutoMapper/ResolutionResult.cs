using System;

namespace AutoMapper
{
	public class ResolutionResult
	{
		private readonly object _value;
		private readonly Type _type;
		private readonly Type _memberType;

		public ResolutionResult(object value, Type memberType)
		{
			_value = value;
			_type = ResolveType(value, memberType);
			_memberType = memberType;
		}

	    public ResolutionResult(object value, Type memberType, Type type)
		{
			_value = value;
			_type = type;
			_memberType = memberType;
		}

		public ResolutionResult(object value)
		{
            _value = value;
            _type = ResolveType(value, typeof(object));
            _memberType = _type;
        }

		public object Value { get { return _value; } }
		public Type Type { get { return _type; } }
		public Type MemberType { get { return _memberType; } }
    
        private Type ResolveType(object value, Type memberType)
        {
            if (value == null)
                return memberType;

            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                return memberType;

            return value.GetType();
        }

    }
}