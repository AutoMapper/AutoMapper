using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
	internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IMemberConfigurationExpression<TSource>, IFormatterCtorConfigurator
	{
		private readonly TypeMap _typeMap;
		private readonly Func<Type, IValueFormatter> _formatterCtor;
		private readonly Func<Type, IValueResolver> _resolverCtor;
		private PropertyMap _propertyMap;

		public MappingExpression(TypeMap typeMap, Func<Type, IValueFormatter> formatterCtor, Func<Type, IValueResolver> resolverCtor)
		{
			_typeMap = typeMap;
			_formatterCtor = formatterCtor;
			_resolverCtor = resolverCtor;
		}

		public IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember,
		                                                           Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			IMemberAccessor destProperty = ReflectionHelper.FindProperty(destinationMember);
			ForDestinationMember(destProperty, memberOptions);
			return new MappingExpression<TSource, TDestination>(_typeMap, _formatterCtor, _resolverCtor);
		}

		public void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			_typeMap.GetPropertyMaps().ForEach(x => ForDestinationMember(x.DestinationProperty, memberOptions));
		}

		public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>() where TOtherSource : TSource
			where TOtherDestination : TDestination
		{
			_typeMap.IncludeDerivedTypes(typeof (TOtherSource), typeof (TOtherDestination));

			return this;
		}

		public IMappingExpression<TSource, TDestination> WithProfile(string profileName)
		{
			_typeMap.Profile = profileName;

			return this;
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_propertyMap.AddFormatterToSkip<TValueFormatter>();
		}

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

		public void AddFormatter(IValueFormatter formatter)
		{
			_propertyMap.AddFormatter(formatter);
		}

		public void FormatNullValueAs(string nullSubstitute)
		{
			_propertyMap.SetNullSubstitute(nullSubstitute);
		}

		public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
		{
			var resolver = new DeferredInstantiatedResolver(() => _resolverCtor(typeof(TValueResolver)));

			ResolveUsing(resolver);

			return new ResolutionExpression<TSource, TValueResolver>(_propertyMap);
		}

		public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
		{
			var resolver = new DeferredInstantiatedResolver(() => _resolverCtor(valueResolverType));

			ResolveUsing(resolver);

			return new ResolutionExpression<TSource>(_propertyMap);
		}

		public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
		{
			_propertyMap.AssignCustomValueResolver(valueResolver);

			return new ResolutionExpression<TSource>(_propertyMap);
		}

		public void MapFrom(Func<TSource, object> sourceMember)
		{
			_propertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(sourceMember));
		}

		public void Ignore()
		{
			_propertyMap.Ignore();
		}

		public void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator)
		{
			_propertyMap.RemoveLastFormatter();
			_propertyMap.AddFormatter(new DeferredInstantiatedFormatter(instantiator));
		}

		public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
		{
			_typeMap.UseCustomMapper(source => mappingFunction((TSource) source));
		}

		public void ConvertUsing(Func<ITypeConverter> converter)
		{
			_typeMap.UseCustomMapper(source => converter().Convert(source));
		}

		public void ConvertUsing(ITypeConverter converter)
		{
			_typeMap.UseCustomMapper(converter.Convert);
		}

		private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			_propertyMap = _typeMap.FindOrCreatePropertyMapFor(destinationProperty);

			memberOptions(this);
		}
	}
}
