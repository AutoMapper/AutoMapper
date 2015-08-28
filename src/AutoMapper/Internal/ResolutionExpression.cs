namespace AutoMapper.Internal
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>,
        IResolverConfigurationExpression
    {
        private readonly Type _sourceType;
        private readonly PropertyMap _propertyMap;

        public ResolutionExpression(Type sourceType, PropertyMap propertyMap)
        {
            _sourceType = sourceType;
            _propertyMap = propertyMap;
        }

        public void FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            var body = sourceMember.Body as MemberExpression;
            if (body != null)
            {
                _propertyMap.SourceMember = body.Member;
            }
            var func = sourceMember.Compile();
            _propertyMap.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(r => func((TSource) r.Value)));
        }

        public void FromMember(string sourcePropertyName)
        {
            _propertyMap.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
            _propertyMap.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));
        }

        IResolutionExpression IResolverConfigurationExpression.ConstructedBy(Func<IValueResolver> constructor)
        {
            return ConstructedBy(constructor);
        }

        public IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor)
        {
            _propertyMap.ChainConstructorForResolver(new DeferredInstantiatedResolver(ctxt => constructor()));

            return this;
        }
    }

    public class ResolutionExpression : ResolutionExpression<object>
    {
        public ResolutionExpression(Type sourceType, PropertyMap propertyMap) : base(sourceType, propertyMap)
        {
        }
    }

    public class ResolutionExpression<TSource, TValueResolver> :
        IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        private readonly PropertyMap _propertyMap;
        private readonly Type _sourceType;

        public ResolutionExpression(Type sourceType, PropertyMap propertyMap)
        {
            _sourceType = sourceType;
            _propertyMap = propertyMap;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(
            Expression<Func<TSource, object>> sourceMember)
        {
            var body = sourceMember.Body as MemberExpression;
            if (body != null)
            {
                _propertyMap.SourceMember = body.Member;
            }
            var func = sourceMember.Compile();
            _propertyMap.ChainTypeMemberForResolver(new DelegateBasedResolver<TSource>(r => func((TSource) r.Value)));

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName)
        {
            _propertyMap.SourceMember = _sourceType.GetMember(sourcePropertyName)[0];
            _propertyMap.ChainTypeMemberForResolver(new PropertyNameResolver(_sourceType, sourcePropertyName));

            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor)
        {
            _propertyMap.ChainConstructorForResolver(new DeferredInstantiatedResolver(ctxt => constructor()));

            return this;
        }
    }
}