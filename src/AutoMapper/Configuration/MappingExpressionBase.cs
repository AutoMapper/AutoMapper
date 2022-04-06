using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Features;
using AutoMapper.Internal;
namespace AutoMapper.Configuration
{
    using static Expression;
    using Execution;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ITypeMapConfiguration
    {
        void Configure(TypeMap typeMap);
        Type SourceType { get; }
        Type DestinationType { get; }
        bool IsReverseMap { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
        TypeMap TypeMap { get; }
    }
    public abstract class MappingExpressionBase : ITypeMapConfiguration
    {
        private List<ValueTransformerConfiguration> _valueTransformers;
        private Features<IMappingFeature> _features;
        private List<ISourceMemberConfiguration> _sourceMemberConfigurations;
        private List<ICtorParameterConfiguration> _ctorParamConfigurations;
        private List<IPropertyMapConfiguration> _memberConfigurations;
        private readonly MemberList _memberList;
        private readonly TypePair _types;

        protected MappingExpressionBase(MemberList memberList, Type sourceType, Type destinationType) : this(memberList, new TypePair(sourceType, destinationType))
        {
        }
        protected MappingExpressionBase(MemberList memberList, TypePair types)
        {
            _memberList = memberList;
            _types = types;
        }
        protected bool Projection { get; set; }
        public TypePair Types => _types;
        public bool IsReverseMap { get; set; }
        public TypeMap TypeMap { get; private set; }
        public Type SourceType => _types.SourceType;
        public Type DestinationType => _types.DestinationType;
        public Features<IMappingFeature> Features => _features ??= new();
        public ITypeMapConfiguration ReverseTypeMap => ReverseMapExpression;
        public List<ValueTransformerConfiguration> ValueTransformers => _valueTransformers ??= new();
        protected MappingExpressionBase ReverseMapExpression { get; set; }
        protected List<Action<TypeMap>> TypeMapActions { get; } = new List<Action<TypeMap>>();
        protected List<IPropertyMapConfiguration> MemberConfigurations => _memberConfigurations ??= new();
        protected List<ISourceMemberConfiguration> SourceMemberConfigurations => _sourceMemberConfigurations ??= new();
        protected List<ICtorParameterConfiguration> CtorParamConfigurations => _ctorParamConfigurations ??= new();
        public void Configure(TypeMap typeMap)
        {
            TypeMap = typeMap;
            typeMap.Projection = Projection;
            typeMap.ConfiguredMemberList = _memberList;
            var globalIgnores = typeMap.Profile.GlobalIgnores;
            if (globalIgnores.Count > 0)
            {
                GlobalIgnores(typeMap, globalIgnores);
            }
            foreach (var action in TypeMapActions)
            {
                action(typeMap);
            }
            if (typeMap.ConstructorMap == null && typeMap.CanConstructorMap())
            {
                MapDestinationCtorToSource(typeMap);
            }
            if (_memberConfigurations != null)
            {
                foreach (var memberConfig in _memberConfigurations)
                {
                    memberConfig.Configure(typeMap);
                }
            }
            if (_sourceMemberConfigurations != null)
            {
                AddSourceMembersConfigurations(typeMap);
            }
            if (_ctorParamConfigurations != null)
            {
                AddCtorParamConfigurations(typeMap);
            }
            if (_valueTransformers != null)
            {
                AddValueTransformers(typeMap);
            }
            _features?.Configure(typeMap);
            if (ReverseMapExpression != null)
            {
                ConfigureReverseMap(typeMap);
            }
        }

        protected void ReverseMapCore(MappingExpressionBase reverseMap)
        {
            ReverseMapExpression = reverseMap;
            if (_memberConfigurations != null)
            {
                reverseMap.MemberConfigurations.AddRange(_memberConfigurations.Select(m => m.Reverse()).Where(m => m != null));
            }
            _features?.ReverseTo(reverseMap.Features);
        }

        private void AddCtorParamConfigurations(TypeMap typeMap)
        {
            foreach (var paramConfig in _ctorParamConfigurations)
            {
                paramConfig.Configure(typeMap);
            }
        }

        private void AddSourceMembersConfigurations(TypeMap typeMap)
        {
            foreach (var memberConfig in _sourceMemberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
        }

        private void AddValueTransformers(TypeMap typeMap)
        {
            foreach (var valueTransformer in _valueTransformers)
            {
                typeMap.AddValueTransformation(valueTransformer);
            }
        }

        private void ConfigureReverseMap(TypeMap typeMap)
        {
            ReverseSourceMembers(typeMap);
            foreach (var destProperty in typeMap.PropertyMaps.Where(pm => pm.Ignored))
            {
                ReverseMapExpression.ForSourceMemberCore(destProperty.DestinationName, opt => opt.DoNotValidate());
            }
            foreach (var includedDerivedType in typeMap.IncludedDerivedTypes)
            {
                ReverseMapExpression.IncludeCore(includedDerivedType.DestinationType, includedDerivedType.SourceType);
            }
            foreach (var includedBaseType in typeMap.IncludedBaseTypes)
            {
                ReverseMapExpression.IncludeBaseCore(includedBaseType.DestinationType, includedBaseType.SourceType);
            }
            ReverseIncludedMembers(typeMap);
        }

        private void GlobalIgnores(TypeMap typeMap, IReadOnlyCollection<string> globalIgnores)
        {
            foreach (var ignoredPropertyName in globalIgnores.Where(p => GetDestinationMemberConfiguration(p) == null))
            {
                var ignoredProperty = typeMap.DestinationSetters.SingleOrDefault(p => p.Name == ignoredPropertyName);
                if (ignoredProperty != null)
                {
                    IgnoreDestinationMember(ignoredProperty);
                }
            }
        }

        private void MapDestinationCtorToSource(TypeMap typeMap)
        {
            var sourceMembers = new List<MemberInfo>();
            foreach (var destCtor in typeMap.DestinationConstructors)
            {
                var constructor = destCtor.Constructor;
                var ctorMap = new ConstructorMap(constructor, typeMap);
                bool canMapResolve = true;
                foreach (var parameter in destCtor.Parameters)
                {
                    sourceMembers.Clear();
                    var canResolve = typeMap.Profile.MapDestinationPropertyToSource(typeMap.SourceTypeDetails, constructor.DeclaringType, parameter.ParameterType, parameter.Name, sourceMembers, IsReverseMap);
                    if (!canResolve)
                    {
                        if (parameter.IsOptional || IsConfigured(parameter))
                        {
                            canResolve = true;
                        }
                        else
                        {
                            canMapResolve = false;
                        }
                    }
                    ctorMap.AddParameter(parameter, sourceMembers, canResolve);
                }
                typeMap.ConstructorMap = ctorMap;
                if (canMapResolve)
                {
                    ctorMap.CanResolve = true;
                    break;
                }
            }
            return;
            bool IsConfigured(ParameterInfo parameter) => _ctorParamConfigurations?.Any(c => c.CtorParamName == parameter.Name) is true;
        }

        protected IEnumerable<IPropertyMapConfiguration> MapToSourceMembers() =>
            _memberConfigurations?.Where(m => m.SourceExpression != null && m.SourceExpression.Body == m.SourceExpression.Parameters[0]) ?? Array.Empty<IPropertyMapConfiguration>();

        private void ReverseIncludedMembers(TypeMap typeMap)
        {
            Stack<Member> chain = null;
            foreach (var includedMember in typeMap.IncludedMembers.Where(i => i.IsMemberPath(out chain)))
            {
                var memberPath = new MemberPath(chain);
                var newSource = Parameter(typeMap.DestinationType, "source");
                var customExpression = Lambda(newSource, newSource);
                ReverseSourceMembers(memberPath, customExpression);
            }
        }

        private void ReverseSourceMembers(TypeMap typeMap)
        {
            foreach (var propertyMap in typeMap.PropertyMaps.Where(p => p.SourceMembers.Length > 1 && !p.SourceMembers.Any(s => s is MethodInfo)))
            {
                var memberPath = new MemberPath(propertyMap.SourceMembers);
                var customExpression = propertyMap.DestinationMember.Lambda();
                ReverseSourceMembers(memberPath, customExpression);
            }
        }

        private void ReverseSourceMembers(MemberPath memberPath, LambdaExpression customExpression)
        {
            ReverseMapExpression.TypeMapActions.Add(reverseTypeMap =>
            {
                var newDestination = Parameter(reverseTypeMap.DestinationType, "destination");
                var path = memberPath.Members.Chain(newDestination);
                var forPathLambda = Lambda(path, newDestination);

                var pathMap = reverseTypeMap.FindOrCreatePathMapFor(forPathLambda, memberPath, reverseTypeMap);

                pathMap.CustomMapExpression = customExpression;
            });
        }

        protected void ForSourceMemberCore(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var memberInfo = SourceType.GetFieldOrProperty(sourceMemberName);

            ForSourceMemberCore(memberInfo, memberOptions);
        }

        protected void ForSourceMemberCore(MemberInfo memberInfo, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            var srcConfig = new SourceMappingExpression(memberInfo);

            memberOptions(srcConfig);

            SourceMemberConfigurations.Add(srcConfig);
        }

        protected void IncludeCore(Type derivedSourceType, Type derivedDestinationType)
        {
            var derivedTypes = new TypePair(derivedSourceType, derivedDestinationType);
            derivedTypes.CheckIsDerivedFrom(_types);
            TypeMapActions.Add(tm => tm.IncludeDerivedTypes(derivedTypes));
        }


        protected void IncludeBaseCore(Type sourceBase, Type destinationBase)
        {
            var baseTypes = new TypePair(sourceBase, destinationBase);
            _types.CheckIsDerivedFrom(baseTypes);
            TypeMapActions.Add(tm => tm.IncludeBaseTypes(baseTypes));
        }

        protected IPropertyMapConfiguration GetDestinationMemberConfiguration(MemberInfo destinationMember) =>
            GetDestinationMemberConfiguration(destinationMember.Name);

        private IPropertyMapConfiguration GetDestinationMemberConfiguration(string name) =>
            _memberConfigurations?.FirstOrDefault(m => m.DestinationMember.Name == name);

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

        public TMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination> =>
            BeforeMap(CallMapAction<TMappingAction>);
        public TMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination> =>
            AfterMap(CallMapAction<TMappingAction>);
        private static void CallMapAction<TMappingAction>(TSource source, TDestination destination, ResolutionContext context) =>
            ((IMappingAction<TSource, TDestination>)context.CreateInstance(typeof(TMappingAction))).Process(source, destination, context);

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
            if (typeOverride == DestinationType)
            {
                throw new InvalidOperationException("As must specify a derived type, not " + DestinationType);
            }
            typeOverride.CheckIsDerivedFrom(DestinationType);
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
            foreach(var property in PropertiesWithAnInaccessibleSetter(DestinationType))
            {
                IgnoreDestinationMember(property);
            }
            return this as TMappingExpression;
        }

        public TMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            foreach (var property in PropertiesWithAnInaccessibleSetter(SourceType))
            {
                ForSourceMember(property.Name, options => options.DoNotValidate());
            }
            return this as TMappingExpression;
        }

        private static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(Type type) => type.GetRuntimeProperties().Where(p => p.GetSetMethod() == null);

        public void ConvertUsing(Expression<Func<TSource, TDestination>> mappingFunction) =>
            TypeMapActions.Add(tm => tm.CustomMapExpression = mappingFunction);

        public TMappingExpression AsProxy()
        {
            if (!DestinationType.IsInterface)
            {
                throw new InvalidOperationException("Only interfaces can be proxied. " + DestinationType);
            }
            TypeMapActions.Add(tm => tm.AsProxy = true);
            return this as TMappingExpression;
        }
    }
}