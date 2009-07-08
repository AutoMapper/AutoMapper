using System;

namespace AutoMapper
{
	internal class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>
	{
		private readonly PropertyMap _propertyMap;

		public ResolutionExpression(PropertyMap propertyMap)
		{
			_propertyMap = propertyMap;
		}

		public void FromMember(Func<TSource, object> sourceMember)
		{
			_propertyMap.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(sourceMember));
		}

		public void FromMember(string sourcePropertyName)
		{
			_propertyMap.ChainTypeMemberForResolver(new PropertyNameResolver<TSource>(sourcePropertyName));
		}

		public IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor)
		{
			_propertyMap.ChainConstructorForResolver(new DeferredInstantiatedResolver(constructor));

			return this;
		}
	}

	internal class ResolutionExpression<TSource, TValueResolver> : IResolverConfigurationExpression<TSource, TValueResolver>
		where TValueResolver : IValueResolver
	{
		private readonly PropertyMap _propertyMap;

		public ResolutionExpression(PropertyMap propertyMap)
		{
			_propertyMap = propertyMap;
		}

		public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Func<TSource, object> sourceMember)
		{
			_propertyMap.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(sourceMember));

			return this;
		}

		public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName)
		{
			_propertyMap.ChainTypeMemberForResolver(new PropertyNameResolver<TSource>(sourcePropertyName));

			return this;
		}

		public IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor)
		{
			_propertyMap.ChainConstructorForResolver(new DeferredInstantiatedResolver(() => constructor()));

			return this;
		}
	}
}