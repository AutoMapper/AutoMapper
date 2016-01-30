namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>,
        IResolverConfigurationExpression
    {
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public void FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            _propertyMapActions.Add(pm =>
            {
                var body = sourceMember.Body as MemberExpression;
                if (body != null)
                {
                    pm.SourceMember = body.Member;
                }
                var func = sourceMember.Compile();
                pm.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(r => func((TSource) r.Value)));
            });
        }

        public void FromMember(string sourcePropertyName)
        {
            _propertyMapActions.Add(pm =>
            {
                pm.SourceMember = typeof(TSource).GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(typeof(TSource), sourcePropertyName));
            });
        }

        IResolutionExpression IResolverConfigurationExpression.ConstructedBy(Func<IValueResolver> constructor)
        {
            return ConstructedBy(constructor);
        }

        public IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor)
        {
            _propertyMapActions.Add(pm => pm.ChainConstructorForResolver(new DeferredInstantiatedResolver(ctxt => constructor())));

            return this;
        }

        public void Configure(PropertyMap propertyMap)
        {
            foreach (var action in _propertyMapActions)
            {
                action(propertyMap);
            }
        }
    }

    public class ResolutionExpression : ResolutionExpression<object>
    {
    }

    public class ResolutionExpression<TSource, TValueResolver> :
        IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(
            Expression<Func<TSource, object>> sourceMember)
        {
            _propertyMapActions.Add(pm =>
            {
                var body = sourceMember.Body as MemberExpression;
                if (body != null)
                {
                    pm.SourceMember = body.Member;
                }
                var func = sourceMember.Compile();
                pm.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(r => func((TSource) r.Value)));
            });

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName)
        {
            _propertyMapActions.Add(pm =>
            {
                pm.SourceMember = typeof(TSource).GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(typeof(TSource), sourcePropertyName));
            });

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor)
        {
            _propertyMapActions.Add(pm => pm.ChainConstructorForResolver(new DeferredInstantiatedResolver(ctxt => constructor())));

            return this;
        }

        public void Configure(PropertyMap propertyMap)
        {
            foreach (var action in _propertyMapActions)
            {
                action(propertyMap);
            }
        }
    }
}