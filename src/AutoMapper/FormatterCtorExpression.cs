using System;

namespace AutoMapper
{
	internal class FormatterCtorExpression : IFormatterCtorExpression
	{
		private readonly Type _formatterType;
		private readonly FormatterExpression _formatterExpression;

		public FormatterCtorExpression(Type formatterType, FormatterExpression formatterExpression)
		{
			_formatterType = formatterType;
			_formatterExpression = formatterExpression;
		}

		public void ConstructedBy(Func<IValueFormatter> constructor)
		{
			_formatterExpression.ConstructFormatterBy(_formatterType, constructor);
		}
	}

	internal class FormatterCtorExpression<TValueFormatter> : IFormatterCtorExpression<TValueFormatter>
		where TValueFormatter : IValueFormatter
	{
		private readonly FormatterExpression _formatterExpression;

		public FormatterCtorExpression(FormatterExpression formatterExpression)
		{
			_formatterExpression = formatterExpression;
		}

		public void ConstructedBy(Func<TValueFormatter> constructor)
		{
			_formatterExpression.ConstructFormatterBy(typeof (TValueFormatter), () => constructor());
		}
	}
}