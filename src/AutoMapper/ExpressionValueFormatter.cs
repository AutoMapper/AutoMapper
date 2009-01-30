using System;

namespace AutoMapper
{
	internal class ExpressionValueFormatter : IValueFormatter
	{
		private readonly Func<ResolutionContext, string> _valueFormatterExpression;

		public ExpressionValueFormatter(Func<ResolutionContext, string> valueFormatterExpression)
		{
			_valueFormatterExpression = valueFormatterExpression;
		}

		public string FormatValue(ResolutionContext context)
		{
			return _valueFormatterExpression(context);
		}
	}
}