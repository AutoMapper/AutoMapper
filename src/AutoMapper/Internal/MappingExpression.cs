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

    public class MappingExpression : MappingExpression<object, object>, IMappingExpression
    {
        public MappingExpression(TypePair types, MemberList memberList) : base(memberList, types.SourceType, types.DestinationType)
        {
        }

        public new IMappingExpression ReverseMap() => (IMappingExpression) base.ReverseMap();

        public new IMappingExpression Substitute(Func<object, object> substituteFunc)
            => (IMappingExpression) base.Substitute(substituteFunc);

        public new IMappingExpression ConstructUsingServiceLocator() 
            => (IMappingExpression)base.ConstructUsingServiceLocator();

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions) 
            => base.ForAllMembers(opts => memberOptions((IMemberConfigurationExpression)opts));

        void IMappingExpression.ConvertUsing<TTypeConverter>()
        {
            ConvertUsing(typeof(TTypeConverter));
        }

        public void ConvertUsing(Type typeConverterType)
        {
            var interfaceType = typeof(ITypeConverter<,>).MakeGenericType(Types.SourceType, Types.DestinationType);
            var convertMethodType = interfaceType.IsAssignableFrom(typeConverterType) ? interfaceType : typeConverterType;
            var converter = new DeferredInstantiatedConverter(convertMethodType, typeConverterType.BuildCtor<object>());

            TypeMapActions.Add(tm => tm.UseCustomMapper(converter.Convert));
        }

        public void As(Type typeOverride) => TypeMapActions.Add(tm => tm.DestinationTypeOverride = typeOverride);

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

        public new IMappingExpression ConstructUsing(Func<ResolutionContext, object> ctor) 
            => (IMappingExpression)base.ConstructUsing(ctor);

        public new IMappingExpression ConstructProjectionUsing(Expression<Func<object, object>> ctor)
            => (IMappingExpression) base.ConstructProjectionUsing(ctor);

        public new IMappingExpression MaxDepth(int depth) 
            => (IMappingExpression)base.MaxDepth(depth);

        public new IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions) 
            => (IMappingExpression)base.ForCtorParam(ctorParamName, paramOptions);

        protected override MemberConfigurationExpression<object> CreateMemberConfigurationExpression(IMemberAccessor member, Type sourceType)
            => new MemberConfigurationExpression(member, sourceType);

        private class MemberConfigurationExpression : MemberConfigurationExpression<object>, IMemberConfigurationExpression
        {
            public MemberConfigurationExpression(IMemberAccessor destinationMember, Type sourceType) 
                : base(destinationMember, sourceType)
            {
            }
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

        public void MapFrom<TMember>(string sourceMember)
        {
            var members = _sourceType.GetMember(sourceMember);
            if (!members.Any())
                throw new AutoMapperConfigurationException($"Unable to find source member {sourceMember} on type {_sourceType.FullName}");
            if (members.Skip(1).Any())
                throw new AutoMapperConfigurationException($"Source member {sourceMember} is ambiguous on type {_sourceType.FullName}");

            var par = Expression.Parameter(_sourceType);
            var prop = Expression.Property(par, sourceMember);
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

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, ITypeMapConfiguration
    {
        private readonly List<MemberConfigurationExpression<TSource>> _memberConfigurations = new List<MemberConfigurationExpression<TSource>>();
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

        public MappingExpression(MemberList memberList, Type sourceType, Type destinationType)
        {
            MemberList = memberList;
            Types = new TypePair(sourceType, destinationType);
        }

        public MemberList MemberList { get; }
        public TypePair Types { get; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public ITypeMapConfiguration ReverseTypeMap => _reverseMap;
        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;
        protected List<Action<TypeMap>> TypeMapActions { get; } = new List<Action<TypeMap>>();

        protected virtual MemberConfigurationExpression<TSource> CreateMemberConfigurationExpression(IMemberAccessor member, Type sourceType)
        {
            return new MemberConfigurationExpression<TSource>(member, sourceType);
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
            var propertyInfo = DestinationType.GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = DestinationType.GetField(name);
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
            var properties = DestinationType.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
            foreach (var property in properties)
                ForMember(property.Name, opt => opt.Ignore());
            return this;
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            var properties = SourceType.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
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
            TypeMapActions.Add(tm => tm.IncludeDerivedTypes(otherSourceType, otherDestinationType));

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

            return this;
        }

        public void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression)
        {
            TypeMapActions.Add(tm => tm.UseCustomProjection(projectionExpression));

            ConvertUsing(projectionExpression.Compile());
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
            var memberInfo = SourceType.GetMember(sourceMemberName).First();

            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            _sourceMemberConfigurations.Add(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> Substitute(Func<TSource, object> substituteFunc)
        {
            TypeMapActions.Add(tm => tm.Substitution = src => substituteFunc((TSource) src));

            return this;
        }

        public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm => tm.UseCustomMapper(source => mappingFunction((TSource)source.SourceValue)));
        }

        public void ConvertUsing(Func<ResolutionContext, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm => tm.UseCustomMapper(context => mappingFunction(context)));
        }

        public void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm => tm.UseCustomMapper(source => mappingFunction(source, (TSource)source.SourceValue)));
        }

        public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
        {
            ConvertUsing(converter.Convert);
        }

        public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>
        {
            var converter = new DeferredInstantiatedConverter<TSource, TDestination>(typeof(TTypeConverter).BuildCtor<ITypeConverter<TSource, TDestination>>());

            ConvertUsing(converter.Convert);
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            TypeMapActions.Add(tm => tm.AddBeforeMapAction((src, dest, ctxt) => beforeFunction((TSource)src, (TDestination)dest)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction)
        {
            TypeMapActions.Add(tm => tm.AddBeforeMapAction((src, dest, ctxt) => beforeFunction((TSource)src, (TDestination)dest, ctxt)));

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
            TypeMapActions.Add(tm => tm.AddAfterMapAction((src, dest, ctxt) => afterFunction((TSource)src, (TDestination)dest)));

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction)
        {
            TypeMapActions.Add(tm => tm.AddAfterMapAction((src, dest, ctxt) => afterFunction((TSource)src, (TDestination)dest, ctxt)));

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
            TypeMapActions.Add(tm => tm.DestinationCtor = ctxt => ctor(ctxt));

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor)
        {
            var func = ctor.Compile();

            TypeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return ConstructUsing(ctxt => func((TSource)ctxt.SourceValue));
        }

        private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var expression = CreateMemberConfigurationExpression(destinationProperty, SourceType);

            _memberConfigurations.Add(expression);

            memberOptions(expression);
        }

        public void As<T>()
        {
            TypeMapActions.Add(tm => tm.DestinationTypeOverride = typeof(T));
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
                    _reverseMap?.ForMember(destProperty.Name, opt => opt.Ignore());
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

    }
}

