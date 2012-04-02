using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
    internal class FormatterExpression : IFormatterExpression, IFormatterConfiguration, IFormatterCtorConfigurator, IMappingOptions
	{
		private readonly Func<Type, IValueFormatter> _formatterCtor;
		private readonly IList<IValueFormatter> _formatters = new List<IValueFormatter>();
		private readonly IDictionary<Type, IFormatterConfiguration> _typeSpecificFormatters = new Dictionary<Type, IFormatterConfiguration>();
		private readonly IList<Type> _formattersToSkip = new List<Type>();
	    private readonly HashSet<string> _prefixes = new HashSet<string>();
	    private readonly HashSet<string> _postfixes = new HashSet<string>();
	    private readonly HashSet<string> _destinationPrefixes = new HashSet<string>();
	    private readonly HashSet<string> _destinationPostfixes = new HashSet<string>();
        private readonly HashSet<AliasedMember> _aliases = new HashSet<AliasedMember>();

	    public FormatterExpression(Func<Type, IValueFormatter> formatterCtor)
		{
			_formatterCtor = formatterCtor;
			SourceMemberNamingConvention = new PascalCaseNamingConvention();
			DestinationMemberNamingConvention = new PascalCaseNamingConvention();
		    RecognizePrefixes("Get");
			AllowNullDestinationValues = true;
		}

		public bool AllowNullDestinationValues { get; set; }
		public bool AllowNullCollections { get; set; }
		public INamingConvention SourceMemberNamingConvention { get; set; }
		public INamingConvention DestinationMemberNamingConvention { get; set; }
        public IEnumerable<string> Prefixes { get { return _prefixes; } }
        public IEnumerable<string> Postfixes { get { return _postfixes; } }
        public IEnumerable<string> DestinationPrefixes { get { return _destinationPrefixes; } }
        public IEnumerable<string> DestinationPostfixes { get { return _destinationPostfixes; } }
        public IEnumerable<AliasedMember> Aliases { get { return _aliases; } }

		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			var formatter = new DeferredInstantiatedFormatter(BuildCtor(typeof(TValueFormatter)));

			AddFormatter(formatter);

			return new FormatterCtorExpression<TValueFormatter>(this);
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			var formatter = new DeferredInstantiatedFormatter(BuildCtor(valueFormatterType));

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
				Type formatterType = GetFormatterType(formatter, context);
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
			_formatters.Add(new DeferredInstantiatedFormatter(ctxt => instantiator()));
		}

		public bool MapNullSourceValuesAsNull
		{
			get { return AllowNullDestinationValues; }
		}

		public bool MapNullSourceCollectionsAsNull
		{
			get { return AllowNullCollections; }
		}

		public void RecognizePrefixes(params string[] prefixes)
		{
		    foreach (var prefix in prefixes)
		    {
                _prefixes.Add(prefix);
		    }
		}

		public void RecognizePostfixes(params string[] postfixes)
		{
		    foreach (var postfix in postfixes)
		    {
                _postfixes.Add(postfix);
		    }
		}

		public void RecognizeAlias(string original, string alias)
		{
		    _aliases.Add(new AliasedMember(original, alias));
		}

		public void RecognizeDestinationPrefixes(params string[] prefixes)
		{
		    foreach (var prefix in prefixes)
		    {
		        _destinationPrefixes.Add(prefix);
		    }
		}

		public void RecognizeDestinationPostfixes(params string[] postfixes)
		{
		    foreach (var postfix in postfixes)
		    {
		        _destinationPostfixes.Add(postfix);
		    }
		}

		private static Type GetFormatterType(IValueFormatter formatter, ResolutionContext context)
		{
			return formatter is DeferredInstantiatedFormatter ? ((DeferredInstantiatedFormatter)formatter).GetFormatterType(context) : formatter.GetType();
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

		private Func<ResolutionContext, IValueFormatter> BuildCtor(Type type)
		{
			return context =>
			{
				if (context.Options.ServiceCtor != null)
				{
					var obj = context.Options.ServiceCtor(type);
					if (obj != null)
						return (IValueFormatter)obj;
				}
				return (IValueFormatter)_formatterCtor(type);
			};
		}

		private static string DefaultPrefixTransformer(string src, string prefix)
		{
			return src != null
				&& !String.IsNullOrEmpty(prefix)
				&& src.StartsWith(prefix, StringComparison.Ordinal)
					? src.Substring(prefix.Length)
					: src;
		}

		private static string DefaultPostfixTransformer(string src, string postfix)
		{
			return src != null
				&& !String.IsNullOrEmpty(postfix)
				&& src.EndsWith(postfix, StringComparison.Ordinal)
					? src.Remove(src.Length - postfix.Length)
					: src;
		}

		private static string DefaultAliasTransformer(string src, string original, string @alias)
		{
			return src != null
				&& !String.IsNullOrEmpty(original)
				&& String.Equals(src, original, StringComparison.Ordinal)
					? @alias
					: src;
		}

		private static string DefaultSourceMemberNameTransformer(string src)
		{
			return src != null
				&& src.StartsWith("Get", StringComparison.Ordinal)
					? src.Substring(3) // Removes initial "Get"
					: src;
		}
	}

	internal interface IFormatterCtorConfigurator
	{
		void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator);
	}
}