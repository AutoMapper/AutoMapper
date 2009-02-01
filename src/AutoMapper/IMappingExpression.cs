using System;
using System.Linq.Expressions;

namespace AutoMapper
{
	public interface IMappingExpression<TSource, TDestination>
	{
		IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember, Action<IMemberConfigurationExpression<TSource>> memberOptions);
		void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions);
		IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>() where TOtherSource : TSource where TOtherDestination : TDestination;
		IMappingExpression<TSource, TDestination> WithProfile(string profileName);
	}

	public interface IMemberConfigurationExpression<TSource>
	{
		void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		void AddFormatter(Type valueFormatterType);
		void AddFormatter(IValueFormatter formatter);
		void FormatNullValueAs(string nullSubstitute);
		IResolutionExpression<TSource> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver;
		IResolutionExpression<TSource> ResolveUsing(Type valueResolverType);
		IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver);
		void MapFrom(Func<TSource, object> sourceMember);
		void Ignore();
	}

	public interface IResolutionExpression<TSource>
	{
		void FromMember(Func<TSource, object> sourceMember);
	}
}