using System;

namespace AutoMapper
{
	internal class DeferredInstantiatedFormatter : IValueFormatter
	{
		private readonly Func<IValueFormatter> _instantiator;
		private IValueFormatter _formatter;

		public DeferredInstantiatedFormatter(Func<IValueFormatter> instantiator)
		{
			_instantiator = instantiator;
		}

		public string FormatValue(ResolutionContext context)
		{
			Initialize();

			return _formatter.FormatValue(context);
		}

		public Type GetFormatterType()
		{
			Initialize();

			return _formatter.GetType();
		}

		private void Initialize()
		{
			if (_formatter == null)
			{
				_formatter = _instantiator();
			}
		}
	}
}