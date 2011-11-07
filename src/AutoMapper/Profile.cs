using System;

namespace AutoMapper
{
	public class Profile : IProfileExpression
	{
        private ConfigurationStore _configurator;

		internal Profile(string profileName)
		{
			ProfileName = profileName;
		}

		protected Profile()
		{
		    ProfileName = GetType().FullName;
		}

	    public virtual string ProfileName { get; private set; }

	    public bool AllowNullDestinationValues
		{
			get { return GetProfile().AllowNullDestinationValues; }
			set { GetProfile().AllowNullDestinationValues = value; }
		}

	    public bool AllowNullCollections
		{
            get { return GetProfile().AllowNullCollections; }
            set { GetProfile().AllowNullCollections = value; }
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

        public Func<string, string> DestinationMemberNameTransformer
        {
            get { return GetProfile().DestinationMemberNameTransformer; }
            set { GetProfile().DestinationMemberNameTransformer = value; }
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
			var map = _configurator.CreateMap<TSource, TDestination>(ProfileName);

			return map;
		}

		public IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
			var map = _configurator.CreateMap(sourceType, destinationType, ProfileName);

			return map;
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

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile().RecognizeDestinationPrefixes(prefixes);
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile().RecognizeDestinationPostfixes(postfixes);
        }

	    public void AddGlobalIgnore(string propertyNameStartingWith)
	    {
	        _configurator.AddGlobalIgnore(propertyNameStartingWith);
	    }

	    protected internal virtual void Configure()
		{
			// override in a derived class for custom configuration behavior
		}

        public void Initialize(ConfigurationStore configurator)
		{
			_configurator = configurator;
		}

		private FormatterExpression GetProfile()
		{
			return _configurator.GetProfile(ProfileName);
		}
	}
}