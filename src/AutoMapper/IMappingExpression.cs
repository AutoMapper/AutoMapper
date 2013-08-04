using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    /// <summary>
    /// Mapping configuration options for non-generic maps
    /// </summary>
    public interface IMappingExpression
    {
        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// </summary>
        /// <typeparam name="TTypeConverter">Type converter type</typeparam>
        void ConvertUsing<TTypeConverter>();

        /// <summary>
        /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
        /// Use this method if you need to specify the converter type at runtime
        /// </summary>
        /// <param name="typeConverterType">Type converter type</param>
        void ConvertUsing(Type typeConverterType);

        /// <summary>
        /// Override the destination type mapping for looking up configuration and instantiation
        /// </summary>
        /// <param name="typeOverride"></param>
        void As(Type typeOverride);

        /// <summary>
        /// Assign a profile to the current type map
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Itself</returns>
        IMappingExpression WithProfile(string profileName);

        /// <summary>
        /// Customize individual members
        /// </summary>
        /// <param name="name">Name of the member</param>
        /// <param name="memberOptions">Callback for configuring member</param>
        /// <returns>Itself</returns>
        IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions);
    }

    public interface IMappingExpression<TSource, TDestination>
    {
        IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember, Action<IMemberConfigurationExpression<TSource>> memberOptions);
        IMappingExpression<TSource, TDestination> ForMember(string name, Action<IMemberConfigurationExpression<TSource>> memberOptions);
        void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions);
        IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination;
        IMappingExpression<TSource, TDestination> WithProfile(string profileName);
        void ConvertUsing(Func<TSource, TDestination> mappingFunction);
        void ConvertUsing(ITypeConverter<TSource, TDestination> converter);
        void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction);
        IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);
        IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor);
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<ResolutionContext, TDestination> ctor);
        void As<T>();
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);
        IMappingExpression<TSource, TDestination> ConstructUsingServiceLocator();
        IMappingExpression<TDestination, TSource> ReverseMap();
        IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression<TSource>> memberOptions);
    }

    public interface IMemberConfigurationExpression
    {
        void MapFrom(string sourceMember);
        IResolutionExpression ResolveUsing(IValueResolver valueResolver);
        IResolverConfigurationExpression ResolveUsing(Type valueResolverType);
        IResolverConfigurationExpression ResolveUsing<TValueResolver>();
        void Ignore();
    }

    public interface ISourceMemberConfigurationExpression<TSource>
    {
        void Ignore();
    }

    public interface IMemberConfigurationExpression<TSource>
    {
        void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        IFormatterCtorExpression AddFormatter(Type valueFormatterType);
        void AddFormatter(IValueFormatter formatter);
        void NullSubstitute(object nullSubstitute);
        IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver;
        IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType);
        IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver);
        void ResolveUsing(Func<TSource, object> resolver);
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);
        void Ignore();
        void SetMappingOrder(int mappingOrder);
        void UseDestinationValue();
        void UseValue<TValue>(TValue value);
        void UseValue(object value);
        void Condition(Func<TSource, bool> condition);
        void Condition(Func<ResolutionContext, bool> condition);
    }

    public interface IResolutionExpression
    {
        void FromMember(string sourcePropertyName);
    }

    public interface IResolverConfigurationExpression : IResolutionExpression
    {
        IResolutionExpression ConstructedBy(Func<IValueResolver> constructor);
    }

    public interface IResolutionExpression<TSource> : IResolutionExpression
    {
        void FromMember(Expression<Func<TSource, object>> sourceMember);
    }

    public interface IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Expression<Func<TSource, object>> sourceMember);
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName);
        IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor);
    }

    public interface IResolverConfigurationExpression<TSource> : IResolutionExpression<TSource>
    {
        IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor);
    }
}
