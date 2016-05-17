namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class ResolutionExpression<TSource> : IResolverConfigurationExpression<TSource>, IResolutionExpression<TSource>
    {
        private readonly ValueResolverConfiguration _config;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public ResolutionExpression(ValueResolverConfiguration config)
        {
            _config = config;

            _propertyMapActions.Add(pm => pm.ValueResolverConfig = _config);
        }

        public void FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            _config.SourceMember = sourceMember;
        }

        public void FromMember(string sourcePropertyName)
        {
            _config.SourceMemberName = sourcePropertyName;
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
        public ResolutionExpression(ValueResolverConfiguration config) : base(config)
        {
        }
    }
}