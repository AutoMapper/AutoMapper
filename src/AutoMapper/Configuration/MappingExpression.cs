using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    using Execution;
    public class MappingExpression : MappingExpressionBase<object, object, IMappingExpression>, IMappingExpression
    {
        public MappingExpression(TypePair types, MemberList memberList) : base(memberList, types)
        {
        }

        public string[] IncludedMembersNames { get; internal set; } = Array.Empty<string>();

        public IMappingExpression ReverseMap()
        {
            var reversedTypes = new TypePair(DestinationType, SourceType);
            var reverseMap = new MappingExpression(reversedTypes, MemberList.None)
            {
                IsReverseMap = true
            };
            ReverseMapCore(reverseMap);
            reverseMap.IncludeMembers(MapToSourceMembers().Select(m => m.DestinationMember.Name).ToArray());
            foreach (var includedMemberName in IncludedMembersNames)
            {
                reverseMap.ForMember(includedMemberName, m => m.MapFrom(s => s));
            }
            return reverseMap;
        }

        public IMappingExpression IncludeMembers(params string[] memberNames)
        {
            IncludedMembersNames = memberNames;
            foreach(var memberName in memberNames)
            {
                SourceType.GetFieldOrProperty(memberName);
            }
            TypeMapActions.Add(tm => tm.IncludedMembersNames = memberNames);
            return this;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions)
        {
            TypeMapActions.Add(typeMap =>
            {
                foreach (var accessor in typeMap.DestinationSetters)
                {
                    ForMember(accessor, memberOptions);
                }
            });
        }

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
        {
            var member = DestinationType.GetFieldOrProperty(name);
            ForMember(member, memberOptions);
            return this;
        }

        protected override void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true)
        {
            ForMember(property, _ => {}).Ignore(ignorePaths);
        }

        internal MemberConfigurationExpression ForMember(MemberInfo destinationProperty, Action<IMemberConfigurationExpression> memberOptions)
        {
            var expression = new MemberConfigurationExpression(destinationProperty, SourceType);

            MemberConfigurations.Add(expression);

            memberOptions(expression);

            return expression;
        }

        public class MemberConfigurationExpression : MemberConfigurationExpression<object, object, object>, IMemberConfigurationExpression
        {
            public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
                : base(destinationMember, sourceType)
            {
            }

            public void MapFrom(Type valueResolverType)
            {
                var config = new ValueResolverConfiguration(valueResolverType, valueResolverType.GetGenericInterface(typeof(IValueResolver<,,>)));

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }

            public void MapFrom(Type valueResolverType, string sourceMemberName)
            {
                var config = new ValueResolverConfiguration(valueResolverType, valueResolverType.GetGenericInterface(typeof(IMemberValueResolver<,,,>)))
                {
                    SourceMemberName = sourceMemberName
                };

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }

            public void MapFrom<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> resolver, string sourceMemberName)
            {
                var config = new ValueResolverConfiguration(resolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember>))
                {
                    SourceMemberName = sourceMemberName
                };

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }

            public void ConvertUsing(Type valueConverterType) 
                => PropertyMapActions.Add(pm => ConvertUsing(pm, valueConverterType));

            public void ConvertUsing(Type valueConverterType, string sourceMemberName) 
                => PropertyMapActions.Add(pm => ConvertUsing(pm, valueConverterType, sourceMemberName));

            public void ConvertUsing<TSourceMember, TDestinationMember>(IValueConverter<TSourceMember, TDestinationMember> valueConverter, string sourceMemberName)
            {
                PropertyMapActions.Add(pm =>
                {
                    var config = new ValueConverter(valueConverter, typeof(IValueConverter<TSourceMember, TDestinationMember>))
                    {
                        SourceMemberName = sourceMemberName
                    };

                    pm.Resolver = config;
                });
            }

            private static void ConvertUsing(PropertyMap propertyMap, Type valueConverterType, string sourceMemberName = null)
            {
                var config = new ValueConverter(valueConverterType, valueConverterType.GetGenericInterface(typeof(IValueConverter<,>)))
                {
                    SourceMemberName = sourceMemberName
                };

                propertyMap.Resolver = config;
            }
        }
    }

    public class MappingExpression<TSource, TDestination> :
        MappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>,
        IMappingExpression<TSource, TDestination>, IProjectionExpression<TSource, TDestination>
    {
        public MappingExpression(MemberList memberList, bool projection = false) : base(memberList)
        {
            Projection = projection;
        }
        public MappingExpression(MemberList memberList, Type sourceType, Type destinationType) : base(memberList, sourceType, destinationType) { }
        public IMappingExpression<TSource, TDestination> ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            if (!destinationMember.IsMemberPath(out var chain))
            {
                throw new ArgumentOutOfRangeException(nameof(destinationMember), "Only member accesses are allowed. " + destinationMember);
            }
            var expression = new PathConfigurationExpression<TSource, TDestination, TMember>(destinationMember, chain);
            var firstMember = expression.MemberPath.First;
            var firstMemberConfig = GetDestinationMemberConfiguration(firstMember);
            if(firstMemberConfig == null)
            {
                IgnoreDestinationMember(firstMember, ignorePaths: false);
            }
            MemberConfigurations.Add(expression);
            memberOptions(expression);
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            return ForDestinationMember(memberInfo, memberOptions);
        }

        private void IncludeMembersCore(LambdaExpression[] memberExpressions) => TypeMapActions.Add(tm => tm.IncludedMembers = memberExpressions);

        public IMappingExpression<TSource, TDestination> IncludeMembers(params Expression<Func<TSource, object>>[] memberExpressions)
        {
            var memberExpressionsWithoutCastToObject = Array.ConvertAll(
                memberExpressions,
                e =>
                {
                    var bodyIsCastToObject = e.Body.NodeType == ExpressionType.Convert && e.Body.Type == typeof(object);
                    return bodyIsCastToObject ? Expression.Lambda(((UnaryExpression)e.Body).Operand, e.Parameters) : e;
                });

            IncludeMembersCore(memberExpressionsWithoutCastToObject);
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember(string name, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            var member = DestinationType.GetFieldOrProperty(name);
            return ForDestinationMember(member, memberOptions);
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            TypeMapActions.Add(typeMap =>
            {
                foreach (var accessor in typeMap.DestinationSetters)
                {
                    ForDestinationMember(accessor, memberOptions);
                }
            });
        }

        public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination
        {
            IncludeCore(typeof(TOtherSource), typeof(TOtherDestination));
            
            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>() 
            => IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));

        public IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(sourceMember);

            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            SourceMemberConfigurations.Add(srcConfig);

            return this;
        }

        public void As<T>() where T : TDestination => As(typeof(T));

        public IMappingExpression<TSource, TDestination> AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer)
        {
            var config = new ValueTransformerConfiguration(typeof(TValue), transformer);

            ValueTransformers.Add(config);

            return this;
        }

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var reverseMap = new MappingExpression<TDestination, TSource>(MemberList.None, DestinationType, SourceType)
            {
                IsReverseMap = true
            };
            ReverseMapCore(reverseMap);
            reverseMap.IncludeMembersCore(MapToSourceMembers().Select(m => m.GetDestinationExpression()).ToArray());
            return reverseMap;
        }

        private IMappingExpression<TSource, TDestination> ForDestinationMember<TMember>(MemberInfo destinationProperty, Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var expression = new MemberConfigurationExpression<TSource, TDestination, TMember>(destinationProperty, SourceType);

            MemberConfigurations.Add(expression);

            memberOptions(expression);

            return this;
        }

        protected override void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true) 
            => ForDestinationMember<object>(property, options => options.Ignore(ignorePaths));

        IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination>.ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IProjectionMemberConfiguration<TSource, TDestination, TMember>> memberOptions) => 
            (IProjectionExpression<TSource, TDestination>)ForMember(destinationMember, memberOptions);
        IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.AddTransform<TValue>(
            Expression<Func<TValue, TValue>> transformer) => (IProjectionExpression<TSource, TDestination>)AddTransform(transformer);
        IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.IncludeMembers(
            params Expression<Func<TSource, object>>[] memberExpressions) => (IProjectionExpression<TSource, TDestination>)IncludeMembers(memberExpressions);
        IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.MaxDepth(int depth) =>
            (IProjectionExpression<TSource, TDestination>)MaxDepth(depth);
        IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ValidateMemberList(
            MemberList memberList) => (IProjectionExpression<TSource, TDestination>)ValidateMemberList(memberList);
        IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ConstructUsing(
            Expression<Func<TSource, TDestination>> ctor) => (IProjectionExpression<TSource, TDestination>)ConstructUsing(ctor);
        IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ForCtorParam(
            string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions) =>
            (IProjectionExpression<TSource, TDestination>)ForCtorParam(ctorParamName, paramOptions);
    }
}