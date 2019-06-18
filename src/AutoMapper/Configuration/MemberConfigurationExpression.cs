using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Configuration
{
    using static AutoMapper.Internal.ExpressionFactory;

    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
    {
        private LambdaExpression _sourceMember;
        private readonly Type _sourceType;
        protected List<Action<PropertyMap>> PropertyMapActions { get; } = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
        {
            DestinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public MemberInfo DestinationMember { get; }

        public void MapAtRuntime()
        {
            PropertyMapActions.Add(pm => pm.Inline = false);
        }

        public void NullSubstitute(object nullSubstitute)
        {
            PropertyMapActions.Add(pm => pm.NullSubstitute = nullSubstitute);
        }

        public void MapFrom<TValueResolver>() 
            where TValueResolver : IValueResolver<TSource, TDestination, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IValueResolver<TSource, TDestination, TMember>));

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void MapFrom<TValueResolver, TSourceMember>(string sourceMemberName)
            where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>
        {
            var config = new ValueResolverConfiguration(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMemberName = sourceMemberName
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver)
        {
            var config = new ValueResolverConfiguration(valueResolver, typeof(IValueResolver<TSource, TDestination, TMember>));

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void MapFrom<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var config = new ValueResolverConfiguration(valueResolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
            {
                SourceMember = sourceMember
            };

            PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
        }

        public void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => mappingFunction(src, dest);

                pm.CustomMapFunction = expr;
            });
        }

        public void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember);

                pm.CustomMapFunction = expr;
            });
        }

        public void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr = (src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember, ctxt);

                pm.CustomMapFunction = expr;
            });
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression)
        {
            MapFromUntyped(mapExpression);
        }

        internal void MapFromUntyped(LambdaExpression sourceExpression)
        {
            _sourceMember = sourceExpression;
            PropertyMapActions.Add(pm => pm.MapFrom(sourceExpression));
        }

        public void MapFrom(string sourceMemberName)
        {
            _sourceType.GetFieldOrProperty(sourceMemberName);
            PropertyMapActions.Add(pm => pm.MapFrom(sourceMemberName));
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
                Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr =
                    (src, dest, ctxt) => condition(src);

                pm.PreCondition = expr;
            });
        }

        public void PreCondition(Func<ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr =
                    (src, dest, ctxt) => condition(ctxt);

                pm.PreCondition = expr;
            });
        }

        public void PreCondition(Func<TSource, ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr =
                    (src, dest, ctxt) => condition(src, ctxt);

                pm.PreCondition = expr;
            });
        }

        public void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr =
                    (src, dest, ctxt) => condition(src, dest, ctxt);

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

        public void Ignore(bool ignorePaths) =>
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

        public void ConvertUsing<TValueConverter, TSourceMember>()
            where TValueConverter : IValueConverter<TSourceMember, TMember>
            => PropertyMapActions.Add(pm => ConvertUsing<TValueConverter, TSourceMember>(pm));

        public void ConvertUsing<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
            where TValueConverter : IValueConverter<TSourceMember, TMember>
            => PropertyMapActions.Add(pm => ConvertUsing<TValueConverter, TSourceMember>(pm, sourceMember));

        public void ConvertUsing<TValueConverter, TSourceMember>(string sourceMemberName)
            where TValueConverter : IValueConverter<TSourceMember, TMember>
            => PropertyMapActions.Add(pm => ConvertUsing<TValueConverter, TSourceMember>(pm, sourceMemberName: sourceMemberName));

        public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter)
            => PropertyMapActions.Add(pm => ConvertUsing(pm, valueConverter));

        public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, Expression<Func<TSource, TSourceMember>> sourceMember)
            => PropertyMapActions.Add(pm => ConvertUsing(pm, valueConverter, sourceMember));

        public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, string sourceMemberName) 
            => PropertyMapActions.Add(pm => ConvertUsing(pm, valueConverter, sourceMemberName: sourceMemberName));

        private static void ConvertUsing<TValueConverter, TSourceMember>(PropertyMap propertyMap,
            Expression<Func<TSource, TSourceMember>> sourceMember = null,
            string sourceMemberName = null)
        {
            var config = new ValueConverterConfiguration(typeof(TValueConverter),
                typeof(IValueConverter<TSourceMember, TMember>))
            {
                SourceMember = sourceMember,
                SourceMemberName = sourceMemberName
            };

            propertyMap.ValueConverterConfig = config;
        }

        private static void ConvertUsing<TSourceMember>(PropertyMap propertyMap, IValueConverter<TSourceMember, TMember> valueConverter,
            Expression<Func<TSource, TSourceMember>> sourceMember = null, string sourceMemberName = null)
        {
            var config = new ValueConverterConfiguration(valueConverter,
                typeof(IValueConverter<TSourceMember, TMember>))
            {
                SourceMember = sourceMember,
                SourceMemberName = sourceMemberName
            };

            propertyMap.ValueConverterConfig = config;
        }

        public void Configure(TypeMap typeMap)
        {
            var destMember = DestinationMember;

            if(destMember.DeclaringType.IsGenericTypeDefinition())
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
            propertyMap.CheckMappedReadonly();
        }

        public LambdaExpression SourceExpression => _sourceMember;
        public LambdaExpression GetDestinationExpression() => MemberAccessLambda(DestinationMember);

        public IPropertyMapConfiguration Reverse() =>
            PathConfigurationExpression<TDestination, TSource, object>.Create(_sourceMember, GetDestinationExpression());
    }
}