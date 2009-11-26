using System;
using System.Linq.Expressions;

namespace AutoMapper
{
	internal class ExpressionBasedResolver<TSource, TMember> : IMemberResolver
	{
		private readonly Func<TSource, TMember> _method;

		public ExpressionBasedResolver(Expression<Func<TSource, TMember>> expression)
		{
			_method = expression.Compile();
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value != null && ! (source.Value is TSource))
			{
				throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " + source.Value.GetType());
			}

			object result;
			try
			{
				result = _method((TSource)source.Value);
			}
			catch (NullReferenceException)
			{
				result = null;
			}

			return source.New(result);
		}

		public Type MemberType
		{
			get { return typeof (TMember); }
		}
	}
}