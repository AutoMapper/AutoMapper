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
		}

		protected virtual string ProfileName
		{
			get { return _profileName; }
		}

		protected internal virtual void Configure()
		{
			// override in a derived class for custom configuration behavior
		}

		public void Initialize(Configuration configurator)
		{
			_configurator = configurator;
		}

		public bool AllowNullDestinationValues
		{
			get { return GetProfile().AllowNullDestinationValues; }
			set { GetProfile().AllowNullDestinationValues = value; }
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

		private FormatterExpression GetProfile()
		{
			return _configurator.GetProfile(ProfileName);
		}
	}
}