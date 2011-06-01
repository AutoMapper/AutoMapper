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
			: this(context.SourceValue, context)
		{
		}

		private ResolutionResult(object value, ResolutionContext context, Type memberType)
		{
			_value = value;
			_context = context;
			_type = ResolveType(value, memberType);
			_memberType = memberType;
		}

		private ResolutionResult(object value, ResolutionContext context)
		{
            _value = value;
			_context = context;
			_type = ResolveType(value, typeof(object));
            _memberType = _type;
        }

		public object Value { get { return _value; } }
		public Type Type { get { return _type; } }
		public Type MemberType { get { return _memberType; } }
		public ResolutionContext Context { get { return _context; } }

        public bool ShouldIgnore { get; set; }

	    private Type ResolveType(object value, Type memberType)
        {
            if (value == null)
                return memberType;

            return value.GetType();
        }
        public ResolutionResult Ignore()
        {
            return new ResolutionResult(Context) { ShouldIgnore = true };
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