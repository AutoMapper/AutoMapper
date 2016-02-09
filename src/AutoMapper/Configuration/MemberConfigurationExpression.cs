namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    public class MemberConfigurationExpression<TSource> : IMemberConfigurationExpression<TSource>
    {
        private readonly IMemberAccessor _destinationMember;
        private readonly Type _sourceType;
        private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(IMemberAccessor destinationMember, Type sourceType)
        {
            _destinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public void NullSubstitute(object nullSubstitute)
        {
            _propertyMapActions.Add(pm => pm.SetNullSubstitute(nullSubstitute));
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
        {
            var resolver = new DeferredInstantiatedResolver(typeof(TValueResolver).BuildCtor<IValueResolver>());

            ResolveUsing(resolver);

            var expression = new ResolutionExpression<TSource, TValueResolver>(_sourceType);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
        {
            var resolver = new DeferredInstantiatedResolver(valueResolverType.BuildCtor<IValueResolver>());

            ResolveUsing(resolver);

            var expression = new ResolutionExpression<TSource>(_sourceType);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(valueResolver));

            var expression = new ResolutionExpression<TSource>(_sourceType);

            _propertyMapActions.Add(pm => expression.Configure(pm));

            return expression;
        }

        public void ResolveUsing<TMember>(Func<TSource, TMember> resolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource, TMember>(rr => resolver((TSource)rr.Value))));
        }

        public void ResolveUsing<TMember>(Func<ResolutionResult, TMember> resolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource, TMember>(resolver)));
        }

        public void ResolveUsing<TMember>(Func<ResolutionResult, TSource, TMember> resolver)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource, TMember>(rr => resolver(rr, (TSource)rr.Value))));
        }

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            _propertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(sourceMember));
        }

        public void MapFrom<TMember>(string sourceMember)
        {
            var members = _sourceType.GetMember(sourceMember);
            if (!members.Any())
                throw new AutoMapperConfigurationException($"Unable to find source member {sourceMember} on type {_sourceType.FullName}");
            if (members.Skip(1).Any())
                throw new AutoMapperConfigurationException($"Source member {sourceMember} is ambiguous on type {_sourceType.FullName}");
            var member = members.Single();

            var par = Expression.Parameter(typeof(TSource));
            var prop = typeof(TSource) != _sourceType ? (Expression) Expression.Property(Expression.Convert(par, _sourceType), sourceMember) : Expression.Property(par, sourceMember);
            if (typeof (TMember) != member.GetMemberType())
                prop = Expression.Convert(prop, typeof(TMember));

            var lambda = Expression.Lambda<Func<TSource, TMember>>(prop, par);

            _propertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(lambda));
        }

        public void MapFrom(string sourceMember)
        {
            MapFrom<object>(sourceMember);
        }

        public void UseValue<TValue>(TValue value)
        {
            MapFrom(src => value);
        }

        public void UseValue(object value)
        {
            _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource, object>(rr => value)));
        }

        public void Condition(Func<TSource, bool> condition)
        {
            Condition(context => condition((TSource)context.Parent.SourceValue));
        }

        public void Condition(Func<ResolutionContext, bool> condition)
        {
            _propertyMapActions.Add(pm => pm.ApplyCondition(condition));
        }

        public void PreCondition(Func<TSource, bool> condition)
        {
            PreCondition(context => condition((TSource)context.Parent.SourceValue));
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