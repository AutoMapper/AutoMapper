namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public interface ITypeMapConfiguration
    {
        void Configure(IProfileConfiguration profile, TypeMap typeMap);
        MemberList MemberList { get; }
        Type SourceType { get; }
        Type DestinationType { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
        IEnumerable<TypePair> IncludedDerivedTypes { get; } 
        IEnumerable<TypePair> IncludedBaseTypes { get; } 
    }

    public class MappingExpression : IMappingExpression, ITypeMapConfiguration
    {
        private readonly MappingExpression<object, object> _innerExpression;
        private readonly List<Action<TypeMap>> _typeMapActions = new List<Action<TypeMap>>();
        private readonly List<MemberConfigurationExpression> _memberConfigurations = new List<MemberConfigurationExpression>();
        private MappingExpression _reverseMap;
        private Action<IMemberConfigurationExpression> _allMemberOptions;

        public MappingExpression(TypePair types, MemberList memberList)
        {
            MemberList = memberList;
            Types = types;
            _innerExpression = new MappingExpression<object, object>(memberList);
        }

        public MemberList MemberList { get; }
        public TypePair Types { get; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public ITypeMapConfiguration ReverseTypeMap => _reverseMap;
        public IEnumerable<TypePair> IncludedDerivedTypes => _innerExpression.IncludedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _innerExpression.IncludedBaseTypes;

        private class MemberConfigurationExpression : IMemberConfigurationExpression
        {
            private readonly Type _sourceType;
            private readonly IMemberAccessor _destinationMember;
            private readonly MappingExpression<object, object>.MemberConfigurationExpression _innerExpression;
            private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

            public MemberConfigurationExpression(Type sourceType, IMemberAccessor destinationMember)
            {
                _sourceType = sourceType;
                _destinationMember = destinationMember;
                _innerExpression = new MappingExpression<object, object>.MemberConfigurationExpression(destinationMember);
            }

            public void MapFrom(string sourceMember)
            {
                var members = _sourceType.GetMember(sourceMember);
                if (!members.Any())
                    throw new AutoMapperConfigurationException(
    $"Unable to find source member {sourceMember} on type {_sourceType.FullName}");
                if (members.Skip(1).Any())
                    throw new AutoMapperConfigurationException(
    $"Source member {sourceMember} is ambiguous on type {_sourceType.FullName}");
                var member = members.Single();

                _propertyMapActions.Add(pm =>
                {
                    pm.SourceMember = member;
                    pm.AssignCustomValueResolver(member.ToMemberGetter());
                });
            }

            public void Configure(TypeMap typeMap)
            {
                var propertyMap = typeMap.FindOrCreatePropertyMapFor(_destinationMember);

                foreach (var action in _propertyMapActions)
                {
                    action(propertyMap);
                }

                _innerExpression.Configure(typeMap);
            }

            public void NullSubstitute(object nullSubstitute)
            {
                _innerExpression.NullSubstitute(nullSubstitute);
            }

            public IResolverConfigurationExpression<object, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
            {
                return _innerExpression.ResolveUsing<TValueResolver>();
            }

            public IResolverConfigurationExpression<object> ResolveUsing(Type valueResolverType)
            {
                return _innerExpression.ResolveUsing(valueResolverType);
            }

            public IResolutionExpression<object> ResolveUsing(IValueResolver valueResolver)
            {
                return _innerExpression.ResolveUsing(valueResolver);
            }

            public void ResolveUsing(Func<object, object> resolver)
            {
                _innerExpression.ResolveUsing(resolver);
            }

            public void ResolveUsing(Func<ResolutionResult, object> resolver)
            {
                _innerExpression.ResolveUsing(resolver);
            }

            public void ResolveUsing(Func<ResolutionResult, object, object> resolver)
            {
                _innerExpression.ResolveUsing(resolver);
            }

            public void MapFrom<TMember>(Expression<Func<object, TMember>> sourceMember)
            {
                _innerExpression.MapFrom(sourceMember);
            }

            public void MapFrom<TMember>(string property)
            {
                _innerExpression.MapFrom<TMember>(property);
            }

            public void Ignore()
            {
                _innerExpression.Ignore();
            }

            public void SetMappingOrder(int mappingOrder)
            {
                _innerExpression.SetMappingOrder(mappingOrder);
            }

            public void UseDestinationValue()
            {
                _innerExpression.UseDestinationValue();
            }

            public void DoNotUseDestinationValue()
            {
                _innerExpression.DoNotUseDestinationValue();
            }

            public void UseValue<TValue>(TValue value)
            {
                _innerExpression.UseValue(value);
            }

            public void UseValue(object value)
            {
                _innerExpression.UseValue(value);
            }

            public void Condition(Func<object, bool> condition)
            {
                _innerExpression.Condition(condition);
            }

            public void Condition(Func<ResolutionContext, bool> condition)
            {
                _innerExpression.Condition(condition);
            }

            public void PreCondition(Func<object, bool> condition)
            {
                _innerExpression.PreCondition(condition);
            }

            public void PreCondition(Func<ResolutionContext, bool> condition)
            {
                _innerExpression.PreCondition(condition);
            }

            public void ExplicitExpansion()
            {
                _innerExpression.ExplicitExpansion();
            }
        }

        public IMappingExpression ReverseMap()
        {
            _reverseMap = new MappingExpression(new TypePair(Types.DestinationType, Types.SourceType), MemberList.Source);

            return _reverseMap;
        }

        public IMappingExpression Substitute(Func<object, object> substituteFunc)
        {
            _innerExpression.Substitute(substituteFunc);

            return this;
        }

        public IMappingExpression ConstructUsingServiceLocator()
        {
            _innerExpression.ConstructUsingServiceLocator();

            return this;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions)
        {
            _allMemberOptions = memberOptions;
        }

        public void ConvertUsing<TTypeConverter>()
        {
            ConvertUsing(typeof(TTypeConverter));
        }

        public void ConvertUsing(Type typeConverterType)
        {
            var interfaceType = typeof(ITypeConverter<,>).MakeGenericType(Types.SourceType, Types.DestinationType);
            var convertMethodType = interfaceType.IsAssignableFrom(typeConverterType) ? interfaceType : typeConverterType;
            var converter = new DeferredInstantiatedConverter(convertMethodType, BuildCtor<object>(typeConverterType));

            _innerExpression.ConvertUsing(converter);
        }

        public void As(Type typeOverride)
        {
            _typeMapActions.Add(tm => tm.DestinationTypeOverride = typeOverride);
        }

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
        {
            IMemberAccessor destMember = null;
            var propertyInfo = Types.DestinationType.GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = Types.DestinationType.GetField(name);
                if (fieldInfo == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "Cannot find a field or property named " + name);
                }
                destMember = new FieldAccessor(fieldInfo);
            }
            ForDestinationMember(destMember, memberOptions);

            return this;
        }

        public IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            _innerExpression.ForSourceMember(sourceMemberName, memberOptions);

            return this;
        }

        public IMappingExpression Include(Type otherSourceType, Type otherDestinationType)
        {
            _innerExpression.Include(otherSourceType, otherDestinationType);

            return this;
        }

        public IMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            _innerExpression.IgnoreAllPropertiesWithAnInaccessibleSetter();

            return this;
        }

        public IMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            _innerExpression.IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

            return this;
        }

        public IMappingExpression IncludeBase(Type sourceBase, Type destinationBase)
        {
            _innerExpression.IncludeBase(sourceBase, destinationBase);

            return this;
        }

        public void ProjectUsing(Expression<Func<object, object>> projectionExpression)
        {
            _innerExpression.ProjectUsing(projectionExpression);
        }

        public IMappingExpression BeforeMap(Action<object, object> beforeFunction)
        {
            _innerExpression.BeforeMap(beforeFunction);

            return this;
        }

        public IMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            _innerExpression.BeforeMap<TMappingAction>();

            return this;
        }

        public IMappingExpression AfterMap(Action<object, object> afterFunction)
        {
            _innerExpression.AfterMap(afterFunction);

            return this;
        }

        public IMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            _innerExpression.AfterMap<TMappingAction>();

            return this;
        }

        public IMappingExpression ConstructUsing(Func<object, object> ctor)
        {
            _innerExpression.ConstructUsing(ctor);

            return this;
        }

        public IMappingExpression ConstructUsing(Func<ResolutionContext, object> ctor)
        {
            _innerExpression.ConstructUsing(ctor);

            return this;
        }

        public IMappingExpression ConstructProjectionUsing(LambdaExpression ctor)
        {
            var func = ctor.Compile();

            _typeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return ConstructUsing(ctxt => func.DynamicInvoke(ctxt.SourceValue));
        }

        public IMappingExpression MaxDepth(int depth)
        {
            _innerExpression.MaxDepth(depth);

            return this;
        }

        public IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions)
        {
            _innerExpression.ForCtorParam(ctorParamName, paramOptions);

            return this;
        }

        public void Configure(IProfileConfiguration profile, TypeMap typeMap)
        {
            foreach (var destProperty in typeMap.DestinationTypeDetails.PublicWriteAccessors)
            {
                var attrs = destProperty.GetCustomAttributes(true);
                if (attrs.Any(x => x is IgnoreMapAttribute))
                {
                    ForMember(destProperty.Name, y => y.Ignore());
                }
                if (profile.GlobalIgnores.Contains(destProperty.Name))
                {
                    ForMember(destProperty.Name, y => y.Ignore());
                }
            }

            if (_allMemberOptions != null)
            {
                foreach (var accessor in typeMap.DestinationTypeDetails.PublicReadAccessors)
                {
                    ForDestinationMember(accessor.ToMemberAccessor(), _allMemberOptions);
                }
            }

            foreach (var action in _typeMapActions)
            {
                action(typeMap);
            }
            foreach (var memberConfig in _memberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }

            _innerExpression.Configure(profile, typeMap);

            if (_reverseMap != null)
            {
                foreach (var destProperty in typeMap.GetPropertyMaps().Where(pm => pm.IsIgnored()))
                {
                    _reverseMap.ForSourceMember(destProperty.DestinationProperty.Name, opt => opt.Ignore());
                }
                foreach (var includedDerivedType in typeMap.IncludedDerivedTypes)
                {
                    _reverseMap.Include(includedDerivedType.DestinationType, includedDerivedType.SourceType);
                }
            }
        }

        private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression> memberOptions)
        {
            var expression = new MemberConfigurationExpression(Types.SourceType, destinationProperty);

            _memberConfigurations.Add(expression);

            memberOptions(expression);
        }

        protected static Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(Type type)
        {
            return context =>
            {
                if (type.IsGenericTypeDefinition())
                {
                    type = type.MakeGenericType(context.SourceType.GetTypeInfo().GenericTypeArguments);
                }

                var obj = context.Options.ServiceCtor.Invoke(type);

                return (TServiceType)obj;
            };
        }

    }

    public class SourceMappingExpression : ISourceMemberConfigurationExpression
    {
        private readonly MemberInfo _sourceMember;
        private readonly List<Action<SourceMemberConfig>> _sourceMemberActions = new List<Action<SourceMemberConfig>>();

        public SourceMappingExpression(MemberInfo sourceMember)
        {
            _sourceMember = sourceMember;
        }

        public void Ignore()
        {
            _sourceMemberActions.Add(smc => smc.Ignore());
        }

        public void Configure(TypeMap typeMap)
        {
            var sourcePropertyConfig = typeMap.FindOrCreateSourceMemberConfigFor(_sourceMember);

            foreach (var action in _sourceMemberActions)
            {
                action(sourcePropertyConfig);
            }
        }
    }

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, ITypeMapConfiguration
    {
        private readonly List<Action<TypeMap>> _typeMapActions = new List<Action<TypeMap>>();
        private readonly List<MemberConfigurationExpression> _memberConfigurations = new List<MemberConfigurationExpression>();
        private readonly List<SourceMappingExpression> _sourceMemberConfigurations = new List<SourceMappingExpression>();
        private readonly List<CtorParamConfigurationExpression<TSource>> _ctorParamConfigurations = new List<CtorParamConfigurationExpression<TSource>>();
        private readonly List<TypePair> _includedDerivedTypes = new List<TypePair>();
        private readonly List<TypePair> _includedBaseTypes = new List<TypePair>();
        private MappingExpression<TDestination, TSource> _reverseMap;
        private Action<IMemberConfigurationExpression<TSource>> _allMemberOptions;

        public MappingExpression(MemberList memberList)
        {
            MemberList = memberList;
            Types = new TypePair(typeof(TSource), typeof(TDestination));
        }

        public MemberList MemberList { get; }
        public TypePair Types { get; }
        public Type SourceType => typeof(TSource);
        public Type DestinationType => typeof(TDestination);
        public ITypeMapConfiguration ReverseTypeMap => _reverseMap;
        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;

        public class MemberConfigurationExpression : IMemberConfigurationExpression<TSource>
        {
            private readonly IMemberAccessor _destinationMember;
            private readonly List<Action<PropertyMap>> _propertyMapActions = new List<Action<PropertyMap>>();

            public MemberConfigurationExpression(IMemberAccessor destinationMember)
            {
                _destinationMember = destinationMember;
            }

            public void NullSubstitute(object nullSubstitute)
            {
                _propertyMapActions.Add(pm => pm.SetNullSubstitute(nullSubstitute));
            }

            public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
            {
                var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(typeof(TValueResolver)));

                ResolveUsing(resolver);

                var resolutionExpression = new ResolutionExpression<TSource, TValueResolver>();

                return resolutionExpression;
            }

            public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
            {
                var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(valueResolverType));

                ResolveUsing(resolver);

                var expression = new ResolutionExpression<TSource>();

                _propertyMapActions.Add(pm => expression.Configure(pm));

                return expression;
            }

            public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
            {
                _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(valueResolver));

                var expression = new ResolutionExpression<TSource>();

                _propertyMapActions.Add(pm => expression.Configure(pm));

                return expression;
            }

            public void ResolveUsing(Func<TSource, object> resolver)
            {
                _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(r => resolver((TSource)r.Value))));
            }

            public void ResolveUsing(Func<ResolutionResult, object> resolver)
            {
                _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(resolver)));
            }

            public void ResolveUsing(Func<ResolutionResult, TSource, object> resolver)
            {
                _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(r => resolver(r, (TSource)r.Value))));
            }

            public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
            {
                _propertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(sourceMember));
            }

            public void MapFrom<TMember>(string property)
            {
                var par = Expression.Parameter(typeof(TSource));
                var prop = Expression.Property(par, property);
                var lambda = Expression.Lambda<Func<TSource, TMember>>(prop, par);
                _propertyMapActions.Add(pm => pm.SetCustomValueResolverExpression(lambda));
            }

            public void UseValue<TValue>(TValue value)
            {
                MapFrom(src => value);
            }

            public void UseValue(object value)
            {
                _propertyMapActions.Add(pm => pm.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(src => value)));
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

        public IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember,
                                                                   Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            IMemberAccessor destProperty = memberInfo.ToMemberAccessor();

            ForDestinationMember(destProperty, memberOptions);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember(string name,
                                                                   Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            IMemberAccessor destMember = null;
            var propertyInfo = typeof(TDestination).GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = typeof(TDestination).GetField(name);
                if(fieldInfo == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "Cannot find a field or property named " + name);
                }
                destMember = new FieldAccessor(fieldInfo);
            }
            ForDestinationMember(destMember, memberOptions);
            return this;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            _allMemberOptions = memberOptions;
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TDestination).GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
            foreach (var property in properties)
                ForMember(property.Name, opt => opt.Ignore());
            return this;
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TSource).GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
            foreach (var property in properties)
                ForSourceMember(property.Name, opt => opt.Ignore());
            return this;
        }

        public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination
        {
            return Include(typeof(TOtherSource), typeof(TOtherDestination));
        }

        public IMappingExpression<TSource, TDestination> Include(Type otherSourceType, Type otherDestinationType)
        {
            _typeMapActions.Add(tm => tm.IncludeDerivedTypes(otherSourceType, otherDestinationType));

            _includedDerivedTypes.Add(new TypePair(otherSourceType, otherDestinationType));

            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
        {
            return IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));
        }

        public IMappingExpression<TSource, TDestination> IncludeBase(Type sourceBase, Type destinationBase)
        {
            _includedBaseTypes.Add(new TypePair(sourceBase, destinationBase));

            /*
            var objectType = typeof(object);
            var currentSourceBase = sourceBase;
            var currentDestinationBase = destinationBase;
            while(currentSourceBase != null && currentDestinationBase != null && currentSourceBase != objectType && currentDestinationBase != objectType)
            {
                TypeMap baseTypeMap = Profile.CreateMap(currentSourceBase, currentDestinationBase).TypeMap;
                baseTypeMap.IncludeDerivedTypes(TypeMap.SourceType, TypeMap.DestinationType);
                TypeMap.ApplyInheritedMap(baseTypeMap);
                currentSourceBase = currentSourceBase.BaseType();
                currentDestinationBase = currentDestinationBase.BaseType();
            }
            */
            return this;
        }

        public void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression)
        {
            _typeMapActions.Add(tm => tm.UseCustomProjection(projectionExpression));

            ConvertUsing(projectionExpression.Compile());
        }

        public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
        {
            _typeMapActions.Add(tm => tm.MaxDepth = depth);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructUsingServiceLocator()
        {
            _typeMapActions.Add(tm => tm.ConstructDestinationUsingServiceLocator = true);

            return this;
        }

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var mappingExpression = new MappingExpression<TDestination, TSource>(MemberList.Source);

            _reverseMap = mappingExpression;

            return mappingExpression;
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
            var memberInfo = typeof(TSource).GetMember(sourceMemberName).First();

            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            _sourceMemberConfigurations.Add(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> Substitute(Func<TSource, object> substituteFunc)
        {
            _typeMapActions.Add(tm => tm.Substitution = src => substituteFunc((TSource) src));

            return this;
        }

        public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
        {
            _typeMapActions.Add(tm => tm.UseCustomMapper(source => mappingFunction((TSource)source.SourceValue)));
        }

        public void ConvertUsing(Func<ResolutionContext, TDestination> mappingFunction)
        {
            _typeMapActions.Add(tm => tm.UseCustomMapper(context => mappingFunction(context)));
        }

        public void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction)
        {
            _typeMapActions.Add(tm => tm.UseCustomMapper(source => mappingFunction(source, (TSource)source.SourceValue)));
        }

        public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
        {
            ConvertUsing(converter.Convert);
        }

        public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>
        {
            var converter = new DeferredInstantiatedConverter<TSource, TDestination>(BuildCtor<ITypeConverter<TSource, TDestination>>(typeof(TTypeConverter)));

            ConvertUsing(converter.Convert);
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            _typeMapActions.Add(tm => tm.AddBeforeMapAction((src, dest, ctxt) => beforeFunction((TSource)src, (TDestination)dest)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction)
        {
            _typeMapActions.Add(tm => tm.AddBeforeMapAction((src, dest, ctxt) => beforeFunction((TSource)src, (TDestination)dest, ctxt)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            Action<TSource, TDestination, ResolutionContext> beforeFunction = (src, dest, ctxt) => 
                ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return BeforeMap(beforeFunction);
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            _typeMapActions.Add(tm => tm.AddAfterMapAction((src, dest, ctxt) => afterFunction((TSource)src, (TDestination)dest)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction)
        {
            _typeMapActions.Add(tm => tm.AddAfterMapAction((src, dest, ctxt) => afterFunction((TSource)src, (TDestination)dest, ctxt)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            Action<TSource, TDestination, ResolutionContext> afterFunction = (src, dest, ctxt) 
                => ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest);

            return AfterMap(afterFunction);
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
        {
            return ConstructUsing(ctxt => ctor((TSource)ctxt.SourceValue));
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<ResolutionContext, TDestination> ctor)
        {
            _typeMapActions.Add(tm => tm.DestinationCtor = ctxt => ctor(ctxt));

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor)
        {
            var func = ctor.Compile();

            _typeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return ConstructUsing(ctxt => func((TSource)ctxt.SourceValue));
        }

        private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var expression = new MemberConfigurationExpression(destinationProperty);

            _memberConfigurations.Add(expression);

            memberOptions(expression);
        }

        public void As<T>()
        {
            _typeMapActions.Add(tm => tm.DestinationTypeOverride = typeof(T));
        }

        public IMappingExpression<TSource, TDestination> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
        {
            var ctorParamExpression = new CtorParamConfigurationExpression<TSource>(ctorParamName);

            paramOptions(ctorParamExpression);

            _ctorParamConfigurations.Add(ctorParamExpression);

            return this;
        }

        public void Configure(IProfileConfiguration profile, TypeMap typeMap)
        {
            foreach (var destProperty in typeMap.DestinationTypeDetails.PublicWriteAccessors)
            {
                var attrs = destProperty.GetCustomAttributes(true);
                if (attrs.Any(x => x is IgnoreMapAttribute))
                {
                    ForMember(destProperty.Name, y => y.Ignore());
                }
                if (profile.GlobalIgnores.Contains(destProperty.Name))
                {
                    ForMember(destProperty.Name, y => y.Ignore());
                }
            }

            if (_allMemberOptions != null)
            {
                foreach (var accessor in typeMap.DestinationTypeDetails.PublicReadAccessors)
                {
                    ForDestinationMember(accessor.ToMemberAccessor(), _allMemberOptions);
                }
            }

            foreach (var action in _typeMapActions)
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

            if (_reverseMap != null)
            {
                foreach (var destProperty in typeMap.GetPropertyMaps().Where(pm => pm.IsIgnored()))
                {
                    _reverseMap.ForSourceMember(destProperty.DestinationProperty.Name, opt => opt.Ignore());
                }
                foreach (var includedDerivedType in typeMap.IncludedDerivedTypes)
                {
                    _reverseMap.Include(includedDerivedType.DestinationType, includedDerivedType.SourceType);
                }
            }
        }

        private static Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(Type type)
        {
            return context =>
            {
                if(type.IsGenericTypeDefinition())
                {
                    type = type.MakeGenericType(context.SourceType.GetTypeInfo().GenericTypeArguments);
                }

                var obj = context.Options.ServiceCtor.Invoke(type);

                return (TServiceType)obj;
            };
        }
    }
}

