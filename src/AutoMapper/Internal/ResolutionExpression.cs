namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>,
        IResolverConfigurationExpression
    {
        private readonly Type _sourceType;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public ResolutionExpression(Type sourceType)
        {
            _sourceType = sourceType;
        }

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
                pm.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));
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
        public ResolutionExpression(Type sourceType) : base(sourceType)
        {
        }
    }

    public class ResolutionExpression<TSource, TValueResolver> :
        IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        private readonly Type _sourceType;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public ResolutionExpression(Type sourceType)
        {
            _sourceType = sourceType;
        }

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
                pm.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));
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