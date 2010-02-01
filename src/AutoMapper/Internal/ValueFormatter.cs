using System;
using System.Linq;

namespace AutoMapper
{
	internal class ValueFormatter : IValueFormatter
	{
		private readonly IFormatterConfiguration _formatterConfiguration;

		public ValueFormatter(IFormatterConfiguration formatterConfiguration)
		{
			_formatterConfiguration = formatterConfiguration;
		}

		public string FormatValue(ResolutionContext context)
		{
			Type valueType = context.SourceType;
			object valueToFormat = context.SourceValue;
			IFormatterConfiguration typeSpecificFormatterConfig = null;
			string formattedValue = context.SourceValue.ToNullSafeString();

			if (context.PropertyMap != null)
			{
				foreach (IValueFormatter formatter in context.PropertyMap.GetFormatters())
				{
					formattedValue = formatter.FormatValue(context.CreateValueContext(valueToFormat));
					valueToFormat = formattedValue;
				}

				if (_formatterConfiguration.GetTypeSpecificFormatters().TryGetValue(valueType, out typeSpecificFormatterConfig))
				{
					if (!context.PropertyMap.FormattersToSkipContains(typeSpecificFormatterConfig.GetType()))
					{
						var typeSpecificFormatter = new ValueFormatter(typeSpecificFormatterConfig);
						formattedValue = typeSpecificFormatter.FormatValue(context);
						valueToFormat = formattedValue;
					}
				}
			} 
            else if (_formatterConfiguration.GetTypeSpecificFormatters().TryGetValue(valueType, out typeSpecificFormatterConfig))
            {
                var typeSpecificFormatter = new ValueFormatter(typeSpecificFormatterConfig);
                formattedValue = typeSpecificFormatter.FormatValue(context);
                valueToFormat = formattedValue;
            }
            
            foreach (IValueFormatter formatter in _formatterConfiguration.GetFormatters())
			{
				Type formatterType = GetFormatterType(formatter);
				if (CheckPropertyMapSkipList(context, formatterType) &&
					CheckTypeSpecificSkipList(typeSpecificFormatterConfig, formatterType))
				{
					formattedValue = formatter.FormatValue(context.CreateValueContext(valueToFormat));
					valueToFormat = formattedValue;
				}
			}

			return formattedValue;
		}

		private static Type GetFormatterType(IValueFormatter formatter)
		{
			return formatter is DeferredInstantiatedFormatter ? ((DeferredInstantiatedFormatter) formatter).GetFormatterType() : formatter.GetType();
		}

		private static bool CheckTypeSpecificSkipList(IFormatterConfiguration valueFormatter, Type formatterType)
		{
			if (valueFormatter == null)
			{
				return true;
			}

			return !valueFormatter.GetFormatterTypesToSkip().Contains(formatterType);
		}

		private static bool CheckPropertyMapSkipList(ResolutionContext context, Type formatterType)
		{
			if (context.PropertyMap == null)
				return true;

			return !context.PropertyMap.FormattersToSkipContains(formatterType);
		}

	}
}