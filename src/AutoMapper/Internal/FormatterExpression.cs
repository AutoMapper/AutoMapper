using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoMapper
{
	internal class FormatterExpression : IFormatterExpression, IFormatterConfiguration, IFormatterCtorConfigurator, IMappingOptions
	{
		private readonly Func<Type, IValueFormatter> _formatterCtor;
		private readonly IList<IValueFormatter> _formatters = new List<IValueFormatter>();
		private readonly IDictionary<Type, IFormatterConfiguration> _typeSpecificFormatters = new Dictionary<Type, IFormatterConfiguration>();
		private readonly IList<Type> _formattersToSkip = new List<Type>();
		private static readonly Func<string, string, string> PrefixFunc = (src, prefix) => Regex.Replace(src, string.Format("(?:^{0})?(.*)", prefix), "$1");
		private static readonly Func<string, string, string> PostfixFunc = (src, prefix) => Regex.Replace(src, string.Format("(.*)(?:{0})$", prefix), "$1");
		private static readonly Func<string, string, string, string> AliasFunc = (src, original, alias) => Regex.Replace(src, string.Format("^({0})$", original), alias);

		public FormatterExpression(Func<Type, IValueFormatter> formatterCtor)
		{
			_formatterCtor = formatterCtor;
			SourceMemberNamingConvention = new PascalCaseNamingConvention();
			DestinationMemberNamingConvention = new PascalCaseNamingConvention();
			SourceMemberNameTransformer = s => Regex.Replace(s, "(?:^Get)?(.*)", "$1");
            DestinationMemberNameTransformer = s => s;
            AllowNullDestinationValues = true;
		}

		public bool AllowNullDestinationValues { get; set; }
		public INamingConvention SourceMemberNamingConvention { get; set; }
		public INamingConvention DestinationMemberNamingConvention { get; set; }
		public Func<string, string> SourceMemberNameTransformer { get; set; }
        public Func<string, string> DestinationMemberNameTransformer { get; set; }

		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			var formatter = new DeferredInstantiatedFormatter(() => _formatterCtor(typeof(TValueFormatter)));

			AddFormatter(formatter);

			return new FormatterCtorExpression<TValueFormatter>(this);
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			var formatter = new DeferredInstantiatedFormatter(() => _formatterCtor(valueFormatterType));

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
			var valueFormatter = new FormatterExpression(_formatterCtor);

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

	    public IEnumerable<IValueFormatter> GetFormattersToApply(ResolutionContext context)
	    {
	        return GetFormatters(context);
	    }

        private IEnumerable<IValueFormatter> GetFormatters(ResolutionContext context)
        {
            Type valueType = context.SourceType;
            IFormatterConfiguration typeSpecificFormatterConfig;

            if (context.PropertyMap != null)
            {
                foreach (IValueFormatter formatter in context.PropertyMap.GetFormatters())
                {
                    yield return formatter;
                }

                if (GetTypeSpecificFormatters().TryGetValue(valueType, out typeSpecificFormatterConfig))
                {
                    if (!context.PropertyMap.FormattersToSkipContains(typeSpecificFormatterConfig.GetType()))
                    {
                        foreach (var typeSpecificFormatter in typeSpecificFormatterConfig.GetFormattersToApply(context))
                        {
                            yield return typeSpecificFormatter;
                        }
                    }
                }
            }
            else if (GetTypeSpecificFormatters().TryGetValue(valueType, out typeSpecificFormatterConfig))
            {
                foreach (var typeSpecificFormatter in typeSpecificFormatterConfig.GetFormattersToApply(context))
                {
                    yield return typeSpecificFormatter;
                }
            }

            foreach (IValueFormatter formatter in GetFormatters())
            {
                Type formatterType = GetFormatterType(formatter);
                if (CheckPropertyMapSkipList(context, formatterType) &&
                    CheckTypeSpecificSkipList(typeSpecificFormatterConfig, formatterType))
                {
                    yield return formatter;
                }
            }
        }

	    public void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator)
		{
			_formatters.RemoveAt(_formatters.Count - 1);
			_formatters.Add(new DeferredInstantiatedFormatter(instantiator));
		}

		public bool MapNullSourceValuesAsNull
		{
			get { return AllowNullDestinationValues; }
		}

		public void RecognizePrefixes(params string[] prefixes)
		{
			var orig = SourceMemberNameTransformer;

			SourceMemberNameTransformer = val => prefixes.Aggregate(orig(val), PrefixFunc);
		}

		public void RecognizePostfixes(params string[] postfixes)
		{
			var orig = SourceMemberNameTransformer;

			SourceMemberNameTransformer = val => postfixes.Aggregate(orig(val), PostfixFunc);
		}

		public void RecognizeAlias(string original, string alias)
		{
			var orig = SourceMemberNameTransformer;

			SourceMemberNameTransformer = val => AliasFunc(orig(val), original, alias);
		}

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            var orig = DestinationMemberNameTransformer;

            DestinationMemberNameTransformer = val => prefixes.Aggregate(orig(val), PrefixFunc);
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            var orig = DestinationMemberNameTransformer;

            DestinationMemberNameTransformer = val => postfixes.Aggregate(orig(val), PostfixFunc);
        }

        private static Type GetFormatterType(IValueFormatter formatter)
        {
            return formatter is DeferredInstantiatedFormatter ? ((DeferredInstantiatedFormatter)formatter).GetFormatterType() : formatter.GetType();
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

	internal interface IFormatterCtorConfigurator
	{
		void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator);
	}
}