using System;
using System.Reflection;

namespace AutoMapper
{
	public interface IFormatterExpression
	{
		void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		void AddFormatter(Type valueFormatterType);
		void AddFormatter(IValueFormatter formatter);
		void AddFormatExpression(Func<ResolutionContext, string> formatExpression);
		void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		IFormatterExpression ForSourceType<TSource>();
	}

	public interface IProfileExpression : IFormatterExpression
	{
		IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
	}

	public interface IConfigurationExpression : IProfileExpression
	{
		IProfileExpression CreateProfile(string profileName);
		void CreateProfile(string profileName, Action<IProfileExpression> initializationExpression);
		void AddProfile(Profile profile);
		void AddProfile<TProfile>() where TProfile : Profile, new();
		void SelfConfigure(Assembly assembly);
	}
}
