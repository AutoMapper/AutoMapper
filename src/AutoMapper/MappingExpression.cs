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
			_propertyMap.FormatNullValueAs(nullSubstitute);
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
			TypeMember[] modelTypeMembers = BuildModelTypeMembers(sourceMember);

			_propertyMap.ChainTypeMembers(modelTypeMembers);
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

		private static TypeMember[] BuildModelTypeMembers(LambdaExpression lambdaExpression)
		{
			Expression expressionToCheck = lambdaExpression;
			var typeMembers = new List<TypeMember>();

			bool done = false;

			while (!done)
			{
				switch (expressionToCheck.NodeType)
				{
					case ExpressionType.Convert:
						expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
						break;
					case ExpressionType.Lambda:
						expressionToCheck = lambdaExpression.Body;
						break;
					case ExpressionType.MemberAccess:
						var memberExpression = ((MemberExpression)expressionToCheck);
						var propertyInfo = memberExpression.Member as PropertyInfo;
						if (propertyInfo != null)
						{
							typeMembers.Add(new PropertyMember(propertyInfo));
						}
						expressionToCheck = memberExpression.Expression;
						break;
					case ExpressionType.Call:
						var methodCallExpression = ((MethodCallExpression)expressionToCheck);
						typeMembers.Add(new MethodMember(methodCallExpression.Method));
						expressionToCheck = methodCallExpression.Object;
						break;
					default:
						done = true;
						break;
				}
				if (expressionToCheck == null)
					done = true;
			}

			// LINQ lists members in reverse
			typeMembers.Reverse();

			return typeMembers.ToArray();
		}

		private class ResolutionExpression<TResolutionModel> : IResolutionExpression<TResolutionModel>
		{
			private readonly PropertyMap _propertyMap;

			public ResolutionExpression(PropertyMap propertyMap)
			{
				_propertyMap = propertyMap;
			}

			public void FromMember(Expression<Func<TResolutionModel, object>> sourceMember)
			{
				_propertyMap.ChainTypeMembersForResolver(BuildModelTypeMembers(sourceMember));
			}
		}
	}
}