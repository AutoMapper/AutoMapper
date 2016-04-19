using AutoMapper.Configuration;
using AutoMapper.Mappers;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;
using static AutoMapper.ExpressionExtensions;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Execution;

    public class ValueResolverConfiguration
    {
        public object Instance { get; }
        public Type Type { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }

        public ValueResolverConfiguration(Type type)
        {
            Type = type;
        }

        public ValueResolverConfiguration(object instance)
        {
            Instance = instance;
        }
    }

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();
        private bool _ignored;
        public int? MappingOrder { get; set; }
        public Func<object, ResolutionContext, object> CustomResolver { get; private set; }
        private bool _sealed;
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        private MemberInfo _sourceMember;
        private LambdaExpression _customExpression;
        internal LambdaExpression MapExpression { get; private set; }

        public PropertyMap(IMemberAccessor destinationProperty, TypeMap typeMap)
        {
            TypeMap = typeMap;
            UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public IMemberAccessor DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.MemberType;

        public IEnumerable<IMemberGetter> SourceMembers => _memberChain;

        public LambdaExpression CustomExpression
        {
            get { return _customExpression; }
            private set
            {
                _customExpression = value;
                if (value != null)
                    SourceType = value.ReturnType;
            }
        }

        public Type SourceType { get; private set; }

        public MemberInfo SourceMember
        {
            get
            {
                return _sourceMember ?? _memberChain.LastOrDefault()?.MemberInfo;
            }
            internal set
            {
                _sourceMember = value;
                if (value != null)
                    SourceType = value.GetMemberType();
            }
        }

        public bool UseDestinationValue { get; set; }

        public bool ExplicitExpansion { get; set; }
        public object CustomValue { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public void ChainMembers(IEnumerable<IMemberGetter> members)
        {
            var getters = members as IList<IMemberGetter> ?? members.ToList();
            _memberChain.AddRange(getters);
            SourceType = getters.LastOrDefault()?.MemberType;
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if (!CanResolveValue() && inheritedMappedProperty.IsIgnored())
            {
                Ignore();
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            SourceType = SourceType ?? inheritedMappedProperty.SourceType;
            CustomValue = CustomValue ?? inheritedMappedProperty.CustomValue;
        }

        internal void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
            {
                return;
            }

            if (!CanResolveValue())
            {
                MapExpression = null;

                return;
            }

            MapExpression = this.CreateExpression(typeMapRegistry);

            _sealed = true;
        }

        public void AssignCustomExpression<TSource, TMember>(Func<TSource, ResolutionContext, TMember> resolverFunc)
        {
            //Expression<Func<TSource, ResolutionContext, TMember>> expr = (s, c) => resolverFunc(s, c);
            CustomResolver = (s, c) => resolverFunc((TSource) s, c);
            SourceType = typeof (TMember);
            //AssignCustomExpression(expr);
        }

        public void Ignore()
        {
            _ignored = true;
        }

        public bool IsIgnored()
        {
            return _ignored;
        }

        public bool IsMapped()
        {
            return _memberChain.Count > 0 
                || ValueResolverConfig != null 
                || CustomResolver != null 
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null
                || _ignored;
        }

        public bool CanResolveValue()
        {
            return (_memberChain.Count > 0
                || ValueResolverConfig != null
                || CustomResolver != null
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null) && !_ignored;
        }

        public bool Equals(PropertyMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.DestinationProperty, DestinationProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyMap)) return false;
            return Equals((PropertyMap) obj);
        }

        public override int GetHashCode()
        {
            return DestinationProperty.GetHashCode();
        }

        public void SetCustomValueResolverExpression<TSource, TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            var finder = new MemberFinderVisitor();
            finder.Visit(sourceMember);

            if (finder.Member != null)
            {
                SourceMember = finder.Member.Member;
            }
            CustomExpression = sourceMember;

            _ignored = false;
        }

        public object GetDestinationValue(object mappedObject)
        {
            return UseDestinationValue
                ? DestinationProperty.GetValue(mappedObject)
                : null;
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

    internal class ConvertingVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _newParam;
        private readonly ParameterExpression _oldParam;

        public ConvertingVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _newParam = newParam;
            _oldParam = oldParam;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return node.Expression == _oldParam
                ? MakeMemberAccess(Convert(_newParam, _oldParam.Type), node.Member)
                : base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node.Object == _oldParam
                ? Call(Convert(_newParam, _oldParam.Type), node.Method)
                : base.VisitMethodCall(node);
        }
    }

    internal class IfNotNullVisitor : ExpressionVisitor
    {
        private readonly IList<MemberExpression> AllreadyUpdated = new List<MemberExpression>();
        protected override Expression VisitMember(MemberExpression node)
        {
            if (AllreadyUpdated.Contains(node))
                return base.VisitMember(node);
            AllreadyUpdated.Add(node);
            return Visit(DelegateFactory.IfNotNullExpression(node));
        }
    }

    internal class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldExpression;
        private readonly Expression _newExpression;

        public ReplaceExpressionVisitor(Expression oldExpression, Expression newExpression)
        {
            _oldExpression = oldExpression;
            _newExpression = newExpression;
        }

        public override Expression Visit(Expression node)
        {
            return _oldExpression == node ? _newExpression : base.Visit(node);
        }
    }

    internal class ExpressionConcatVisitor : ExpressionVisitor
    {
        private readonly LambdaExpression _overrideExpression;

        public ExpressionConcatVisitor(LambdaExpression overrideExpression)
        {
            _overrideExpression = overrideExpression;
        }

        public override Expression Visit(Expression node)
        {
            if (_overrideExpression == null)
                return node;
            if (node.NodeType != ExpressionType.Lambda && node.NodeType != ExpressionType.Parameter)
            {
                var expression = node;
                if (node.Type == typeof(object))
                    expression = Convert(node, _overrideExpression.Parameters[0].Type);

                return _overrideExpression.ReplaceParameters(expression);
            }
            return base.Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Lambda(Visit(node.Body), node.Parameters);
        }
    }

    internal static class ExpressionVisitors
    {
        private static readonly ExpressionVisitor IfNullVisitor = new IfNotNullVisitor();

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = replaceExp.Replace(exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params ParameterExpression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = new ConvertingVisitor(exp.Parameters[i], replace[i]).Visit(replaceExp);
            return replaceExp;
        }

        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat) => (LambdaExpression)new ExpressionConcatVisitor(expr).Visit(concat);

        public static Expression IfNotNull(this Expression expression) => IfNullVisitor.Visit(expression);

        public static Expression IfNullElse(this Expression expression, params Expression[] ifElse)
        {
            return ifElse.Any()
                ? Condition(NotEqual(expression, Default(expression.Type)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
        }

        public static LambdaExpression CreateExpression(this PropertyMap propertyMap, TypeMapRegistry typeMapRegistry)
        {
            var srcParam = Parameter(propertyMap.TypeMap.SourceType, "src");
            var destParam = Parameter(propertyMap.TypeMap.DestinationType, "dest");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");
            
            var valueResolverExpr = BuildValueResolverFunc(propertyMap, typeMapRegistry, srcParam, ctxtParam);
            var destMember = MakeMemberAccess(destParam,propertyMap.DestinationProperty.MemberInfo);

            Expression getter;

            if (propertyMap.DestinationProperty.MemberInfo is PropertyInfo &&
                ((PropertyInfo)propertyMap.DestinationProperty.MemberInfo).GetGetMethod(true) == null)
            {
                getter = Default(propertyMap.TypeMap.DestinationType);
            }
            else
            {
                getter = destMember;
            }

            var destValueExpr = propertyMap.UseDestinationValue
                ? getter
                : Constant(null, propertyMap.DestinationPropertyType);

            if (propertyMap.SourceType == null
                || (propertyMap.SourceType.IsEnumerableType() && propertyMap.SourceType != typeof(string))
                || typeMapRegistry.GetTypeMap(new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType)) != null
                || ((!EnumMapper.EnumToEnumMapping(new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType)) ||
                  EnumMapper.EnumToNullableTypeMapping(new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType))) &&
                 EnumMapper.EnumToEnumMapping(new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType)))
                || !propertyMap.DestinationPropertyType.IsAssignableFrom(propertyMap.SourceType))
            {
                var mapperProp = MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof(IRuntimeMapper)
                    .GetMethods()
                    .Single(m => m.Name == "Map" && m.IsGenericMethodDefinition)
                    .MakeGenericMethod(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
                var second = Call(
                    mapperProp,
                    mapMethod,
                    valueResolverExpr,
                    destValueExpr,
                    ctxtParam
                    );
                valueResolverExpr = Convert(second, propertyMap.DestinationPropertyType);
            }

            if (propertyMap.Condition != null)
            {
                valueResolverExpr =
                    Condition(
                        Invoke(
                            propertyMap.Condition,
                            srcParam,
                            destParam,
                            ToType(destValueExpr, propertyMap.Condition.Parameters[2].Type),
                            ctxtParam
                            ),
                        Convert(valueResolverExpr, propertyMap.DestinationPropertyType),
                        destValueExpr
                        );
            }
            
            Expression mapperExpr;
            if (propertyMap.DestinationProperty.MemberInfo is FieldInfo)
            {
                {
                    if (propertyMap.SourceType != propertyMap.DestinationPropertyType)
                        mapperExpr = Assign(destMember, Convert(valueResolverExpr, propertyMap.DestinationPropertyType));
                    else
                        mapperExpr = Assign(getter,valueResolverExpr);
                }
            }
            else
            {
                var setter = ((PropertyInfo)propertyMap.DestinationProperty.MemberInfo).GetSetMethod(true);
                if (setter == null)
                {
                    mapperExpr = valueResolverExpr;
                }
                else
                {
                    if (propertyMap.SourceType != propertyMap.DestinationPropertyType)
                        mapperExpr = Assign(destMember, Convert(valueResolverExpr, propertyMap.DestinationPropertyType));
                    else
                        mapperExpr = Assign(destMember, valueResolverExpr);
                }
            }

            if (propertyMap.PreCondition != null)
            {
                mapperExpr = IfThen(
                    Invoke(propertyMap.PreCondition, srcParam, ctxtParam),
                    mapperExpr
                    );
            }

            return Lambda(mapperExpr, srcParam, destParam, ctxtParam);
        }

        private static Expression BuildValueResolverFunc(PropertyMap propertyMap, TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression ctxtParam)
        {
            Expression valueResolverFunc;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if (valueResolverConfig != null)
            {
                Expression ctor;
                Type resolverType;
                if (valueResolverConfig.Instance != null)
                {
                    ctor = Constant(valueResolverConfig.Instance);
                    resolverType = valueResolverConfig.Instance.GetType();
                }
                else
                {
                    ctor = Call(MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("Options")), 
                                typeof(MappingOperationOptions).GetMethod("CreateInstance").MakeGenericMethod(valueResolverConfig.Type)
                                );
                    resolverType = valueResolverConfig.Type;
                }

                Expression sourceFunc;
                if (valueResolverConfig.SourceMember != null)
                {
                    sourceFunc = valueResolverConfig.SourceMember.ReplaceParameters(srcParam);
                }
                else if (valueResolverConfig.SourceMemberName != null)
                {
                    sourceFunc = MakeMemberAccess(srcParam,
                        typeMap.SourceType.GetFieldOrProperty(valueResolverConfig.SourceMemberName));
                }
                else
                {
                    sourceFunc = srcParam;
                }
                
                valueResolverFunc = Convert(Call(ToType(ctor, resolverType), resolverType.GetMethod("Resolve"), sourceFunc, ctxtParam), propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomValue != null)
            {
                valueResolverFunc = Convert(Constant(propertyMap.CustomValue), propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomResolver != null)
            {
                valueResolverFunc = TryCatch(Convert(Invoke(Constant(propertyMap.CustomResolver), srcParam, ctxtParam), propertyMap.DestinationPropertyType), Catch(typeof(Exception), Default(propertyMap.DestinationPropertyType)));
            }
            else if (propertyMap.CustomExpression != null)
            {
                valueResolverFunc = propertyMap.CustomExpression.ReplaceParameters(srcParam).IfNotNull();
            }
            //else if (propertyMap.SourceMember != null)
            //{
            //    valueResolverFunc = MakeMemberAccess(srcParam, propertyMap.SourceMember);
            //}
            else if (propertyMap.SourceMembers.Any()
                && propertyMap.SourceType != null
                )
            {
                var last = propertyMap.SourceMembers.Last();
                if (last.MemberInfo is PropertyInfo && ((PropertyInfo)last.MemberInfo).GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.MemberType);
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression)srcParam,
                        (inner, getter) => getter.MemberInfo is MethodInfo
                            ? getter.MemberInfo.IsStatic()
                                ? Call(null, (MethodInfo)getter.MemberInfo, inner)
                                : (Expression)Call(inner, (MethodInfo)getter.MemberInfo)
                            : MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                        );
                    valueResolverFunc = valueResolverFunc.IfNotNull();
                }
            }
            else
            {
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if (propertyMap.DestinationPropertyType == typeof(string) && valueResolverFunc.Type != typeof(string)
                && typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, propertyMap.DestinationPropertyType)) == null)
            {
                valueResolverFunc = Call(valueResolverFunc, valueResolverFunc.Type.GetMethod("ToString", new Type[0]));
            }

            if (propertyMap.NullSubstitute != null)
            {
                Expression value = Constant(propertyMap.NullSubstitute);
                if (propertyMap.NullSubstitute.GetType() != propertyMap.DestinationPropertyType)
                    value = Convert(value, propertyMap.DestinationPropertyType);
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce, valueResolverFunc, value);
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? propertyMap.DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        Convert(Call(
                            typeof(ObjectCreator).GetMethod("CreateNonNullValue"),
                            Constant(toCreate)
                            ), propertyMap.SourceType));
                }
            }
            
            return valueResolverFunc;
        }
    }
}