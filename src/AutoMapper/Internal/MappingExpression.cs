namespace AutoMapper.Internal
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using TypeInfo = AutoMapper.TypeInfo;

    public class MappingExpression : IMappingExpression, IMemberConfigurationExpression
    {
        private readonly TypeMap _typeMap;
        private readonly Func<Type, object> _typeConverterCtor;
        private PropertyMap _propertyMap;

        public MappingExpression(TypeMap typeMap, Func<Type, object> typeConverterCtor)
        {
            _typeMap = typeMap;
            _typeConverterCtor = typeConverterCtor;
        }

        public void ConvertUsing<TTypeConverter>()
        {
            ConvertUsing(typeof(TTypeConverter));
        }

        public void ConvertUsing(Type typeConverterType)
        {
            var interfaceType = typeof(ITypeConverter<,>).MakeGenericType(_typeMap.SourceType, _typeMap.DestinationType);
            var convertMethodType = interfaceType.IsAssignableFrom(typeConverterType) ? interfaceType : typeConverterType;
            var converter = new DeferredInstantiatedConverter(convertMethodType, BuildCtor<object>(typeConverterType));

            _typeMap.UseCustomMapper(converter.Convert);
        }

        public void As(Type typeOverride)
        {
            _typeMap.DestinationTypeOverride = typeOverride;
        }

        public IMappingExpression WithProfile(string profileName)
        {
            _typeMap.Profile = profileName;

            return this;
        }

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
        {
            IMemberAccessor destMember = null;
            var propertyInfo = _typeMap.DestinationType.GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = _typeMap.DestinationType.GetField(name);
                destMember = new FieldAccessor(fieldInfo);
            }
            ForDestinationMember(destMember, memberOptions);
            return new MappingExpression(_typeMap, _typeConverterCtor);
        }

        public IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            MemberInfo srcMember = _typeMap.SourceType.GetMember(sourceMemberName).First();

            var srcConfig = new SourceMappingExpression(_typeMap, srcMember);

            memberOptions(srcConfig);

            return new MappingExpression(_typeMap, _typeConverterCtor);
        }

        private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression> memberOptions)
        {
            _propertyMap = _typeMap.FindOrCreatePropertyMapFor(destinationProperty);

            memberOptions(this);
        }

        public void MapFrom(string sourceMember)
        {
            var members = _typeMap.SourceType.GetMember(sourceMember);
            if (!members.Any()) throw new AutoMapperConfigurationException(
                $"Unable to find source member {sourceMember} on type {_typeMap.SourceType.FullName}");
            if (members.Skip(1).Any()) throw new AutoMapperConfigurationException(
                $"Source member {sourceMember} is ambiguous on type {_typeMap.SourceType.FullName}");
            var member = members.Single();
            _propertyMap.SourceMember = member;
            _propertyMap.AssignCustomValueResolver(member.ToMemberGetter());
        }

        public IResolutionExpression ResolveUsing(IValueResolver valueResolver)
        {
            _propertyMap.AssignCustomValueResolver(valueResolver);

            return new ResolutionExpression(_typeMap.SourceType, _propertyMap);
        }

        public IResolverConfigurationExpression ResolveUsing(Type valueResolverType)
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(valueResolverType));

            ResolveUsing(resolver);

            return new ResolutionExpression(_typeMap.SourceType, _propertyMap);
        }

        public IResolverConfigurationExpression ResolveUsing<TValueResolver>()
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>((typeof(TValueResolver))));

            ResolveUsing(resolver);

            return new ResolutionExpression(_typeMap.SourceType, _propertyMap);
        }

        public void Ignore()
        {
            _propertyMap.Ignore();
        }

        public void UseDestinationValue()
        {
            _propertyMap.UseDestinationValue = true;
        }

        private Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(Type type)
        {
            return context =>
            {
                var obj = context.Options.ServiceCtor?.Invoke(type);
                if (obj != null)
                    return (TServiceType)obj;
                return (TServiceType)_typeConverterCtor(type);
            };
        }

        private class SourceMappingExpression : ISourceMemberConfigurationExpression
        {
            private readonly SourceMemberConfig _sourcePropertyConfig;

            public SourceMappingExpression(TypeMap typeMap, MemberInfo sourceMember)
            {
                _sourcePropertyConfig = typeMap.FindOrCreateSourceMemberConfigFor(sourceMember);
            }

            public void Ignore()
            {
                _sourcePropertyConfig.Ignore();
            }
        }

    }

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IMemberConfigurationExpression<TSource>
    {
        private readonly Func<Type, object> _serviceCtor;
        private readonly IProfileExpression _configurationContainer;
        private PropertyMap _propertyMap;

        public MappingExpression(TypeMap typeMap, Func<Type, object> serviceCtor, IProfileExpression configurationContainer)
        {
            TypeMap = typeMap;
            _serviceCtor = serviceCtor;
            _configurationContainer = configurationContainer;
        }

        public TypeMap TypeMap { get; }

        public IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember,
                                                                   Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            IMemberAccessor destProperty = memberInfo.ToMemberAccessor();
            ForDestinationMember(destProperty, memberOptions);
            return new MappingExpression<TSource, TDestination>(TypeMap, _serviceCtor, _configurationContainer);
        }

        public IMappingExpression<TSource, TDestination> ForMember(string name,
                                                                   Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            IMemberAccessor destMember = null;
            var propertyInfo = TypeMap.DestinationType.GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = TypeMap.DestinationType.GetField(name);
                destMember = new FieldAccessor(fieldInfo);
            }
            ForDestinationMember(destMember, memberOptions);
            return new MappingExpression<TSource, TDestination>(TypeMap, _serviceCtor, _configurationContainer);
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var typeInfo = new TypeInfo(TypeMap.DestinationType);

            typeInfo.PublicWriteAccessors.Each(acc => ForDestinationMember(acc.ToMemberAccessor(), memberOptions));
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TDestination).GetDeclaredProperties().Where(HasAnInaccessibleSetter);
            foreach (var property in properties)
                ForMember(property.Name, opt => opt.Ignore());
            return new MappingExpression<TSource, TDestination>(TypeMap, _serviceCtor, _configurationContainer);
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TSource).GetDeclaredProperties().Where(HasAnInaccessibleSetter);
            foreach (var property in properties)
                ForSourceMember(property.Name, opt => opt.Ignore());
            return new MappingExpression<TSource, TDestination>(TypeMap, _serviceCtor, _configurationContainer);
        }

        private bool HasAnInaccessibleSetter(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource
            where TOtherDestination : TDestination
        {
            return Include(typeof(TOtherSource), typeof(TOtherDestination));
        }

        public IMappingExpression<TSource, TDestination> Include(Type otherSourceType, Type otherDestinationType)
        {
            TypeMap.IncludeDerivedTypes(otherSourceType, otherDestinationType);

            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
        {
            TypeMap baseTypeMap = _configurationContainer.CreateMap<TSourceBase, TDestinationBase>().TypeMap;
            baseTypeMap.IncludeDerivedTypes(typeof(TSource), typeof(TDestination));
            TypeMap.ApplyInheritedMap(baseTypeMap);

            return this;
        }

        public IMappingExpression<TSource, TDestination> WithProfile(string profileName)
        {
            TypeMap.Profile = profileName;

            return this;
        }

        public void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression)
        {
            TypeMap.UseCustomProjection(projectionExpression);
        }

        public void NullSubstitute(object nullSubstitute)
        {
            _propertyMap.SetNullSubstitute(nullSubstitute);
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(typeof(TValueResolver)));

            ResolveUsing(resolver);

            return new ResolutionExpression<TSource, TValueResolver>(_propertyMap);
        }

        public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(valueResolverType));

            ResolveUsing(resolver);

            return new ResolutionExpression<TSource>(_propertyMap);
        }

        public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
        {
            _propertyMap.AssignCustomValueResolver(valueResolver);

            return new ResolutionExpression<TSource>(_propertyMap);
        }

        public void ResolveUsing(Func<TSource, object> resolver)
        {
            _propertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(r => resolver((TSource)r.Value)));
        }

        public void ResolveUsing(Func<ResolutionResult, object> resolver)
        {
            _propertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(resolver));
        }

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            _propertyMap.SetCustomValueResolverExpression(sourceMember);
        }

        public void UseValue<TValue>(TValue value)
        {
            MapFrom(src => value);
        }

        public void UseValue(object value)
        {
            _propertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(src => value));
        }

        public void Condition(Func<TSource, bool> condition)
        {
            Condition(context => condition((TSource)context.Parent.SourceValue));
        }

        public void Condition(Func<ResolutionContext, bool> condition)
        {
            _propertyMap.ApplyCondition(condition);
        }

        public void PreCondition(Func<TSource, bool> condition)
        {
            PreCondition(context => condition((TSource)context.Parent.SourceValue));
        }

        public void PreCondition(Func<ResolutionContext, bool> condition)
        {
            _propertyMap.ApplyPreCondition(condition);
        }

        public void ExplicitExpansion()
        {
            _propertyMap.ExplicitExpansion = true;
        }

        public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
        {
            TypeMap.MaxDepth = depth;
            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructUsingServiceLocator()
        {
            TypeMap.ConstructDestinationUsingServiceLocator = true;

            return this;
        }

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var mappingExpression = _configurationContainer.CreateMap<TDestination, TSource>(MemberList.Source);

            foreach (var destProperty in TypeMap.GetPropertyMaps().Where(pm => pm.IsIgnored()))
            {
                mappingExpression.ForSourceMember(destProperty.DestinationProperty.Name, opt => opt.Ignore());
            }

            foreach (var includedDerivedType in TypeMap.IncludedDerivedTypes)
            {
                mappingExpression.Include(includedDerivedType.DestinationType, includedDerivedType.SourceType);
            }

            return mappingExpression;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression<TSource>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(sourceMember);

            var srcConfig = new SourceMappingExpression(TypeMap, memberInfo);

            memberOptions(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression<TSource>> memberOptions)
        {
            var memberInfo = TypeMap.SourceType.GetMember(sourceMemberName).First();

            var srcConfig = new SourceMappingExpression(TypeMap, memberInfo);

            memberOptions(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> Substitute(Func<TSource, object> substituteFunc)
        {
            TypeMap.Substitution = src => substituteFunc((TSource) src);

            return this;
        }

        public void Ignore()
        {
            _propertyMap.Ignore();
        }

        public void UseDestinationValue()
        {
            _propertyMap.UseDestinationValue = true;
        }

        public void DoNotUseDestinationValue()
        {
            _propertyMap.UseDestinationValue = false;
        }

        public void SetMappingOrder(int mappingOrder)
        {
            _propertyMap.SetMappingOrder(mappingOrder);
        }

        public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
        {
            TypeMap.UseCustomMapper(source => mappingFunction((TSource)source.SourceValue));
        }

        public void ConvertUsing(Func<ResolutionContext, TDestination> mappingFunction)
        {
            TypeMap.UseCustomMapper(context => mappingFunction(context));
        }

        public void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction)
        {
            TypeMap.UseCustomMapper(source => mappingFunction(source, (TSource)source.SourceValue));
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
            TypeMap.AddBeforeMapAction((src, dest) => beforeFunction((TSource)src, (TDestination)dest));

            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            Action<TSource, TDestination> beforeFunction = (src, dest) => ((TMappingAction)_serviceCtor(typeof(TMappingAction))).Process(src, dest);

            return BeforeMap(beforeFunction);
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            TypeMap.AddAfterMapAction((src, dest) => afterFunction((TSource)src, (TDestination)dest));

            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            Action<TSource, TDestination> afterFunction = (src, dest) => ((TMappingAction)_serviceCtor(typeof(TMappingAction))).Process(src, dest);

            return AfterMap(afterFunction);
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
        {
            return ConstructUsing(ctxt => ctor((TSource)ctxt.SourceValue));
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<ResolutionContext, TDestination> ctor)
        {
            TypeMap.DestinationCtor = ctxt => ctor(ctxt);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor)
        {
            var func = ctor.Compile();

            TypeMap.ConstructExpression = ctor;

            return ConstructUsing(ctxt => func((TSource)ctxt.SourceValue));
        }

        private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            _propertyMap = TypeMap.FindOrCreatePropertyMapFor(destinationProperty);

            memberOptions(this);
        }

        public void As<T>()
        {
            TypeMap.DestinationTypeOverride = typeof(T);
        }

        private Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(Type type)
        {
            return context =>
            {
                var obj = context.Options.ServiceCtor?.Invoke(type);
                if (obj != null)
                    return (TServiceType)obj;
                return (TServiceType)_serviceCtor(type);
            };
        }

        private class SourceMappingExpression : ISourceMemberConfigurationExpression<TSource>
        {
            private readonly SourceMemberConfig _sourcePropertyConfig;

            public SourceMappingExpression(TypeMap typeMap, MemberInfo memberInfo)
            {
                _sourcePropertyConfig = typeMap.FindOrCreateSourceMemberConfigFor(memberInfo);
            }

            public void Ignore()
            {
                _sourcePropertyConfig.Ignore();
            }
        }
    }
}

