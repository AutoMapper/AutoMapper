using System;
using System.Reflection;

namespace AutoMapper
{
	public interface IFormatterExpression
	{
		IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		IFormatterCtorExpression AddFormatter(Type valueFormatterType);
		void AddFormatter(IValueFormatter formatter);
		void AddFormatExpression(Func<ResolutionContext, string> formatExpression);
		void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		IFormatterExpression ForSourceType<TSource>();
		bool AllowNullDestinationValues { get; set; }
        bool AllowNullCollections { get; set; }
    }

	public interface IFormatterCtorExpression
	{
		void ConstructedBy(Func<IValueFormatter> constructor);
	}

	public interface IFormatterCtorExpression<TValueFormatter>
		where TValueFormatter : IValueFormatter
	{
		void ConstructedBy(Func<TValueFormatter> constructor);
	}

	public interface IProfileExpression : IFormatterExpression, IMappingOptions
	{
		IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList source);
        IMappingExpression CreateMap(Type sourceType, Type destinationType);
        IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList source);
		void RecognizePrefixes(params string[] prefixes);
		void RecognizePostfixes(params string[] postfixes);
		void RecognizeAlias(string original, string alias);
        void RecognizeDestinationPrefixes(params string[] prefixes);
        void RecognizeDestinationPostfixes(params string[] postfixes);
        void AddGlobalIgnore(string propertyNameStartingWith);
    }

	public interface IConfiguration : IProfileExpression
	{
		IProfileExpression CreateProfile(string profileName);
		void CreateProfile(string profileName, Action<IProfileExpression> initializationExpression);
		void AddProfile(Profile profile);
		void AddProfile<TProfile>() where TProfile : Profile, new();
		void ConstructServicesUsing(Func<Type, object> constructor);
		void Seal();
	}
}
