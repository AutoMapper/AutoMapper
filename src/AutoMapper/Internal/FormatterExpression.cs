using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	internal class FormatterExpression : IFormatterExpression, IFormatterConfiguration, IFormatterCtorConfigurator
	{
		private readonly IList<IValueFormatter> _formatters = new List<IValueFormatter>();
		private readonly IDictionary<Type, IFormatterConfiguration> _typeSpecificFormatters = new Dictionary<Type, IFormatterConfiguration>();
		private readonly IList<Type> _formattersToSkip = new List<Type>();

		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			var formatter = new DeferredInstantiatedFormatter(() => (IValueFormatter)Activator.CreateInstance(typeof(TValueFormatter), true));

			AddFormatter(formatter);

			return new FormatterCtorExpression<TValueFormatter>(this);
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			var formatter = new DeferredInstantiatedFormatter(() => (IValueFormatter) Activator.CreateInstance(valueFormatterType, true));

			AddFormatter(formatter);

			return new FormatterCtorExpression(valueFormatterType, this);
		}

		public void AddFormatter(IValueFormatter valueFormatter)
		{
			_formatters.Add(valueFormatter);
		}

		public void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			_formatters.Add(new ExpressionValueFormatter(formatExpression));
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_formattersToSkip.Add(typeof(TValueFormatter));
		}

		public IFormatterExpression ForSourceType<TSource>()
		{
			var valueFormatter = new FormatterExpression();

			_typeSpecificFormatters[typeof (TSource)] = valueFormatter;

			return valueFormatter;
		}

		public IValueFormatter[] GetFormatters()
		{
			return _formatters.ToArray();
		}

		public IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters()
		{
			return new Dictionary<Type, IFormatterConfiguration>(_typeSpecificFormatters);
		}

		public Type[] GetFormatterTypesToSkip()
		{
			return _formattersToSkip.ToArray();
		}

		public void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator)
		{
			_formatters.RemoveAt(_formatters.Count - 1);
			_formatters.Add(new DeferredInstantiatedFormatter(instantiator));
		}
	}

	internal interface IFormatterCtorConfigurator
	{
		void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator);
	}
}