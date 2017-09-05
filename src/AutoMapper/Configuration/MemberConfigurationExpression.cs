using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Configuration
{
    using static Expression;

    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
    {
        private readonly MemberInfo _destinationMember;
        private LambdaExpression _sourceMember;
        private readonly Type _sourceType;
        protected List<Action<PropertyMap>> PropertyMapActions { get; } = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
        {
            _destinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public MemberInfo DestinationMember => _destinationMember;

        public void MapAtRuntime()
        {
            PropertyMapActions.Add(pm => pm.Inline = false);
        }

        public void NullSubstitute(object nullSubstitute)
        {
            PropertyMapActions.Add(pm => pm.NullSubstitute = nullSubstitute);
        }

        public void ResolveUsing<TValueResolver>() 
            where TValueResolver : IValueResolver<TSource, TDestination, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IValueResolver<TSource, TDestination, TMember>));

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TValueResolver, TSourceMember>(string sourceMemberName)
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMemberName = sourceMemberName
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing(IValueResolver<TSource, TDestination, TMember> valueResolver)
        {
            var config = new ValueResolverConfiguration(valueResolver, typeof(IValueResolver<TSource, TDestination, TMember>));

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var config = new ValueResolverConfiguration(valueResolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void ResolveUsing<TResult>(Func<TSource, TResult> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => resolver(src);

                pm.CustomResolver = expr;
            });
        }

        public void ResolveUsing<TResult>(Func<TSource, TDestination, TResult> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => resolver(src, dest);

                pm.CustomResolver = expr;
            });
        }

        public void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, TResult> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => resolver(src, dest, destMember);

                pm.CustomResolver = expr;
            });
        }

        public void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> resolver)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => resolver(src, dest, destMember, ctxt);

                pm.CustomResolver = expr;
            });
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            MapFromUntyped(sourceMember);
        }

        internal void MapFromUntyped(LambdaExpression sourceExpression)
        {
            _sourceMember = sourceExpression;
            PropertyMapActions.Add(pm => pm.MapFrom(sourceExpression));
        }

        public void MapFrom(string sourceMember)
        {
            _sourceType.GetFieldOrProperty(sourceMember);
            PropertyMapActions.Add(pm => pm.CustomSourceMemberName = sourceMember);
        }

        public void UseValue<TValue>(TValue value)
        {
            MapFrom(s => value);
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

        public void PreCondition(Func<TSource, ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, ResolutionContext, bool>> expr =
                    (src, ctxt) => condition(src, ctxt);

                pm.PreCondition = expr;
            });
        }

        public void AddTransform(Expression<Func<TMember, TMember>> transformer)
        {
            PropertyMapActions.Add(pm =>
            {
                var config = new ValueTransformerConfiguration(typeof(TMember), transformer);

                pm.AddValueTransformation(config);
            });
        }

        public void ExplicitExpansion()
        {
            PropertyMapActions.Add(pm => pm.ExplicitExpansion = true);
        }

        public void Ignore() => Ignore(ignorePaths: true);

        internal void Ignore(bool ignorePaths) =>
            PropertyMapActions.Add(pm =>
            {
                pm.Ignored = true;
                if(ignorePaths)
                {
                    pm.TypeMap.IgnorePaths(DestinationMember);
                }
            });

        public void AllowNull()
        {
            PropertyMapActions.Add(pm => pm.AllowNull = true);
        }

        public void UseDestinationValue()
        {
            PropertyMapActions.Add(pm => pm.UseDestinationValue = true);
        }

        public void SetMappingOrder(int mappingOrder)
        {
            PropertyMapActions.Add(pm => pm.MappingOrder = mappingOrder);
        }

        public void Configure(TypeMap typeMap)
        {
            var destMember = _destinationMember;

            if(destMember.DeclaringType.IsGenericType())
            {
                destMember = typeMap.DestinationTypeDetails.PublicReadAccessors.Single(m => m.Name == destMember.Name);
            }

            var propertyMap = typeMap.FindOrCreatePropertyMapFor(destMember);

            Apply(propertyMap);
        }

        private void Apply(PropertyMap propertyMap)
        {
            foreach(var action in PropertyMapActions)
            {
                action(propertyMap);
            }
        }

        public IPropertyMapConfiguration Reverse()
        {
            var newSource = Parameter(DestinationMember.DeclaringType, "source");
            var newSourceProperty = MakeMemberAccess(newSource, _destinationMember);
            var newSourceExpression = Lambda(newSourceProperty, newSource);
            return PathConfigurationExpression<TDestination, TSource, object>.Create(_sourceMember, newSourceExpression);
        }
    }
}