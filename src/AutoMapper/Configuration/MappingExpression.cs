using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Configuration.Internal;

namespace AutoMapper.Configuration
{
    using static Expression;

    public class MappingExpression : MappingExpression<object, object>, IMappingExpression
    {
        public MappingExpression(TypePair types, MemberList memberList) : base(memberList, types)
        {
        }

        public new IMappingExpression ReverseMap() => (IMappingExpression) base.ReverseMap();

        public IMappingExpression Substitute(Func<object, object> substituteFunc)
            => (IMappingExpression) base.Substitute(substituteFunc);

        public new IMappingExpression ConstructUsingServiceLocator() 
            => (IMappingExpression)base.ConstructUsingServiceLocator();

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions) 
            => base.ForAllMembers(opts => memberOptions((IMemberConfigurationExpression)opts));

        void IMappingExpression.ConvertUsing<TTypeConverter>() 
            => ConvertUsing(typeof(TTypeConverter));

        public void ConvertUsing(Type typeConverterType) 
            => TypeMapActions.Add(tm => tm.TypeConverterType = typeConverterType);

        public void ForAllOtherMembers(Action<IMemberConfigurationExpression> memberOptions) 
            => base.ForAllOtherMembers(o => memberOptions((IMemberConfigurationExpression)o));

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions) 
            => (IMappingExpression)base.ForMember(name, c => memberOptions((IMemberConfigurationExpression)c));

        public new IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions) 
            => (IMappingExpression)base.ForSourceMember(sourceMemberName, memberOptions);

        public new IMappingExpression Include(Type otherSourceType, Type otherDestinationType) 
            => (IMappingExpression)base.Include(otherSourceType, otherDestinationType);

        public new IMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter() 
            => (IMappingExpression)base.IgnoreAllPropertiesWithAnInaccessibleSetter();

        public new IMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter() 
            => (IMappingExpression)base.IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        public new IMappingExpression IncludeBase(Type sourceBase, Type destinationBase) 
            => (IMappingExpression)base.IncludeBase(sourceBase, destinationBase);

        public new IMappingExpression BeforeMap(Action<object, object> beforeFunction) 
            => (IMappingExpression)base.BeforeMap(beforeFunction);

        public new IMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<object, object> 
            => (IMappingExpression)base.BeforeMap<TMappingAction>();

        public new IMappingExpression AfterMap(Action<object, object> afterFunction) 
            => (IMappingExpression)base.AfterMap(afterFunction);

        public new IMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<object, object> 
            => (IMappingExpression)base.AfterMap<TMappingAction>();

        public new IMappingExpression ConstructUsing(Func<object, object> ctor)
            => (IMappingExpression)base.ConstructUsing(ctor);

        public new IMappingExpression ConstructUsing(Func<object, ResolutionContext, object> ctor) 
            => (IMappingExpression)base.ConstructUsing(ctor);

        public IMappingExpression ConstructProjectionUsing(LambdaExpression ctor)
        {
            TypeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return this;
        }

        public new IMappingExpression MaxDepth(int depth) 
            => (IMappingExpression)base.MaxDepth(depth);

        public new IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions) 
            => (IMappingExpression)base.ForCtorParam(ctorParamName, paramOptions);

        public new IMappingExpression PreserveReferences() => (IMappingExpression)base.PreserveReferences();

        protected override IPropertyMapConfiguration CreateMemberConfigurationExpression<TMember>(MemberInfo member, Type sourceType)
            => new MemberConfigurationExpression(member, sourceType);

        protected override MappingExpression<object, object> CreateReverseMapExpression() 
            => new MappingExpression(new TypePair(DestinationType, SourceType), MemberList.Source);

        internal class MemberConfigurationExpression : MemberConfigurationExpression<object, object, object>, IMemberConfigurationExpression
        {
            public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
                : base(destinationMember, sourceType)
            {
            }

            public void ResolveUsing(Type valueResolverType)
            {
                var config = new ValueResolverConfiguration(valueResolverType, valueResolverType.GetGenericInterface(typeof(IValueResolver<,,>)));

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }

            public void ResolveUsing(Type valueResolverType, string memberName)
            {
                var config = new ValueResolverConfiguration(valueResolverType, valueResolverType.GetGenericInterface(typeof(IMemberValueResolver<,,,>)))
                {
                    SourceMemberName = memberName
                };

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }

            public void ResolveUsing<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> resolver, string memberName)
            {
                var config = new ValueResolverConfiguration(resolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember>))
                {
                    SourceMemberName = memberName
                };

                PropertyMapActions.Add(pm => pm.ValueResolverConfig = config);
            }
        }

    }

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, ITypeMapConfiguration
    {
        private readonly List<IPropertyMapConfiguration> _memberConfigurations = new List<IPropertyMapConfiguration>();
        private readonly List<SourceMappingExpression> _sourceMemberConfigurations = new List<SourceMappingExpression>();
        private readonly List<CtorParamConfigurationExpression<TSource>> _ctorParamConfigurations = new List<CtorParamConfigurationExpression<TSource>>();
        private readonly List<ValueTransformerConfiguration> _valueTransformers = new List<ValueTransformerConfiguration>();
        private MappingExpression<TDestination, TSource> _reverseMap;
        private Action<IMemberConfigurationExpression<TSource, TDestination, object>> _allMemberOptions;
        private Func<MemberInfo, bool> _memberFilter;

        public MappingExpression(MemberList memberList)
            : this(memberList, typeof(TSource), typeof(TDestination))
        {
        }

        public MappingExpression(MemberList memberList, Type sourceType, Type destinationType)
            : this(memberList, new TypePair(sourceType, destinationType))
        {
        }

        public MappingExpression(MemberList memberList, TypePair types)
        {
            MemberList = memberList;
            Types = types;
            IsOpenGeneric = types.SourceType.IsGenericTypeDefinition() || types.DestinationType.IsGenericTypeDefinition();
        }

        public MemberList MemberList { get; }
        public TypePair Types { get; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public bool IsOpenGeneric { get; }
        public ITypeMapConfiguration ReverseTypeMap => _reverseMap;
        public IList<ValueTransformerConfiguration> ValueTransformers => _valueTransformers;
        protected List<Action<TypeMap>> TypeMapActions { get; } = new List<Action<TypeMap>>();

        public IMappingExpression<TSource, TDestination> PreserveReferences()
        {
            TypeMapActions.Add(tm => tm.PreserveReferences = true);

            return this;
        }

        protected virtual IPropertyMapConfiguration CreateMemberConfigurationExpression<TMember>(MemberInfo member, Type sourceType) 
            => new MemberConfigurationExpression<TSource, TDestination, TMember>(member, sourceType);

        protected virtual MappingExpression<TDestination, TSource> CreateReverseMapExpression() 
            => new MappingExpression<TDestination, TSource>(MemberList.None, DestinationType, SourceType);

        public IMappingExpression<TSource, TDestination> ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            if(!destinationMember.IsMemberPath())
            {
                throw new ArgumentOutOfRangeException(nameof(destinationMember), "Only member accesses are allowed.");
            }
            var expression = new PathConfigurationExpression<TSource, TDestination, TMember>(destinationMember);
            var firstMember = expression.MemberPath.First;
            var firstMemberConfig = GetDestinationMemberConfiguration(firstMember);
            if(firstMemberConfig == null)
            {
                IgnoreDestinationMember(firstMember, ignorePaths: false);
            }
            _memberConfigurations.Add(expression);
            memberOptions(expression);
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
                                                                   Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            return ForDestinationMember(memberInfo, memberOptions);
        }

        public IMappingExpression<TSource, TDestination> ForMember(string name,
                                                                   Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            var member = DestinationType.GetFieldOrProperty(name);
            return ForDestinationMember(member, memberOptions);
        }

        public void ForAllOtherMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            _allMemberOptions = memberOptions;
            _memberFilter = m => GetDestinationMemberConfiguration(m) == null;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            _allMemberOptions = memberOptions;
            _memberFilter = _ => true;
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            foreach(var property in DestinationType.PropertiesWithAnInaccessibleSetter())
            {
                IgnoreDestinationMember(property);
            }
            return this;
        }

        private void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true) =>
            ForDestinationMember<object>(property, options => options.Ignore(ignorePaths));

        public IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            foreach(var property in SourceType.PropertiesWithAnInaccessibleSetter())
            {
                ForSourceMember(property.Name, options => options.Ignore());
            }
            return this;
        }

        public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination 
            => IncludeCore(typeof(TOtherSource), typeof(TOtherDestination));

        public IMappingExpression<TSource, TDestination> Include(Type otherSourceType, Type otherDestinationType)
        {
            CheckIsDerived(otherSourceType, SourceType);
            CheckIsDerived(otherDestinationType, DestinationType);
            return IncludeCore(otherSourceType, otherDestinationType);
        }

        IMappingExpression<TSource, TDestination> IncludeCore(Type otherSourceType, Type otherDestinationType)
        {
            TypeMapActions.Add(tm => tm.IncludeDerivedTypes(otherSourceType, otherDestinationType));
            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>() 
            => IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));

        public IMappingExpression<TSource, TDestination> IncludeBase(Type sourceBase, Type destinationBase)
        {
            CheckIsDerived(SourceType, sourceBase);
            CheckIsDerived(DestinationType, destinationBase);
            TypeMapActions.Add(tm => tm.IncludeBaseTypes(sourceBase, destinationBase));
            return this;
        }

        public void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression)
        {
            TypeMapActions.Add(tm => tm.CustomProjection = projectionExpression);
        }

        public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
        {
            TypeMapActions.Add(tm => tm.MaxDepth = depth);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructUsingServiceLocator()
        {
            TypeMapActions.Add(tm => tm.ConstructDestinationUsingServiceLocator = true);

            return this;
        }

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            _reverseMap = CreateReverseMapExpression();
            _reverseMap._memberConfigurations.AddRange(_memberConfigurations.Select(m => m.Reverse()).Where(m=>m!=null));
            return _reverseMap;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(sourceMember);

            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            _sourceMemberConfigurations.Add(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = SourceType.GetFieldOrProperty(sourceMemberName);

            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            _sourceMemberConfigurations.Add(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> Substitute<TSubstitute>(Func<TSource, TSubstitute> substituteFunc)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TSubstitute>> expr = (src, dest, ctxt) => substituteFunc(src);

                tm.Substitution = expr;
            });

            return this;
        }

        public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr =
                    (src, dest, ctxt) => mappingFunction(src);

                tm.CustomMapper = expr;
            });
        }

        public void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr =
                    (src, dest, ctxt) => mappingFunction(src, dest);

                tm.CustomMapper = expr;
            });
        }

        public void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr =
                    (src, dest, ctxt) => mappingFunction(src, dest, ctxt);

                tm.CustomMapper = expr;
            });
        }

        public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
        {
            ConvertUsing(converter.Convert);
        }

        public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>
        {
            TypeMapActions.Add(tm => tm.TypeConverterType = typeof (TTypeConverter));
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest);

                tm.AddBeforeMapAction(expr);
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest, ctxt);

                tm.AddBeforeMapAction(expr);
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void BeforeFunction(TSource src, TDestination dest, ResolutionContext ctxt) 
                => ((TMappingAction) ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return BeforeMap(BeforeFunction);
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest);

                tm.AddAfterMapAction(expr);
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest, ctxt);

                tm.AddAfterMapAction(expr);
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void AfterFunction(TSource src, TDestination dest, ResolutionContext ctxt) 
                => ((TMappingAction) ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return AfterMap(AfterFunction);
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, ResolutionContext, TDestination>> expr = (src, ctxt) => ctor(src);

                tm.DestinationCtor = expr;
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, ResolutionContext, TDestination>> expr = (src, ctxt) => ctor(src, ctxt);

                tm.DestinationCtor = expr;
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor)
        {
            TypeMapActions.Add(tm =>
            {
                tm.ConstructExpression = ctor;

                var ctxtParam = Parameter(typeof (ResolutionContext), "ctxt");
                var srcParam = Parameter(typeof (TSource), "src");

                var body = ctor.ReplaceParameters(srcParam);

                tm.DestinationCtor = Lambda(body, srcParam, ctxtParam);
            });

            return this;
        }

        private IMappingExpression<TSource, TDestination> ForDestinationMember<TMember>(MemberInfo destinationProperty, Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var expression = (MemberConfigurationExpression<TSource, TDestination, TMember>) CreateMemberConfigurationExpression<TMember>(destinationProperty, SourceType);

            _memberConfigurations.Add(expression);

            memberOptions(expression);

            return this;
        }

        public void As<T>() where T : TDestination => As(typeof(T));

        public void As(Type typeOverride)
        {
            CheckIsDerived(typeOverride, DestinationType);
            TypeMapActions.Add(tm => tm.DestinationTypeOverride = typeOverride);
        }

        private void CheckIsDerived(Type derivedType, Type baseType)
        {
            if(!baseType.IsAssignableFrom(derivedType) && !derivedType.IsGenericTypeDefinition() && !baseType.IsGenericTypeDefinition())
            {
                throw new ArgumentOutOfRangeException(nameof(derivedType), $"{derivedType} is not derived from {baseType}.");
            }
        }

        public IMappingExpression<TSource, TDestination> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
        {
            var ctorParamExpression = new CtorParamConfigurationExpression<TSource>(ctorParamName);

            paramOptions(ctorParamExpression);

            _ctorParamConfigurations.Add(ctorParamExpression);

            return this;
        }

        public IMappingExpression<TSource, TDestination> DisableCtorValidation()
        {
            TypeMapActions.Add(tm =>
            {
                tm.DisableConstructorValidation = true;
            });

            return this;
        }

        public IMappingExpression<TSource, TDestination> AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer)
        {
            var config = new ValueTransformerConfiguration(typeof(TValue), transformer);

            _valueTransformers.Add(config);

            return this;
        }

        private IPropertyMapConfiguration GetDestinationMemberConfiguration(MemberInfo destinationMember) =>
            _memberConfigurations.FirstOrDefault(m => m.DestinationMember == destinationMember);

        public void Configure(TypeMap typeMap)
        {
            foreach (var destProperty in typeMap.DestinationTypeDetails.PublicWriteAccessors)
            {
                var attrs = destProperty.GetCustomAttributes(true);
                if (attrs.Any(x => x is IgnoreMapAttribute))
                {
                    IgnoreDestinationMember(destProperty);
                    var sourceProperty = typeMap.SourceType.GetInheritedMember(destProperty.Name);
                    if (sourceProperty != null)
                    {
                        _reverseMap?.IgnoreDestinationMember(sourceProperty);
                    }
                }
                if (typeMap.Profile.GlobalIgnores.Contains(destProperty.Name) && GetDestinationMemberConfiguration(destProperty) == null)
                {
                    IgnoreDestinationMember(destProperty);
                }
            }

            if (_allMemberOptions != null)
            {
                foreach (var accessor in typeMap.DestinationTypeDetails.PublicReadAccessors.Where(_memberFilter))
                {
                    ForDestinationMember(accessor, _allMemberOptions);
                }
            }

            foreach (var action in TypeMapActions)
            {
                action(typeMap);
            }
            foreach (var memberConfig in _memberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
            foreach (var memberConfig in _sourceMemberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
            foreach (var paramConfig in _ctorParamConfigurations)
            {
                paramConfig.Configure(typeMap);
            }
            foreach (var valueTransformer in _valueTransformers)
            {
                typeMap.AddValueTransformation(valueTransformer);
            }

            if (_reverseMap != null)
            {
                ReverseSourceMembers(typeMap);
                foreach(var destProperty in typeMap.GetPropertyMaps().Where(pm => pm.Ignored))
                {
                    _reverseMap.ForSourceMember(destProperty.DestinationProperty.Name, opt => opt.Ignore());
                }
                foreach(var includedDerivedType in typeMap.IncludedDerivedTypes)
                {
                    _reverseMap.Include(includedDerivedType.DestinationType, includedDerivedType.SourceType);
                }
                foreach(var includedBaseType in typeMap.IncludedBaseTypes)
                {
                    _reverseMap.IncludeBase(includedBaseType.DestinationType, includedBaseType.SourceType);
                }
            }
        }

        private void ReverseSourceMembers(TypeMap typeMap)
        {
            foreach(var propertyMap in typeMap.GetPropertyMaps().Where(p => p.SourceMembers.Count > 1 && !p.SourceMembers.Any(s=>s is MethodInfo)))
            {
                _reverseMap.TypeMapActions.Add(reverseTypeMap =>
                {
                    var memberPath = new MemberPath(propertyMap.SourceMembers);
                    var newDestination = Parameter(reverseTypeMap.DestinationType, "destination");
                    var path = propertyMap.SourceMembers.MemberAccesses(newDestination);
                    var forPathLambda = Lambda(path, newDestination);

                    var pathMap = reverseTypeMap.FindOrCreatePathMapFor(forPathLambda, memberPath, reverseTypeMap);

                    var newSource = Parameter(reverseTypeMap.SourceType, "source");
                    pathMap.SourceExpression = Lambda(MakeMemberAccess(newSource, propertyMap.DestinationProperty), newSource);
                });
            }
        }
    }
}

