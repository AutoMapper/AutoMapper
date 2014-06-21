namespace AutoMapper
{
	public class NullReplacementMethod : IValueResolver
	{
		private readonly object _nullSubstitute;
        public object NullSubstitute {get { return _nullSubstitute; }}

		public NullReplacementMethod(object nullSubstitute)
		{
			_nullSubstitute = nullSubstitute;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (_nullSubstitute == null)
			{
				return source;
			}
			return source.Value == null
					? source.New(_nullSubstitute)
					: source;
		}
	}
}
