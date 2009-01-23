using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class ValueFormatter : IValueFormatter
	{
		private readonly IFormatterConfiguration _formatterConfiguration;

		public ValueFormatter(IFormatterConfiguration formatterConfiguration)
		{
			_formatterConfiguration = formatterConfiguration;
		}

		public string FormatValue(ResolutionContext context)
		{
			Type valueType = context.SourceType;
			string formattedValue = null;
			object valueToFormat = context.SourceValue;
			IFormatterConfiguration typeSpecificFormatterConfig = null;

			if (context.PropertyMap.GetFormatters().Length > 0)
			{
				foreach (IValueFormatter formatter in context.PropertyMap.GetFormatters())
				{
					formattedValue = formatter.FormatValue(context.CreateValueContext(valueToFormat));
					valueToFormat = formattedValue;
				}
			}
			else if (_formatterConfiguration.GetTypeSpecificFormatters().TryGetValue(valueType, out typeSpecificFormatterConfig))
			{
				if (!context.PropertyMap.FormattersToSkipContains(typeSpecificFormatterConfig.GetType()))
				{
					var typeSpecificFormatter = new ValueFormatter(typeSpecificFormatterConfig);
					formattedValue = typeSpecificFormatter.FormatValue(context);
					valueToFormat = formattedValue;
				}
				else
				{
					formattedValue = context.SourceValue.ToNullSafeString();
				}
			}
			else
			{
				formattedValue = context.SourceValue.ToNullSafeString();
			}

			foreach (IValueFormatter formatter in _formatterConfiguration.GetFormatters())
			{
				Type formatterType = formatter.GetType();
				if (CheckPropertyMapSkipList(context, formatterType) &&
					CheckTypeSpecificSkipList(typeSpecificFormatterConfig, formatterType))
				{
					formattedValue = formatter.FormatValue(context.CreateValueContext(valueToFormat));
					valueToFormat = formattedValue;
				}
			}

			return formattedValue;
		}

		private static bool CheckTypeSpecificSkipList(IFormatterConfiguration valueFormatter, Type formatterType)
		{
			if (valueFormatter == null)
			{
				return true;
			}

			return !valueFormatter.GetFormattersToSkip().Contains(formatterType);
		}

		private static bool CheckPropertyMapSkipList(ResolutionContext context, Type formatterType)
		{
			return !context.PropertyMap.FormattersToSkipContains(formatterType);
		}

	}

	public interface IFormatterConfiguration
	{
		IValueFormatter[] GetFormatters();
		IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters();
		Type[] GetFormattersToSkip();
	}
}