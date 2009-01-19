using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class FormatterExpression : IFormatterExpression, IFormatterConfiguration
	{
		private readonly IList<IValueFormatter> _formatters = new List<IValueFormatter>();
		private readonly IDictionary<Type, IFormatterConfiguration> _typeSpecificFormatters = new Dictionary<Type, IFormatterConfiguration>();
		private readonly IList<Type> _formattersToSkip = new List<Type>();

		public void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			AddFormatter(typeof(TValueFormatter));
		}

		public void AddFormatter(Type valueFormatterType)
		{
			var formatter = (IValueFormatter)Activator.CreateInstance(valueFormatterType, true);

			AddFormatter(formatter);
		}

		public void AddFormatter(IValueFormatter valueFormatter)
		{
			_formatters.Add(valueFormatter);
		}

		public void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			_formatters.Add(new ValueFormatterUsingExpression(formatExpression));
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

		public Type[] GetFormattersToSkip()
		{
			return _formattersToSkip.ToArray();
		}

		private class ValueFormatterUsingExpression : IValueFormatter
		{
			private readonly Func<ResolutionContext, string> _valueFormatterExpression;

			public ValueFormatterUsingExpression(Func<ResolutionContext, string> valueFormatterExpression)
			{
				_valueFormatterExpression = valueFormatterExpression;
			}

			public string FormatValue(ResolutionContext context)
			{
				return _valueFormatterExpression(context);
			}
		}

	}
}