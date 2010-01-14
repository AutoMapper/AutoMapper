using System;

namespace AutoMapper
{
	public class Profile : IProfileExpression
	{
		private readonly string _profileName;
		private Configuration _configurator;

		internal Profile(string profileName)
		{
			_profileName = profileName;
		}

		protected Profile()
		{
		    _profileName = GetType().FullName;
		}

		protected virtual string ProfileName
		{
			get { return _profileName; }
		}

		public bool AllowNullDestinationValues
		{
			get { return GetProfile().AllowNullDestinationValues; }
			set { GetProfile().AllowNullDestinationValues = value; }
		}

		public INamingConvention SourceMemberNamingConvention
		{
			get { return GetProfile().SourceMemberNamingConvention; } 
			set { GetProfile().SourceMemberNamingConvention = value; }
		}

		public INamingConvention DestinationMemberNamingConvention
		{
			get { return GetProfile().DestinationMemberNamingConvention; }
			set { GetProfile().DestinationMemberNamingConvention = value; }
		}

		public Func<string, string> SourceMemberNameTransformer
		{
			get { return GetProfile().SourceMemberNameTransformer; }
			set { GetProfile().SourceMemberNameTransformer = value; }
		}


		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			return GetProfile().AddFormatter<TValueFormatter>();
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			return GetProfile().AddFormatter(valueFormatterType);
		}

		public void AddFormatter(IValueFormatter formatter)
		{
			GetProfile().AddFormatter(formatter);
		}

		public void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			GetProfile().AddFormatExpression(formatExpression);
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			GetProfile().SkipFormatter<TValueFormatter>();
		}

		public IFormatterExpression ForSourceType<TSource>()
		{
			return GetProfile().ForSourceType<TSource>();
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			var map = _configurator.CreateMap<TSource, TDestination>();

			return map.WithProfile(ProfileName);
		}

		public IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
			var map = _configurator.CreateMap(sourceType, destinationType);

			return map.WithProfile(ProfileName);
		}

		public void RecognizeAlias(string original, string alias)
		{
			GetProfile().RecognizeAlias(original, alias);
		}

		public void RecognizePrefixes(params string[] prefixes)
		{
			GetProfile().RecognizePrefixes(prefixes);
		}

		public void RecognizePostfixes(params string[] postfixes)
		{
			GetProfile().RecognizePostfixes(postfixes);
		}

		protected internal virtual void Configure()
		{
			// override in a derived class for custom configuration behavior
		}

		public void Initialize(Configuration configurator)
		{
			_configurator = configurator;
		}

		private FormatterExpression GetProfile()
		{
			return _configurator.GetProfile(ProfileName);
		}
	}
}