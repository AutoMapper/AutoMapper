using System;

namespace AutoMapper
{
	internal class FormatterCtorExpression : IFormatterCtorExpression
	{
		private readonly Type _formatterType;
		private readonly IFormatterCtorConfigurator _formatterCtorConfigurator;

		public FormatterCtorExpression(Type formatterType, IFormatterCtorConfigurator formatterCtorConfigurator)
		{
			_formatterType = formatterType;
			_formatterCtorConfigurator = formatterCtorConfigurator;
		}

		public void ConstructedBy(Func<IValueFormatter> constructor)
		{
			_formatterCtorConfigurator.ConstructFormatterBy(_formatterType, constructor);
		}
	}

	internal class FormatterCtorExpression<TValueFormatter> : IFormatterCtorExpression<TValueFormatter>
		where TValueFormatter : IValueFormatter
	{
		private readonly IFormatterCtorConfigurator _formatterCtorConfigurator;

		public FormatterCtorExpression(IFormatterCtorConfigurator formatterCtorConfigurator)
		{
			_formatterCtorConfigurator = formatterCtorConfigurator;
		}

		public void ConstructedBy(Func<TValueFormatter> constructor)
		{
			_formatterCtorConfigurator.ConstructFormatterBy(typeof (TValueFormatter), () => constructor());
		}
	}
}