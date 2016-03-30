namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    public interface IMemberConfiguration
    {
        void Configure(TypeMap typeMap);
        IMemberAccessor DestinationMember { get; }
    }

    public class MemberConfigurationExpression<TSource, TMember> : IMemberConfigurationExpression<TSource, TMember>, IMemberConfiguration
    {
        private readonly IMemberAccessor _destinationMember;
        private readonly Type _sourceType;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(IMemberAccessor destinationMember, Type sourceType)
        {
            _destinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public IMemberAccessor DestinationMember => _destinationMember;

        public void NullSubstitute(TMember nullSubstitute)
        {
            _propertyMapActions.Add(pm => pm.SetNullSubstitute(nullSubstitute));
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver));

            var expression = new ResolutionExpression<TSource, TValueResolver>(_sourceType, config);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
        {
            var config = new ValueResolverConfiguration(valueResolverType);

            var expression = new ResolutionExpression<TSource>(_sourceType, config);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
        {
            var config = new ValueResolverConfiguration(valueResolver);

            var expression = new ResolutionExpression<TSource>(_sourceType, config);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public void ResolveUsing<TSourceMember>(Func<TSource, TSourceMember> resolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomExpression<TSource, TSourceMember>((s, c) => resolver(s)));
        }

        public void ResolveUsing<TSourceMember>(Func<TSource, ResolutionContext, TSourceMember> resolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomExpression(resolver));
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            _propertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(sourceMember));
        }

        public void MapFrom<TSourceMember>(string sourceMember)
        {
            _propertyMapActions.Add(pm => pm.SourceMemberName = sourceMember);
        }

        public void MapFrom(string sourceMember)
        {
            MapFrom<object>(sourceMember);
        }

        public void UseValue<TValue>(TValue value)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValue(value));
        }

        public void UseValue(object value)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValue(value));
        }

        public void Condition(Func<TSource, object, TMember, ResolutionContext, bool> condition)
        {
            _propertyMapActions.Add(pm =>
            {
                pm.ApplyCondition((src, dest, ctxt) => condition((TSource) ctxt.SourceValue, src, (TMember) dest, ctxt));
            });
        }

        public void Condition(Func<TSource, object, TMember, bool> condition)
        {
            _propertyMapActions.Add(pm => pm.ApplyCondition((src, dest, ctxt) => condition((TSource)ctxt.SourceValue, src, (TMember)dest)));
        }

        public void Condition(Func<TSource, bool> condition)
        {
            _propertyMapActions.Add(pm => pm.ApplyCondition((src, dest, ctxt) => condition((TSource)ctxt.SourceValue)));
        }

        public void PreCondition(Func<TSource, bool> condition)
        {
            PreCondition(context => condition((TSource)context.SourceValue));
        }

        public void PreCondition(Func<ResolutionContext, bool> condition)
        {
            _propertyMapActions.Add(pm => pm.ApplyPreCondition(condition));
        }

        public void ExplicitExpansion()
        {
            _propertyMapActions.Add(pm => pm.ExplicitExpansion = true);
        }

        public void Ignore()
        {
            _propertyMapActions.Add(pm => pm.Ignore());
        }

        public void UseDestinationValue()
        {
            _propertyMapActions.Add(pm => pm.UseDestinationValue = true);
        }

        public void DoNotUseDestinationValue()
        {
            _propertyMapActions.Add(pm => pm.UseDestinationValue = false);
        }

        public void SetMappingOrder(int mappingOrder)
        {
            _propertyMapActions.Add(pm => pm.SetMappingOrder(mappingOrder));
        }

        public void Configure(TypeMap typeMap)
        {
            var propertyMap = typeMap.FindOrCreatePropertyMapFor(_destinationMember);

            foreach (var action in _propertyMapActions)
            {
                action(propertyMap);
            }
        }
    }
}