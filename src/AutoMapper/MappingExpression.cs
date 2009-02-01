using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
	internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IFormattingExpression<TSource>
	{
		private readonly TypeMap _typeMap;
		private PropertyMap _propertyMap;

		public MappingExpression(TypeMap typeMap)
		{
			_typeMap = typeMap;
		}

		public IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember,
		                                                  Action<IFormattingExpression<TSource>> memberOptions)
		{
			PropertyInfo destProperty = ReflectionHelper.FindProperty(destinationMember);
			ForDestinationMember(destProperty, memberOptions);
			return new MappingExpression<TSource, TDestination>(_typeMap);
		}

		public void ForAllMembers(Action<IFormattingExpression<TSource>> memberOptions)
		{
			_typeMap.GetPropertyMaps().ForEach(x => ForDestinationMember(x.DestinationProperty, memberOptions));
		}

		public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>() where TOtherSource : TSource
			where TOtherDestination : TDestination
		{
			_typeMap.IncludeDerivedTypes(typeof(TOtherSource), typeof(TOtherDestination));

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

		public void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			AddFormatter(typeof(TValueFormatter));
		}

		public void AddFormatter(Type valueFormatterType)
		{
			var formatter = (IValueFormatter)Activator.CreateInstance(valueFormatterType, true);

			AddFormatter(formatter);
		}

		public void AddFormatter(IValueFormatter formatter)
		{
			_propertyMap.AddFormatter(formatter);
		}

		public void FormatNullValueAs(string nullSubstitute)
		{
			var member = _propertyMap.GetSourceValueResolvers()[0];
			_propertyMap.RemoveLastResolver();
			_propertyMap.ChainResolver(new NullReplacementMethod(member, nullSubstitute));
		}

		public IResolutionExpression<TSource> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
		{
			return ResolveUsing(typeof(TValueResolver));
		}

		public IResolutionExpression<TSource> ResolveUsing(Type valueResolverType)
		{
			var resolver = (IValueResolver)Activator.CreateInstance(valueResolverType, true);

			return ResolveUsing(resolver);
		}

		public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
		{
			_propertyMap.AssignCustomValueResolver(valueResolver);

			return new ResolutionExpression<TSource>(_propertyMap);
		}

		public void MapFrom(Expression<Func<TSource, object>> sourceMember)
		{
			_propertyMap.ResetSourceMemberChain();
			_propertyMap.ChainResolver(new NewMethod<TSource>(sourceMember));
		}

		public void Ignore()
		{
			_propertyMap.Ignore();
		}

		private void ForDestinationMember(PropertyInfo destinationProperty, Action<IFormattingExpression<TSource>> memberOptions)
		{
			_propertyMap = _typeMap.FindOrCreatePropertyMapFor(destinationProperty);

			memberOptions(this);
		}
	}
}
