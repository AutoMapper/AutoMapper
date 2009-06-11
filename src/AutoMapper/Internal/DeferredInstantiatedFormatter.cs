using System;

namespace AutoMapper
{
	internal class DeferredInstantiatedFormatter : IValueFormatter
	{
		private readonly Func<IValueFormatter> _instantiator;

		public DeferredInstantiatedFormatter(Func<IValueFormatter> instantiator)
		{
			_instantiator = instantiator;
		}

		public string FormatValue(ResolutionContext context)
		{
            var formatter = _instantiator();

            return formatter.FormatValue(context);
		}

		public Type GetFormatterType()
		{
		    var formatter = _instantiator();

		    return formatter.GetType();
		}
	}
}