using AutoMapper.Internal;
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
        private MemberInfo[] _sourceMembers;
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

        public void MapFrom<TResult>(Expression<Func<TSource, TDestination, TResult>> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                ParameterExpression p1 = Expression.Parameter(typeof(TSource));
                ParameterExpression p2 = Expression.Parameter(typeof(TDestination));
                ParameterExpression p3 = Expression.Parameter(typeof(TMember));
                ParameterExpression p4 = Expression.Parameter(typeof(ResolutionContext));

                InvocationExpression inv = Expression.Invoke(mappingFunction, p1, p2);
                LambdaExpression exp = Expression.Lambda(inv, p1, p2, p3, p4);
                
                pm.CustomMapFunction = exp;
            });
        }

        public void MapFrom<TResult>(Expression<Func<TSource, TDestination, TMember, TResult>> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                ParameterExpression p1 = Expression.Parameter(typeof(TSource));
                ParameterExpression p2 = Expression.Parameter(typeof(TDestination));
                ParameterExpression p3 = Expression.Parameter(typeof(TMember));
                ParameterExpression p4 = Expression.Parameter(typeof(ResolutionContext));

                InvocationExpression inv = Expression.Invoke(mappingFunction, p1, p2, p3);
                LambdaExpression exp = Expression.Lambda(inv, p1, p2, p3, p4);
                
                pm.CustomMapFunction = exp;
            });
        }

        public void MapFrom<TResult>(Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> mappingFunction)
        {
            PropertyMapActions.Add(pm =>
            {
                pm.CustomMapFunction = mappingFunction;
            });
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression)
        {
            MapFromUntyped(mapExpression);
        }

        internal void MapFromUntyped(LambdaExpression sourceExpression)
        {
            SourceExpression = sourceExpression;
            PropertyMapActions.Add(pm => pm.MapFrom(sourceExpression));
        }

        public void MapFrom(string sourceMembersPath)
        {
            _sourceMembers = ReflectionHelper.GetMemberPath(_sourceType, sourceMembersPath);
            PropertyMapActions.Add(pm => pm.MapFrom(sourceMembersPath));
        }

        public void Condition(Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                pm.Condition = condition;
            });
        }

        public void Condition(Expression<Func<TSource, TDestination, TMember, TMember, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var mSrc = Expression.Parameter(typeof(TMember));
                var mDst = Expression.Parameter(typeof(TMember));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src, dst, mSrc, mDst);
                var lmb = Expression.Lambda(inv, src, dst, mSrc, mDst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void Condition(Expression<Func<TSource, TDestination, TMember, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var mSrc = Expression.Parameter(typeof(TMember));
                var mDst = Expression.Parameter(typeof(TMember));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src, dst, mSrc);
                var lmb = Expression.Lambda(inv, src, dst, mSrc, mDst, ctx);

                pm.Condition = lmb;
            });
        }

        public void Condition(Expression<Func<TSource, TDestination, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var mSrc = Expression.Parameter(typeof(TMember));
                var mDst = Expression.Parameter(typeof(TMember));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src, dst);
                var lmb = Expression.Lambda(inv, src, dst, mSrc, mDst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void Condition(Expression<Func<TSource, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var mSrc = Expression.Parameter(typeof(TMember));
                var mDst = Expression.Parameter(typeof(TMember));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src);
                var lmb = Expression.Lambda(inv, src, dst, mSrc, mDst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void PreCondition(Expression<Func<TSource, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src);
                var lmb = Expression.Lambda(inv, src, dst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void PreCondition(Expression<Func<ResolutionContext, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, ctx);
                var lmb = Expression.Lambda(inv, src, dst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void PreCondition(Expression<Func<TSource, ResolutionContext, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src, ctx);
                var lmb = Expression.Lambda(inv, src, dst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void PreCondition(Expression<Func<TSource, TDestination, ResolutionContext, bool>> condition)
        {
            PropertyMapActions.Add(pm =>
            {
                var src = Expression.Parameter(typeof(TSource));
                var dst = Expression.Parameter(typeof(TDestination));
                var ctx = Expression.Parameter(typeof(ResolutionContext));

                var inv = Expression.Invoke(condition, src, dst, ctx);
                var lmb = Expression.Lambda(inv, src, dst, ctx);
                
                pm.Condition = lmb;
            });
        }

        public void AddTransform(Expression<Func<TMember, TMember>> transformer) =>
            PropertyMapActions.Add(pm => pm.AddValueTransformation(new ValueTransformerConfiguration(pm.DestinationType, transformer)));

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

        public void AllowNull() => SetAllowNull(true);

        public void DoNotAllowNull() => SetAllowNull(false);

        private void SetAllowNull(bool value) => PropertyMapActions.Add(pm => pm.AllowNull = value);

        public void UseDestinationValue() => SetUseDestinationValue(true);

        private void SetUseDestinationValue(bool value) => PropertyMapActions.Add(pm => pm.UseDestinationValue = value);

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

            if(destMember.DeclaringType.IsGenericTypeDefinition)
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

        public LambdaExpression SourceExpression { get; private set; }
        public LambdaExpression GetDestinationExpression() => DestinationMember.Lambda();

        public IPropertyMapConfiguration Reverse()
        {
            var destinationType = DestinationMember.DeclaringType;
            if (_sourceMembers != null)
            {
                if (_sourceMembers.Length > 1)
                {
                    return null;
                }
                var reversedMemberConfiguration = new MemberConfigurationExpression<TDestination, TSource, object>(_sourceMembers[0], destinationType);
                reversedMemberConfiguration.MapFrom(DestinationMember.Name);
                return reversedMemberConfiguration;
            }
            if (destinationType.IsGenericTypeDefinition) // .ForMember("InnerSource", o => o.MapFrom(s => s))
            {
                return null;
            }
            return PathConfigurationExpression<TDestination, TSource, object>.Create(SourceExpression, GetDestinationExpression());
        }

        public void DoNotUseDestinationValue() => SetUseDestinationValue(false);
    }
}