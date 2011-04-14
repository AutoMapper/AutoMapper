using System;

namespace AutoMapper
{
	public class ResolutionResult
	{
		private readonly object _value;
		private readonly ResolutionContext _context;
		private readonly Type _type;
		private readonly Type _memberType;

		public ResolutionResult(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

            _value = context.SourceValue;
            _context = context;
            _type = ResolveType(_value, typeof(object));
            _memberType = _type;
		}

		private ResolutionResult(object value, ResolutionContext context, Type memberType)
		{
            if(context == null) throw new ArgumentNullException("context");
            if(memberType == null) throw new ArgumentNullException("memberType");

			_value = value;
			_context = context;
			_type = ResolveType(value, memberType);
			_memberType = memberType;
		}

		private ResolutionResult(object value, ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

            _value = value;
			_context = context;
			_type = ResolveType(value, typeof(object));
            _memberType = _type;
        }

		public object Value { get { return _value; } }
		public Type Type { get { return _type; } }
		public Type MemberType { get { return _memberType; } }
		public ResolutionContext Context { get { return _context; } }
    
        private Type ResolveType(object value, Type memberType)
        {
            if (value == null)
                return memberType;

            return value.GetType();
        }

		public ResolutionResult New(object value)
		{
			return new ResolutionResult(value, Context);
		}

		public ResolutionResult New(object value, Type memberType)
		{
			return new ResolutionResult(value, Context, memberType);
		}
    }
}