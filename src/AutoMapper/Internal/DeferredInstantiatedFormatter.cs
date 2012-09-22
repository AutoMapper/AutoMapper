using System;

namespace AutoMapper
{
	public class DeferredInstantiatedFormatter : IValueFormatter
	{
        private readonly Func<ResolutionContext, IValueFormatter> _instantiator;

        public DeferredInstantiatedFormatter(Func<ResolutionContext, IValueFormatter> instantiator)
		{
			_instantiator = instantiator;
		}

		public string FormatValue(ResolutionContext context)
		{
            var formatter = _instantiator(context);

            return formatter.FormatValue(context);
		}

		public Type GetFormatterType(ResolutionContext context)
		{
		    var formatter = _instantiator(context);

		    return formatter.GetType();
		}
	}
}