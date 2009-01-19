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

		public void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_configurator.GetProfile(ProfileName).AddFormatter<TValueFormatter>();
		}

		public void AddFormatter(Type valueFormatterType)
		{
			_configurator.GetProfile(ProfileName).AddFormatter(valueFormatterType);
		}

		public void AddFormatter(IValueFormatter formatter)
		{
			_configurator.GetProfile(ProfileName).AddFormatter(formatter);
		}

		public void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			_configurator.GetProfile(ProfileName).AddFormatExpression(formatExpression);
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_configurator.GetProfile(ProfileName).SkipFormatter<TValueFormatter>();
		}

		public IFormatterExpression ForSourceType<TSource>()
		{
			return _configurator.GetProfile(ProfileName).ForSourceType<TSource>();
		}
      
		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			var map = _configurator.CreateMap<TSource, TDestination>();

			return map.WithProfile(ProfileName);
		}
	}
}