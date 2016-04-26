namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IMemberConfiguration
    {
        void Configure(TypeMap typeMap);
        IMemberAccessor DestinationMember { get; }
    }

    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>, IMemberConfiguration
    {
        private readonly IMemberAccessor _destinationMember;
        private readonly Type _sourceType;
        protected List<Action<PropertyMap>> PropertyMapActions { get; } = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(IMemberAccessor destinationMember, Type sourceType)
        {
            _destinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public IMemberAccessor DestinationMember => _destinationMember;

        public void NullSubstitute(TMember nullSubstitute)
        {
            PropertyMapActions.Add(pm => pm.NullSubstitute = nullSubstitute);
        }

        public void ResolveUsing<TValueResolver>() 
            where TValueResolver : IValueResolver<TSource, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver));

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
            where TValueResolver : IValueResolver<TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof (TValueResolver))
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TValueResolver, TSourceMember>(string sourceMemberName)
            where TValueResolver : IValueResolver<TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof (TValueResolver))
            {
                SourceMemberName = sourceMemberName
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing(IValueResolver<TSource, TMember> valueResolver)
        {
            var config = new ValueResolverConfiguration(valueResolver);

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TSourceMember>(IValueResolver<TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var config = new ValueResolverConfiguration(valueResolver)
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TSourceMember>(Func<TSource, TSourceMember> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, ResolutionContext, TSourceMember>> expr = (src, ctxt) => resolver(src);

                pm.CustomResolver = expr;
            });
        }

        public void ResolveUsing<TSourceMember>(Func<TSource, ResolutionContext, TSourceMember> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, ResolutionContext, TSourceMember>> expr = (src, ctxt) => resolver(src, ctxt);

                pm.CustomResolver = expr;
            });
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            PropertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(sourceMember));
        }

        public void MapFrom(string sourceMember)
        {
            var memberInfo = _sourceType.GetMember(sourceMember).FirstOrDefault();
            if (memberInfo == null)
                throw new AutoMapperConfigurationException($"Cannot find member {sourceMember} of type {_sourceType}");

            PropertyMapActions.Add(pm => pm.CustomSourceMember = memberInfo);
        }

        public void UseValue<TValue>(TValue value)
        {
            PropertyMapActions.Add(pm => pm.CustomValue = value);
        }

        public void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember, ctxt);

                pm.Condition = expr;
            });
        }

        public void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember);

                pm.Condition = expr;
            });
        }

        public void Condition(Func<TSource, TDestination, TMember, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember);

                pm.Condition = expr;
            });
        }

        public void Condition(Func<TSource, TDestination, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest);

                pm.Condition = expr;
            });
        }

        public void Condition(Func<TSource, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src);

                pm.Condition = expr;
            });
        }

        public void PreCondition(Func<TSource, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, ResolutionContext, bool>> expr =
                    (src, ctxt) => condition(src);

                pm.PreCondition = expr;
            });
        }

        public void PreCondition(Func<ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, ResolutionContext, bool>> expr =
                    (src, ctxt) => condition(ctxt);

                pm.PreCondition = expr;
            });
        }

        public void ExplicitExpansion()
        {
            PropertyMapActions.Add(pm => pm.ExplicitExpansion = true);
        }

        public void Ignore()
        {
            PropertyMapActions.Add(pm => pm.Ignored = true);
        }

        public void UseDestinationValue()
        {
            PropertyMapActions.Add(pm => pm.UseDestinationValue = true);
        }

        public void DoNotUseDestinationValue()
        {
            PropertyMapActions.Add(pm => pm.UseDestinationValue = false);
        }

        public void SetMappingOrder(int mappingOrder)
        {
            PropertyMapActions.Add(pm => pm.MappingOrder = mappingOrder);
        }

        public void Configure(TypeMap typeMap)
        {
            var propertyMap = typeMap.FindOrCreatePropertyMapFor(_destinationMember);

            foreach (var action in PropertyMapActions)
            {
                action(propertyMap);
            }
        }
    }
}