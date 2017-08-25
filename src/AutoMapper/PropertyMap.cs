using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using static AutoMapper.Internal.ExpressionFactory;
using static AutoMapper.Execution.ExpressionBuilder;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Execution;

#if NET40
using System.Collections.ObjectModel;
#endif

namespace AutoMapper
{
    using static Expression;

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();

        public PropertyMap(PathMap pathMap)
        {
            DestinationProperty = pathMap.DestinationMember;
            CustomExpression = pathMap.SourceExpression;
            TypeMap = pathMap.TypeMap;

#if NET40
            SourceMembers = new ReadOnlyCollection<MemberInfo>(_memberChain);
#endif
        }

        public PropertyMap(MemberInfo destinationProperty, TypeMap typeMap)
        {
            TypeMap = typeMap;
            DestinationProperty = destinationProperty;

#if NET40
            SourceMembers = new ReadOnlyCollection<MemberInfo>(_memberChain);
#endif
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.GetMemberType();

#if NET40
        public ReadOnlyCollection<MemberInfo> SourceMembers { get; }
#else
        public IReadOnlyCollection<MemberInfo> SourceMembers => _memberChain;
#endif

        public bool Inline { get; set; } = true;
        public bool Ignored { get; set; }
        public bool AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomResolver { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomExpression { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public MemberInfo SourceMember
        {
            get
            {
                if (CustomSourceMemberName != null)
                    return TypeMap.SourceType.GetFieldOrProperty(CustomSourceMemberName);

                if (CustomExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
                }

                return _memberChain.LastOrDefault();
            }
        }

        public Type SourceType
        {
            get
            {
                if (CustomExpression != null)
                    return CustomExpression.ReturnType;
                if (CustomResolver != null)
                    return CustomResolver.ReturnType;
                if(ValueResolverConfig != null)
                    return typeof(object);
                return SourceMember?.GetMemberType();
            }
        }

        public string CustomSourceMemberName { get; set; }

        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if(inheritedMappedProperty.Ignored && !ResolveConfigured())
            {
                Ignored = true;
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            ValueResolverConfig = ValueResolverConfig ?? inheritedMappedProperty.ValueResolverConfig;
        }

        public bool IsMapped() => HasSource() || Ignored;

        public bool CanResolveValue() => HasSource() && !Ignored;

        public bool HasSource() => _memberChain.Count > 0 || ResolveConfigured();

        public bool ResolveConfigured() => ValueResolverConfig != null || CustomResolver != null || CustomExpression != null || CustomSourceMemberName != null;

        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomExpression = sourceMember;
            Ignored = false;
        }

        private class MemberFinderVisitor : ExpressionVisitor
        {
            public MemberExpression Member { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                Member = node;

                return base.VisitMember(node);
            }
        }
    }

    public static class PropertyMapExtension
    {
        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression =
            () => new AutoMapperMappingException(null, null, default(TypePair), null, null);

        internal static Expression TryCatchPropertyMap(this PropertyMap propertyMap, TypeMapPlanBuilder planBuilder)
        {
            var pmExpression = propertyMap.CreatePropertyMapFunc(planBuilder);

            if (pmExpression == null)
                return null;

            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = ((NewExpression)CtorExpression.Body).Constructor;

            return TryCatch(Block(typeof(void), pmExpression),
                MakeCatchBlock(typeof(Exception), exception,
                    Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception,
                        Constant(propertyMap.TypeMap.Types), Constant(propertyMap.TypeMap), Constant(propertyMap))),
                    null));
        }

        internal static Expression CreatePropertyMapFunc(this PropertyMap propertyMap, TypeMapPlanBuilder planBuilder)
        {
            var destMember = MakeMemberAccess(planBuilder.Destination, propertyMap.DestinationProperty);

            Expression getter;

            if (propertyMap.DestinationProperty is PropertyInfo pi && pi.GetGetMethod(true) == null)
                getter = Default(propertyMap.DestinationPropertyType);
            else
                getter = destMember;

            Expression destValueExpr;
            if (propertyMap.UseDestinationValue)
            {
                destValueExpr = getter;
            }
            else
            {
                if (planBuilder.InitialDestination.Type.IsValueType())
                    destValueExpr = Default(propertyMap.DestinationPropertyType);
                else
                    destValueExpr = Condition(Equal(planBuilder.InitialDestination, Constant(null)),
                        Default(propertyMap.DestinationPropertyType), getter);
            }

            var valueResolverExpr = planBuilder.BuildValueResolverFunc(propertyMap, getter);
            var resolvedValue = Variable(valueResolverExpr.Type, "resolvedValue");
            var setResolvedValue = Assign(resolvedValue, valueResolverExpr);
            valueResolverExpr = resolvedValue;

            var typePair = new TypePair(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
            valueResolverExpr = propertyMap.Inline
                ? MapExpression(planBuilder.ConfigurationProvider, planBuilder.TypeMap.Profile, typePair, valueResolverExpr, planBuilder.Context,
                    propertyMap, destValueExpr)
                : ContextMap(typePair, valueResolverExpr, planBuilder.Context, destValueExpr);

            ParameterExpression propertyValue;
            Expression setPropertyValue;
            if (valueResolverExpr == resolvedValue)
            {
                propertyValue = resolvedValue;
                setPropertyValue = setResolvedValue;
            }
            else
            {
                propertyValue = Variable(valueResolverExpr.Type, "propertyValue");
                setPropertyValue = Assign(propertyValue, valueResolverExpr);
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, ToType(propertyValue, propertyMap.DestinationPropertyType))
                    : Assign(getter, propertyValue);
            }
            else
            {
                var setter = ((PropertyInfo)propertyMap.DestinationProperty).GetSetMethod(true);
                if (setter == null)
                    mapperExpr = propertyValue;
                else
                    mapperExpr = Assign(destMember, ToType(propertyValue, propertyMap.DestinationPropertyType));
            }

            mapperExpr = mapperExpr.ConditionalCheck(propertyMap, planBuilder.Source, planBuilder.Destination, propertyValue, getter, planBuilder.Context);

            mapperExpr = Block(new[] { setResolvedValue, setPropertyValue, mapperExpr }.Distinct());

            if (propertyMap.PreCondition != null)
                mapperExpr = IfThen(
                    propertyMap.PreCondition.ConvertReplaceParameters(planBuilder.Source, planBuilder.Context),
                    mapperExpr
                );

            return Block(new[] { resolvedValue, propertyValue }.Distinct(), mapperExpr);
        }

        private static Expression BuildValueResolverFunc(this TypeMapPlanBuilder planBuilder, PropertyMap propertyMap, Expression destValueExpr)
        {
            Expression valueResolverFunc;
            var destinationPropertyType = propertyMap.DestinationPropertyType;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if (valueResolverConfig != null)
            {
                valueResolverFunc = ToType(planBuilder.BuildResolveCall(destValueExpr, valueResolverConfig),
                    destinationPropertyType);
            }
            else if (propertyMap.CustomResolver != null)
            {
                valueResolverFunc =
                    propertyMap.CustomResolver.ConvertReplaceParameters(planBuilder.Source, planBuilder.Destination, destValueExpr, planBuilder.Context);
            }
            else if (propertyMap.CustomExpression != null)
            {
                var nullCheckedExpression = propertyMap.CustomExpression.ReplaceParameters(planBuilder.Source)
                    .IfNotNull(destinationPropertyType);
                var destinationNullable = destinationPropertyType.IsNullableType();
                var returnType = destinationNullable && destinationPropertyType.GetTypeOfNullable() ==
                                 nullCheckedExpression.Type
                    ? destinationPropertyType
                    : nullCheckedExpression.Type;
                valueResolverFunc =
                    TryCatch(
                        ToType(nullCheckedExpression, returnType),
                        Catch(typeof(NullReferenceException), Default(returnType)),
                        Catch(typeof(ArgumentNullException), Default(returnType))
                    );
            }
            else if (propertyMap.SourceMembers.Any()
                     && propertyMap.SourceType != null
            )
            {
                var last = propertyMap.SourceMembers.Last();
                if (last is PropertyInfo pi && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression)planBuilder.Source,
                        (inner, getter) => getter is MethodInfo
                            ? getter.IsStatic()
                                ? Call(null, (MethodInfo)getter, inner)
                                : (Expression)Expression.Call(inner, (MethodInfo)getter)
                            : MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                    );
                    valueResolverFunc = valueResolverFunc.IfNotNull(destinationPropertyType);
                }
            }
            else if (propertyMap.SourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(planBuilder.Source, propertyMap.SourceMember);
            }
            else
            {
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if (propertyMap.NullSubstitute != null)
            {
                var nullSubstitute = Constant(propertyMap.NullSubstitute);
                valueResolverFunc = Coalesce(valueResolverFunc, ToType(nullSubstitute, valueResolverFunc.Type));
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? destinationPropertyType;
                if (!toCreate.IsAbstract() && toCreate.IsClass())
                    valueResolverFunc = Coalesce(
                        valueResolverFunc,
                        ToType(DelegateFactory.GenerateNonNullConstructorExpression(toCreate), propertyMap.SourceType)
                    );
            }

            return valueResolverFunc;
        }

        private static Expression BuildResolveCall(this TypeMapPlanBuilder planBuilder, Expression destValueExpr, ValueResolverConfiguration valueResolverConfig)
        {
            var resolverInstance = valueResolverConfig.Instance != null
                ? Constant(valueResolverConfig.Instance)
                : valueResolverConfig.ConcreteType.CreateInstance(planBuilder.Context);

            var sourceMember = valueResolverConfig.SourceMember?.ReplaceParameters(planBuilder.Source) ??
                               (valueResolverConfig.SourceMemberName != null
                                   ? PropertyOrField(planBuilder.Source, valueResolverConfig.SourceMemberName)
                                   : null);

            var iResolverType = valueResolverConfig.InterfaceType;

            var parameters = new[] { planBuilder.Source, planBuilder.Destination, sourceMember, destValueExpr }.Where(p => p != null)
                .Zip(iResolverType.GetGenericArguments(), ToType)
                .Concat(new[] { planBuilder.Context });
            return Call(ToType(resolverInstance, iResolverType), iResolverType.GetDeclaredMethod("Resolve"),
                parameters);
        }
    }
}
