namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    public class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>,
        IResolverConfigurationExpression
    {
        private readonly Type _sourceType;
        private readonly ValueResolverConfiguration _config;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public ResolutionExpression(Type sourceType, ValueResolverConfiguration config)
        {
            _sourceType = sourceType;
            _config = config;

            _propertyMapActions.Add(pm => pm.ValueResolverConfig = _config);
        }

        public void FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            _config.SourceMember = sourceMember;

            _propertyMapActions.Add(pm =>
            {
                var body = sourceMember.Body as MemberExpression;
                if (body != null)
                {
                    pm.SourceMember = body.Member;
                }
                var func = sourceMember.Compile();
                pm.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource, object>((o, c) => func((TSource)o)));
            });
        }

        public void FromMember(string sourcePropertyName)
        {
            _config.SourceMemberName = sourcePropertyName;

            _propertyMapActions.Add(pm =>
            {
                pm.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));
            });
        }

        IResolutionExpression IResolverConfigurationExpression.ConstructedBy(Func<IValueResolver> constructor)
        {
            _config.Constructor = constructor;

            return ConstructedBy(constructor);
        }

        public IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor)
        {
            _config.Constructor = constructor;

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
        public ResolutionExpression(Type sourceType, ValueResolverConfiguration config) : base(sourceType, config)
        {
        }
    }

    public class ResolutionExpression<TSource, TValueResolver> :
        IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        private readonly Type _sourceType;
        private readonly ValueResolverConfiguration _config;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public ResolutionExpression(Type sourceType, ValueResolverConfiguration config)
        {
            _sourceType = sourceType;
            _config = config;
            _propertyMapActions.Add(pm => pm.ValueResolverConfig = _config);
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(
            Expression<Func<TSource, object>> sourceMember)
        {
            _config.SourceMember = sourceMember;

            _propertyMapActions.Add(pm =>
            {
                var body = sourceMember.Body as MemberExpression;
                if (body != null)
                {
                    pm.SourceMember = body.Member;
                }
                var func = sourceMember.Compile();
                pm.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource, object>((o, c) => func((TSource)o)));
            });

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName)
        {
            _config.SourceMemberName = sourcePropertyName;

            _propertyMapActions.Add(pm =>
            {
                pm.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
                pm.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));
            });

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor)
        {
            _config.Constructor = () => constructor();

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