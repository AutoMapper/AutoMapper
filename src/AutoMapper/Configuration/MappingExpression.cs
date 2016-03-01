namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

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

        public void ForAllOtherMembers(Action<IMemberConfigurationExpression> memberOptions)
        {
            base.ForAllOtherMembers(o => memberOptions((IMemberConfigurationExpression)o));
        }

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

        public IMappingExpression ConstructProjectionUsing(LambdaExpression ctor)
        {
            var func = ctor.Compile();

            TypeMapActions.Add(tm => tm.ConstructExpression = ctor);

            return ConstructUsing(ctxt => func.DynamicInvoke(ctxt.SourceValue));
        }

        public new IMappingExpression MaxDepth(int depth) 
            => (IMappingExpression)base.MaxDepth(depth);

        public new IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions) 
            => (IMappingExpression)base.ForCtorParam(ctorParamName, paramOptions);

        protected override IMemberConfiguration CreateMemberConfigurationExpression<TMember>(IMemberAccessor member,
            Type sourceType)
        {
            return new MemberConfigurationExpression(member, sourceType);
        }

        protected override MappingExpression<object, object> CreateReverseMapExpression()
        {
            return new MappingExpression(new TypePair(DestinationType, SourceType), MemberList.Source);
        }

        private class MemberConfigurationExpression : MemberConfigurationExpression<object, object>, IMemberConfigurationExpression
        {
            public MemberConfigurationExpression(IMemberAccessor destinationMember, Type sourceType) 
                : base(destinationMember, sourceType)
            {
            }
        }

    }

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, ITypeMapConfiguration
    {
        private readonly List<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();
        private readonly List<SourceMappingExpression> _sourceMemberConfigurations = new List<SourceMappingExpression>();
        private readonly List<CtorParamConfigurationExpression<TSource>> _ctorParamConfigurations = new List<CtorParamConfigurationExpression<TSource>>();
        private MappingExpression<TDestination, TSource> _reverseMap;
        private Action<IMemberConfigurationExpression<TSource, object>> _allMemberOptions;
        private Func<IMemberAccessor, bool> _memberFilter;

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
        protected List<Action<TypeMap>> TypeMapActions { get; } = new List<Action<TypeMap>>();

        protected virtual IMemberConfiguration CreateMemberConfigurationExpression<TMember>(IMemberAccessor member, Type sourceType)
        {
            return new MemberConfigurationExpression<TSource, TMember>(member, sourceType);
        }

        protected virtual MappingExpression<TDestination, TSource> CreateReverseMapExpression()
        {
            return new MappingExpression<TDestination, TSource>(MemberList.Source, DestinationType, SourceType);
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
                                                                   Action<IMemberConfigurationExpression<TSource, TMember>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            IMemberAccessor destProperty = memberInfo.ToMemberAccessor();

            ForDestinationMember(destProperty, memberOptions);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember(string name,
                                                                   Action<IMemberConfigurationExpression<TSource, object>> memberOptions)
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

        public void ForAllOtherMembers(Action<IMemberConfigurationExpression<TSource, object>> memberOptions)
        {
            _allMemberOptions = memberOptions;
            _memberFilter = m => _memberConfigurations.All(c=>c.DestinationMember.MemberInfo != m.MemberInfo);
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource, object>> memberOptions)
        {
            _allMemberOptions = memberOptions;
            _memberFilter = _ => true;
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

            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
        {
            return IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));
        }

        public IMappingExpression<TSource, TDestination> IncludeBase(Type sourceBase, Type destinationBase)
        {
            TypeMapActions.Add(tm => tm.IncludeBaseTypes(sourceBase, destinationBase));

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
            var mappingExpression = CreateReverseMapExpression();

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
            TypeMapActions.Add(tm => tm.UseCustomMapper((source, ctxt) => mappingFunction((TSource) source)));
        }

        public void ConvertUsing(Func<TSource, ResolutionContext, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm => tm.UseCustomMapper((source, ctxt) => mappingFunction((TSource) source, ctxt)));
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

        private void ForDestinationMember<TMember>(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource, TMember>> memberOptions)
        {
            var expression = (MemberConfigurationExpression<TSource, TMember>) CreateMemberConfigurationExpression<TMember>(destinationProperty, SourceType);

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
                foreach (var accessor in typeMap.DestinationTypeDetails.PublicReadAccessors.Select(m=>m.ToMemberAccessor()).Where(_memberFilter))
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

