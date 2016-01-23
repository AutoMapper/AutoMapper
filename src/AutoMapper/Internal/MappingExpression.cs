namespace AutoMapper.Internal
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MappingExpression : MappingExpression<object, object>, IMappingExpression, IMemberConfigurationExpression
    {
        public MappingExpression(TypeMap typeMap, Func<Type, object> typeConverterCtor, IProfileExpression configurationContainer) : base(typeMap, typeConverterCtor, configurationContainer)
        {
        }

        public new IMappingExpression ReverseMap()
        {
            var mappingExpression = Profile.CreateMap(TypeMap.DestinationType, TypeMap.SourceType, MemberList.Source);
            return (IMappingExpression) ConfigureReverseMap((MappingExpression)mappingExpression);
        }

        public new IMappingExpression Substitute(Func<object, object> substituteFunc)
        {
            return (IMappingExpression)base.Substitute(substituteFunc);
        }

        public new IMappingExpression ConstructUsingServiceLocator()
        {
            return (IMappingExpression)base.ConstructUsingServiceLocator();
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions)
        {
            base.ForAllMembers(o => memberOptions((IMemberConfigurationExpression)o));
        }

        void IMappingExpression.ConvertUsing<TTypeConverter>()
        {
            ConvertUsing(typeof(TTypeConverter));
        }

        public void ConvertUsing(Type typeConverterType)
        {
            var interfaceType = typeof(ITypeConverter<,>).MakeGenericType(TypeMap.SourceType, TypeMap.DestinationType);
            var convertMethodType = interfaceType.IsAssignableFrom(typeConverterType) ? interfaceType : typeConverterType;
            var converter = new DeferredInstantiatedConverter(convertMethodType, BuildCtor<object>(typeConverterType));

            TypeMap.UseCustomMapper(converter.Convert);
        }

        public void As(Type typeOverride)
        {
            TypeMap.DestinationTypeOverride = typeOverride;
        }

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
        {
            return (IMappingExpression) base.ForMember(name, c => memberOptions((IMemberConfigurationExpression)c));
        }

        IMappingExpression IMappingExpression.WithProfile(string profileName)
        {
            return (IMappingExpression) base.WithProfile(profileName);
        }

        public new IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            return (IMappingExpression) base.ForSourceMember(sourceMemberName, memberOptions);
        }

        public void MapFrom(string sourceMember)
        {
            var members = TypeMap.SourceType.GetMember(sourceMember);
            if(!members.Any())
                throw new AutoMapperConfigurationException(
$"Unable to find source member {sourceMember} on type {TypeMap.SourceType.FullName}");
            if(members.Skip(1).Any())
                throw new AutoMapperConfigurationException(
$"Source member {sourceMember} is ambiguous on type {TypeMap.SourceType.FullName}");
            var member = members.Single();
            PropertyMap.SourceMember = member;
            PropertyMap.AssignCustomValueResolver(member.ToMemberGetter());
        }

        public new IMappingExpression Include(Type otherSourceType, Type otherDestinationType)
        {
            return (IMappingExpression) base.Include(otherSourceType, otherDestinationType);
        }

        public new IMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            return (IMappingExpression)base.IgnoreAllPropertiesWithAnInaccessibleSetter();
        }

        public new IMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            return (IMappingExpression)base.IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        }

        public new IMappingExpression IncludeBase(Type sourceBase, Type destinationBase)
        {
            return (IMappingExpression)base.IncludeBase(sourceBase, destinationBase);
        }

        public void ProjectUsing(LambdaExpression projectionExpression)
        {
            TypeMap.UseCustomProjection(projectionExpression);
        }

        public new IMappingExpression BeforeMap(Action<object, object> beforeFunction)
        {
            return (IMappingExpression)base.BeforeMap(beforeFunction);
        }

        public new IMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            return (IMappingExpression)base.BeforeMap<TMappingAction>();
        }

        public new IMappingExpression AfterMap(Action<object, object> afterFunction)
        {
            return (IMappingExpression)base.AfterMap(afterFunction);
        }

        public new IMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            return (IMappingExpression)base.AfterMap<TMappingAction>();
        }

        public new IMappingExpression ConstructUsing(Func<object, object> ctor)
        {
            return (IMappingExpression)base.ConstructUsing(ctor);
        }

        public new IMappingExpression ConstructUsing(Func<ResolutionContext, object> ctor)
        {
            return (IMappingExpression)base.ConstructUsing(ctor);
        }

        public IMappingExpression ConstructProjectionUsing(LambdaExpression ctor)
        {
            var func = ctor.Compile();
            TypeMap.ConstructExpression = ctor;
            return ConstructUsing(ctxt => func.DynamicInvoke(ctxt.SourceValue));
        }

        public new IMappingExpression MaxDepth(int depth)
        {
            return (IMappingExpression)base.MaxDepth(depth);
        }

        public new IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions)
        {
            return (IMappingExpression)base.ForCtorParam(ctorParamName, paramOptions);
        }
    }

    class SourceMappingExpression : ISourceMemberConfigurationExpression
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

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IMemberConfigurationExpression<TSource>
    {
        private readonly Func<Type, object> _serviceCtor;

        public MappingExpression(TypeMap typeMap, Func<Type, object> serviceCtor, IProfileExpression profile)
        {
            TypeMap = typeMap;
            _serviceCtor = serviceCtor;
            Profile = profile;
        }

        public TypeMap TypeMap { get; }

        public PropertyMap PropertyMap { get; private set; }

        public IProfileExpression Profile { get; }

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
            var propertyInfo = TypeMap.DestinationType.GetProperty(name);
            if (propertyInfo != null)
            {
                destMember = new PropertyAccessor(propertyInfo);
            }
            if (destMember == null)
            {
                var fieldInfo = TypeMap.DestinationType.GetField(name);
                if(fieldInfo == null)
                {
                    throw new ArgumentOutOfRangeException("name", "Cannot find a field or property named " + name);
                }
                destMember = new FieldAccessor(fieldInfo);
            }
            ForDestinationMember(destMember, memberOptions);
            return this;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions)
        {
            var typeInfo = new TypeDetails(TypeMap.DestinationType, Profile.ShouldMapProperty, Profile.ShouldMapField);

            typeInfo.PublicWriteAccessors.Each(acc => ForDestinationMember(acc.ToMemberAccessor(), memberOptions));
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TDestination).GetDeclaredProperties().Where(HasAnInaccessibleSetter);
            foreach (var property in properties)
                ForMember(property.Name, opt => opt.Ignore());
            return this;
        }

        public IMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            var properties = typeof(TSource).GetDeclaredProperties().Where(HasAnInaccessibleSetter);
            foreach (var property in properties)
                ForSourceMember(property.Name, opt => opt.Ignore());
            return this;
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
            return IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));
        }

        public IMappingExpression<TSource, TDestination> IncludeBase(Type sourceBase, Type destinationBase)
        {
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
            ConvertUsing(projectionExpression.Compile());
        }

        public void NullSubstitute(object nullSubstitute)
        {
            PropertyMap.SetNullSubstitute(nullSubstitute);
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(typeof(TValueResolver)));

            ResolveUsing(resolver);

            return new ResolutionExpression<TSource, TValueResolver>(TypeMap.SourceType, PropertyMap);
        }

        public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
        {
            var resolver = new DeferredInstantiatedResolver(BuildCtor<IValueResolver>(valueResolverType));

            ResolveUsing(resolver);

            return new ResolutionExpression<TSource>(TypeMap.SourceType, PropertyMap);
        }

        public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
        {
            PropertyMap.AssignCustomValueResolver(valueResolver);

            return new ResolutionExpression<TSource>(TypeMap.SourceType, PropertyMap);
        }

        public void ResolveUsing(Func<TSource, object> resolver)
        {
            PropertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(r => resolver((TSource)r.Value)));
        }

        public void ResolveUsing(Func<ResolutionResult, object> resolver)
        {
            PropertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(resolver));
        }

        public void ResolveUsing(Func<ResolutionResult, TSource, object> resolver)
        {
            PropertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(r => resolver(r, (TSource)r.Value)));
        }

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            PropertyMap.SetCustomValueResolverExpression(sourceMember);
        }

        public void MapFrom<TMember>(string property)
        {
            var par = Expression.Parameter(typeof (TSource));
            var prop = Expression.Property(par, property);
            var lambda = Expression.Lambda<Func<TSource, TMember>>(prop, par);
            PropertyMap.SetCustomValueResolverExpression(lambda);
        }

        public void UseValue<TValue>(TValue value)
        {
            MapFrom(src => value);
        }

        public void UseValue(object value)
        {
            PropertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(src => value));
        }

        public void Condition(Func<TSource, bool> condition)
        {
            Condition(context => condition((TSource)context.Parent.SourceValue));
        }

        public void Condition(Func<ResolutionContext, bool> condition)
        {
            PropertyMap.ApplyCondition(condition);
        }

        public void PreCondition(Func<TSource, bool> condition)
        {
            PreCondition(context => condition((TSource)context.Parent.SourceValue));
        }

        public void PreCondition(Func<ResolutionContext, bool> condition)
        {
            PropertyMap.ApplyPreCondition(condition);
        }

        public void ExplicitExpansion()
        {
            PropertyMap.ExplicitExpansion = true;
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
            var mappingExpression = Profile.CreateMap<TDestination, TSource>(MemberList.Source);

            return ConfigureReverseMap(mappingExpression);
        }

        protected IMappingExpression<TDestination, TSource> ConfigureReverseMap(IMappingExpression<TDestination, TSource> mappingExpression)
        {
            foreach(var destProperty in TypeMap.GetPropertyMaps().Where(pm => pm.IsIgnored()))
            {
                mappingExpression.ForSourceMember(destProperty.DestinationProperty.Name, opt => opt.Ignore());
            }
            foreach(var includedDerivedType in TypeMap.IncludedDerivedTypes)
            {
                mappingExpression.Include(includedDerivedType.DestinationType, includedDerivedType.SourceType);
            }
            return mappingExpression;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(sourceMember);

            var srcConfig = new SourceMappingExpression(TypeMap, memberInfo);

            memberOptions(srcConfig);

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
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
            PropertyMap.Ignore();
        }

        public void UseDestinationValue()
        {
            PropertyMap.UseDestinationValue = true;
        }

        public void DoNotUseDestinationValue()
        {
            PropertyMap.UseDestinationValue = false;
        }

        public void SetMappingOrder(int mappingOrder)
        {
            PropertyMap.SetMappingOrder(mappingOrder);
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
            PropertyMap = TypeMap.FindOrCreatePropertyMapFor(destinationProperty);

            memberOptions(this);
        }

        public void As<T>()
        {
            TypeMap.DestinationTypeOverride = typeof(T);
        }

        public IMappingExpression<TSource, TDestination> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
        {
            var param = TypeMap.ConstructorMap.CtorParams.Single(p => p.Parameter.Name == ctorParamName);

            var ctorParamExpression = new CtorParamConfigurationExpression<TSource>(param);
            param.CanResolve = true;

            paramOptions(ctorParamExpression);

            return this;
        }

        protected Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(Type type)
        {
            return context =>
            {
                if(type.IsGenericTypeDefinition())
                {
                    type = type.MakeGenericType(context.SourceType.GetTypeInfo().GenericTypeArguments);
                }
                var obj = context.Options.ServiceCtor?.Invoke(type);
                if(obj != null)
                    return (TServiceType)obj;
                return (TServiceType)_serviceCtor(type);
            };
        }
    }
}

