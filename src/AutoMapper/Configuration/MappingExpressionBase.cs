using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration.Internal;
using AutoMapper.Features;
using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    using static Expression;

    public abstract class MappingExpressionBase : ITypeMapConfiguration
    {
        protected MappingExpressionBase(MemberList memberList, Type sourceType, Type destinationType)
            : this(memberList, new TypePair(sourceType, destinationType))
        {
        }

        protected MappingExpressionBase(MemberList memberList, TypePair types)
        {
            Types = types;
            IsOpenGeneric = types.SourceType.IsGenericTypeDefinition || types.DestinationType.IsGenericTypeDefinition;
            TypeMapActions.Add(tm => tm.ConfiguredMemberList = memberList);
        }
        public TypePair Types { get; }
        public bool IsOpenGeneric { get; }
        public bool IsReverseMap { get; set; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public Features<IMappingFeature> Features { get; } = new Features<IMappingFeature>();
        public ITypeMapConfiguration ReverseTypeMap => ReverseMapExpression;
        public IList<ValueTransformerConfiguration> ValueTransformers { get; } = new List<ValueTransformerConfiguration>();

        protected MappingExpressionBase ReverseMapExpression { get; set; }
        protected List<Action<TypeMap>> TypeMapActions { get; } = new List<Action<TypeMap>>();
        protected List<IPropertyMapConfiguration> MemberConfigurations { get; } = new List<IPropertyMapConfiguration>();
        protected List<ISourceMemberConfiguration> SourceMemberConfigurations { get; } = new List<ISourceMemberConfiguration>();
        protected List<ICtorParameterConfiguration> CtorParamConfigurations { get; } = new List<ICtorParameterConfiguration>();

        public void Configure(TypeMap typeMap)
        {
            foreach(var destProperty in typeMap.DestinationTypeDetails.PublicWriteAccessors)
            {
                if(destProperty.Has<IgnoreMapAttribute>())
                {
                    IgnoreDestinationMember(destProperty);
                    var sourceProperty = typeMap.SourceType.GetInheritedMember(destProperty.Name);
                    if(sourceProperty != null)
                    {
                        ReverseMapExpression?.IgnoreDestinationMember(sourceProperty);
                    }
                }
                if(typeMap.Profile.GlobalIgnores.Contains(destProperty.Name) && GetDestinationMemberConfiguration(destProperty) == null)
                {
                    IgnoreDestinationMember(destProperty);
                }
            }
            MapDestinationCtorToSource(typeMap, CtorParamConfigurations);
            foreach (var action in TypeMapActions)
            {
                action(typeMap);
            }
            foreach(var memberConfig in MemberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
            foreach(var memberConfig in SourceMemberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
            foreach(var paramConfig in CtorParamConfigurations)
            {
                paramConfig.Configure(typeMap);
            }
            foreach(var valueTransformer in ValueTransformers)
            {
                typeMap.AddValueTransformation(valueTransformer);
            }

            Features.Configure(typeMap);

            if(ReverseMapExpression != null)
            {
                ReverseSourceMembers(typeMap);
                foreach(var destProperty in typeMap.PropertyMaps.Where(pm => pm.Ignored))
                {
                    ReverseMapExpression.ForSourceMemberCore(destProperty.DestinationName, opt => opt.DoNotValidate());
                }
                foreach(var includedDerivedType in typeMap.IncludedDerivedTypes)
                {
                    ReverseMapExpression.IncludeCore(includedDerivedType.DestinationType, includedDerivedType.SourceType);
                }
                foreach(var includedBaseType in typeMap.IncludedBaseTypes)
                {
                    ReverseMapExpression.IncludeBaseCore(includedBaseType.DestinationType, includedBaseType.SourceType);
                }
                ReverseIncludedMembers(typeMap);
            }
        }

        private void MapDestinationCtorToSource(TypeMap typeMap, List<ICtorParameterConfiguration> ctorParamConfigurations)
        {
            var ctorMap = typeMap.ConstructorMap;
            if (ctorMap != null)
            {
                foreach (var paramMap in ctorMap.CtorParams)
                {
                    paramMap.CanResolveValue = paramMap.CanResolveValue || IsConfigured(paramMap.Parameter);
                }
                return;
            }
            if (typeMap.DestinationType.IsAbstract || !typeMap.Profile.ConstructorMappingEnabled)
            {
                return;
            }
            foreach (var destCtor in typeMap.DestinationTypeDetails.Constructors.OrderByDescending(ci => ci.GetParameters().Length))
            {
                var ctorParameters = destCtor.GetParameters();
                if (ctorParameters.Length == 0)
                {
                    break;
                }
                ctorMap = new ConstructorMap(destCtor, typeMap);
                foreach (var parameter in ctorParameters)
                {
                    var resolvers = new LinkedList<MemberInfo>();
                    var canResolve = typeMap.Profile.MapDestinationPropertyToSource(typeMap.SourceTypeDetails, destCtor.DeclaringType, parameter.GetType(), parameter.Name, resolvers, IsReverseMap);
                    if ((!canResolve && parameter.IsOptional) || IsConfigured(parameter))
                    {
                        canResolve = true;
                    }
                    ctorMap.AddParameter(parameter, resolvers.ToArray(), canResolve);
                }
                typeMap.ConstructorMap = ctorMap;
                if (ctorMap.CanResolve)
                {
                    break;
                }
            }
            return;
            bool IsConfigured(ParameterInfo parameter) => ctorParamConfigurations.Any(c => c.CtorParamName == parameter.Name);
        }

        protected IEnumerable<IPropertyMapConfiguration> MapToSourceMembers() =>
            MemberConfigurations.Where(m => m.SourceExpression != null && m.SourceExpression.Body == m.SourceExpression.Parameters[0]);

        private void ReverseIncludedMembers(TypeMap typeMap)
        {
            foreach(var includedMember in typeMap.IncludedMembers.Where(i=>i.IsMemberPath()))
            {
                var memberPath = new MemberPath(includedMember);
                var newSource = Parameter(typeMap.DestinationType, "source");
                var customExpression = Lambda(newSource, newSource);
                ReverseSourceMembers(memberPath, customExpression);
            }
        }

        private void ReverseSourceMembers(TypeMap typeMap)
        {
            foreach(var propertyMap in typeMap.PropertyMaps.Where(p => p.SourceMembers.Count > 1 && !p.SourceMembers.Any(s => s is MethodInfo)))
            {
                var memberPath = new MemberPath(propertyMap.SourceMembers);
                var customExpression = ExpressionFactory.MemberAccessLambda(propertyMap.DestinationMember);
                ReverseSourceMembers(memberPath, customExpression);
            }
        }

        private void ReverseSourceMembers(MemberPath memberPath, LambdaExpression customExpression)
        {
            ReverseMapExpression.TypeMapActions.Add(reverseTypeMap =>
            {
                var newDestination = Parameter(reverseTypeMap.DestinationType, "destination");
                var path = memberPath.Members.MemberAccesses(newDestination);
                var forPathLambda = Lambda(path, newDestination);

                var pathMap = reverseTypeMap.FindOrCreatePathMapFor(forPathLambda, memberPath, reverseTypeMap);

                pathMap.CustomMapExpression = customExpression;
            });
        }

        protected void ForSourceMemberCore(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = Types.SourceType.GetFieldOrProperty(sourceMemberName);

            ForSourceMemberCore(memberInfo, memberOptions);
        }

        protected void ForSourceMemberCore(MemberInfo memberInfo, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            SourceMemberConfigurations.Add(srcConfig);
        }

        protected void IncludeCore(Type otherSourceType, Type otherDestinationType)
        {
            CheckIsDerived(otherSourceType, Types.SourceType);
            CheckIsDerived(otherDestinationType, Types.DestinationType);
            TypeMapActions.Add(tm => tm.IncludeDerivedTypes(otherSourceType, otherDestinationType));
        }

        protected void CheckIsDerived(Type derivedType, Type baseType)
        {
            if(!baseType.IsAssignableFrom(derivedType) && !derivedType.IsGenericTypeDefinition && !baseType.IsGenericTypeDefinition)
            {
                throw new ArgumentOutOfRangeException(nameof(derivedType), $"{derivedType} is not derived from {baseType}.");
            }
        }

        protected void IncludeBaseCore(Type sourceBase, Type destinationBase)
        {
            CheckIsDerived(Types.SourceType, sourceBase);
            CheckIsDerived(Types.DestinationType, destinationBase);
            TypeMapActions.Add(tm => tm.IncludeBaseTypes(sourceBase, destinationBase));
        }

        protected IPropertyMapConfiguration GetDestinationMemberConfiguration(MemberInfo destinationMember) =>
            MemberConfigurations.FirstOrDefault(m => m.DestinationMember.Name == destinationMember.Name);

        protected abstract void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true);
    }

    public abstract class MappingExpressionBase<TSource, TDestination, TMappingExpression>
        : MappingExpressionBase, IMappingExpressionBase<TSource, TDestination, TMappingExpression> 
        where TMappingExpression : class, IMappingExpressionBase<TSource, TDestination, TMappingExpression>
    {

        protected MappingExpressionBase(MemberList memberList)
            : base(memberList, typeof(TSource), typeof(TDestination))
        {
        }

        protected MappingExpressionBase(MemberList memberList, Type sourceType, Type destinationType)
            : base(memberList, sourceType, destinationType)
        {
        }

        protected MappingExpressionBase(MemberList memberList, TypePair types)
            : base(memberList, types)
        {
        }

        public TMappingExpression MaxDepth(int depth)
        {
            TypeMapActions.Add(tm => tm.MaxDepth = depth);

            return this as TMappingExpression;
        }

        protected void ReverseFeatures() => Features.ReverseTo(ReverseMapExpression.Features);

        public TMappingExpression ConstructUsingServiceLocator()
        {
            TypeMapActions.Add(tm => tm.ConstructDestinationUsingServiceLocator = true);

            return this as TMappingExpression;
        }

        public TMappingExpression BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest);

                tm.AddBeforeMapAction(expr);
            });

            return this as TMappingExpression;
        }

        public TMappingExpression BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => beforeFunction(src, dest, ctxt);

                tm.AddBeforeMapAction(expr);
            });

            return this as TMappingExpression;
        }

        public TMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void BeforeFunction(TSource src, TDestination dest, ResolutionContext ctxt)
                => ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest, ctxt);

            return BeforeMap(BeforeFunction);
        }

        public TMappingExpression AfterMap(Action<TSource, TDestination> afterFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest);

                tm.AddAfterMapAction(expr);
            });

            return this as TMappingExpression;
        }

        public TMappingExpression AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Action<TSource, TDestination, ResolutionContext>> expr =
                    (src, dest, ctxt) => afterFunction(src, dest, ctxt);

                tm.AddAfterMapAction(expr);
            });

            return this as TMappingExpression;
        }

        public TMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
        {
            void AfterFunction(TSource src, TDestination dest, ResolutionContext ctxt)
                => ((TMappingAction)ctxt.Options.ServiceCtor(typeof(TMappingAction))).Process(src, dest, ctxt);

            return AfterMap(AfterFunction);
        }

        public TMappingExpression PreserveReferences()
        {
            TypeMapActions.Add(tm => tm.PreserveReferences = true);

            return this as TMappingExpression;
        }

        public TMappingExpression DisableCtorValidation()
        {
            TypeMapActions.Add(tm =>
            {
                tm.DisableConstructorValidation = true;
            });

            return this as TMappingExpression;
        }

        public TMappingExpression ValidateMemberList(MemberList memberList)
        {
            TypeMapActions.Add(tm =>
            {
                tm.ConfiguredMemberList = memberList;
            });
            return this as TMappingExpression;
        }

        public TMappingExpression IncludeAllDerived()
        {
            TypeMapActions.Add(tm => tm.IncludeAllDerivedTypes = true);
            return this as TMappingExpression;
        }

        public TMappingExpression Include(Type otherSourceType, Type otherDestinationType)
        {
            IncludeCore(otherSourceType, otherDestinationType);

            return this as TMappingExpression;
        }

        public TMappingExpression IncludeBase(Type sourceBase, Type destinationBase)
        {
            IncludeBaseCore(sourceBase, destinationBase);

            return this as TMappingExpression;
        }

        public TMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            ForSourceMemberCore(sourceMemberName, memberOptions);

            return this as TMappingExpression;
        }

        public void As(Type typeOverride)
        {
            CheckIsDerived(typeOverride, Types.DestinationType);
            TypeMapActions.Add(tm => tm.DestinationTypeOverride = typeOverride);
        }

        public TMappingExpression ConstructUsing(Expression<Func<TSource, TDestination>> ctor)
        {
            TypeMapActions.Add(tm => tm.CustomCtorExpression = ctor);

            return this as TMappingExpression;
        }

        public TMappingExpression ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, ResolutionContext, TDestination>> expr = (src, ctxt) => ctor(src, ctxt);

                tm.CustomCtorFunction = expr;
            });

            return this as TMappingExpression;
        }

        public void ConvertUsing(Type typeConverterType) 
            => TypeMapActions.Add(tm => tm.TypeConverterType = typeConverterType);

        public void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr =
                    (src, dest, ctxt) => mappingFunction(src, dest);

                tm.CustomMapFunction = expr;
            });
        }

        public void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction)
        {
            TypeMapActions.Add(tm =>
            {
                Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr =
                    (src, dest, ctxt) => mappingFunction(src, dest, ctxt);

                tm.CustomMapFunction = expr;
            });
        }

        public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
        {
            ConvertUsing(converter.Convert);
        }

        public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>
        {
            TypeMapActions.Add(tm => tm.TypeConverterType = typeof(TTypeConverter));
        }

        public TMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
        {
            var ctorParamExpression = new CtorParamConfigurationExpression<TSource, TDestination>(ctorParamName, SourceType);

            paramOptions(ctorParamExpression);

            CtorParamConfigurations.Add(ctorParamExpression);

            return this as TMappingExpression;
        }

        public TMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            foreach(var property in Types.DestinationType.PropertiesWithAnInaccessibleSetter())
            {
                IgnoreDestinationMember(property);
            }
            return this as TMappingExpression;
        }

        public TMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            foreach (var property in Types.SourceType.PropertiesWithAnInaccessibleSetter())
            {
                ForSourceMember(property.Name, options => options.DoNotValidate());
            }
            return this as TMappingExpression;
        }

        public void ConvertUsing(Expression<Func<TSource, TDestination>> mappingFunction) =>
            TypeMapActions.Add(tm => tm.CustomMapExpression = mappingFunction);
    }
}