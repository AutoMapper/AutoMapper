using AutoMapper.Configuration;
using AutoMapper.Mappers;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Configuration;
    using Execution;

    public class ValueResolverConfiguration
    {
        public IValueResolver Instance { get; }
        public Type Type { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }
        public Func<IValueResolver> Constructor { get; set; }

        public ValueResolverConfiguration(Type type)
        {
            Type = type;
        }

        public ValueResolverConfiguration(IValueResolver instance)
        {
            Instance = instance;
        }
    }

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        internal readonly TypeMap _typeMap;
        internal readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();
        internal bool _ignored;
        internal int _mappingOrder;
        internal Func<object, ResolutionContext, object> _customResolverFunc;
        internal bool _sealed;
        internal Func<object, object, ResolutionContext, bool> _condition;
        internal Func<ResolutionContext, bool> _preCondition;
        internal Action<object, ResolutionContext> _mapperFunc;
        internal MemberInfo _sourceMember;
        internal LambdaExpression _customExpression;
        internal Expression<Action<object, object, ResolutionContext>> _finalMapperExpr;
        internal LambdaExpression _mapperExpr;

        public PropertyMap(IMemberAccessor destinationProperty, TypeMap typeMap)
        {
            _typeMap = typeMap;
            UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

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
        public object CustomValue { get; private set; }
        public object NullSubstitute { get; private set; }
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
            _customResolverFunc = _customResolverFunc ?? inheritedMappedProperty._customResolverFunc;
            if (_condition == null && inheritedMappedProperty._condition != null)
            {
                ApplyCondition(inheritedMappedProperty._condition);
            }
            if (NullSubstitute == null)
            {
                SetNullSubstitute(inheritedMappedProperty.NullSubstitute);
            }
            if (_mappingOrder == 0)
            {
                SetMappingOrder(inheritedMappedProperty._mappingOrder);
            }
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
                _mapperFunc = (_, __) => { };
                return;
            }

            var srcParam = Parameter(typeof(object), "src");
            var source = Convert(srcParam, _typeMap.SourceType);
            var destParam = Parameter(typeof(object), "dest");
            var destination = Convert(destParam, _typeMap.DestinationType);

            _mapperExpr = this.CreateExpression(typeMapRegistry);
            var exp = _mapperExpr.ReplaceParameters(source, destination);

            _finalMapperExpr = Lambda<Action<object, object, ResolutionContext>>(exp, srcParam, destParam, _mapperExpr.Parameters[2]);

            var mapperFunc = _finalMapperExpr.Compile();

            _mapperFunc = (dest, ctxt) => GetValue(mapperFunc, ctxt, dest);

            _sealed = true;
        }

        private void GetValue(Action<object, object, ResolutionContext> mapperFunc, ResolutionContext ctxt, object dest)
        {
            mapperFunc(ctxt.SourceValue, dest, ctxt);
        }

        public void AssignCustomExpression<TSource, TMember>(Func<TSource, ResolutionContext, TMember> resolverFunc)
        {
            //Expression<Func<TSource, ResolutionContext, TMember>> expr = (s, c) => resolverFunc(s, c);
            _customResolverFunc = (s, c) => resolverFunc((TSource) s, c);
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

        public void SetMappingOrder(int mappingOrder)
        {
            _mappingOrder = mappingOrder;
        }

        public int GetMappingOrder()
        {
            return _mappingOrder;
        }

        public bool IsMapped()
        {
            return _memberChain.Count > 0 
                || ValueResolverConfig != null 
                || _customResolverFunc != null 
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null
                || _ignored;
        }

        public bool CanResolveValue()
        {
            return (_memberChain.Count > 0
                || ValueResolverConfig != null
                || _customResolverFunc != null
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null) && !_ignored;
        }

        public void SetNullSubstitute(object nullSubstitute)
        {
            NullSubstitute = nullSubstitute;
        }

        public void AssignCustomValue(object value)
        {
            CustomValue = value;
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

        public void ApplyCondition(Func<object, object, ResolutionContext, bool> condition)
        {
            _condition = condition;
        }

        public void ApplyPreCondition(Func<ResolutionContext, bool> condition)
        {
            _preCondition = condition;
        }

        public bool ShouldAssignValue(object resolvedValue, object destinationValue, ResolutionContext context)
        {
            return _condition(resolvedValue, destinationValue, context);
        }

        public bool ShouldAssignValuePreResolving(ResolutionContext context)
        {
            return _preCondition(context);
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

        public void MapValue(object mappedObject, ResolutionContext context)
        {
            _mapperFunc(mappedObject, context);
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
                ? Condition(NotEqual(expression, Constant(null)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
        }

        public static LambdaExpression CreateExpression(this PropertyMap propertyMap, TypeMapRegistry typeMapRegistry)
        {
            var srcParam = Parameter(propertyMap._typeMap.SourceType, "src");
            var destParam = Parameter(propertyMap._typeMap.DestinationType, "dest");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");
            
            var valueResolverExpr = BuildValueResolverFunc(propertyMap, typeMapRegistry, srcParam, ctxtParam);
            var innerResolverExpr = valueResolverExpr;
            var destMember = MakeMemberAccess(destParam,propertyMap.DestinationProperty.MemberInfo);

            Expression getter;

            if (propertyMap.DestinationProperty.MemberInfo is PropertyInfo &&
                ((PropertyInfo)propertyMap.DestinationProperty.MemberInfo).GetGetMethod(true) == null)
            {
                getter = Default(propertyMap._typeMap.DestinationType);
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
                
                var ifTrue = propertyMap.SourceType != null
                    ? (Expression)Constant(propertyMap.SourceType, typeof(Type))
                    : MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("SourceType"));
                var ifFalse = Call(valueResolverExpr, typeof(object).GetMethod("GetType"));

                var a = Condition(Equal(valueResolverExpr.ToObject(), Constant(null)),
                    ifTrue,
                    ifFalse);

                var mapperProp = MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof(IRuntimeMapper).GetMethod("Map", new[] { typeof(object), typeof(object), typeof(Type), typeof(Type), typeof(ResolutionContext) });
                var second = Call(
                    mapperProp,
                    mapMethod,
                    valueResolverExpr.ToObject(),
                    destValueExpr.ToObject(),
                    a,
                    Constant(propertyMap.DestinationPropertyType),
                    ctxtParam
                    );
                valueResolverExpr = Convert(second, propertyMap.DestinationPropertyType);
            }

            if (propertyMap._condition != null)
            {
                valueResolverExpr =
                    Condition(
                        Invoke(
                            Constant(propertyMap._condition),
                            innerResolverExpr.ToObject(),
                            destValueExpr.ToObject(),
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

            if (propertyMap._preCondition != null)
            {
                mapperExpr = IfThen(
                    Invoke(Constant(propertyMap._preCondition), ctxtParam),
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
            var typeMap = propertyMap._typeMap;

            if (valueResolverConfig != null)
            {
                Expression ctor;
                if (valueResolverConfig.Instance != null)
                {
                    ctor = Constant(valueResolverConfig.Instance);
                }
                else if (valueResolverConfig.Constructor != null)
                {
                    ctor = Invoke(Constant(valueResolverConfig.Constructor));
                }
                else
                {
                    ctor = Convert(
                        Invoke(
                            MakeMemberAccess(
                                MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("Options")),
                                typeof(MappingOperationOptions).GetProperty("ServiceCtor"))
                            , Constant(valueResolverConfig.Type)),
                        typeof(IValueResolver)
                        );
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
                
                valueResolverFunc = Convert(Call(ctor, typeof(IValueResolver).GetMethod("Resolve"), sourceFunc.ToObject(), ctxtParam), propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomValue != null)
            {
                valueResolverFunc = Convert(Constant(propertyMap.CustomValue), propertyMap.DestinationPropertyType);
            }
            else if (propertyMap._customResolverFunc != null)
            {
                valueResolverFunc = Convert(Invoke(Constant(propertyMap._customResolverFunc), srcParam, ctxtParam), propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomExpression != null)
            {
                valueResolverFunc = propertyMap.CustomExpression.ReplaceParameters(srcParam).IfNotNull();
            }
            else if (propertyMap._sourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(srcParam, propertyMap._sourceMember);
            }
            else if (propertyMap._memberChain.Any()
                && propertyMap.SourceType != null
                )
            {
                var last = propertyMap._memberChain.Last();
                if (last.MemberInfo is PropertyInfo && ((PropertyInfo)last.MemberInfo).GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.MemberType);
                }
                else
                {
                    valueResolverFunc = propertyMap._memberChain.Aggregate(
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

        public static LambdaExpression CreateExpression(this ConstructorParameterMap paramMap, TypeMapRegistry typeMapRegistry)
        {
            var srcParam = Parameter(paramMap._ctorMap.TypeMap.SourceType, "src");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");

            return Lambda(BuildValueResolverExpr(paramMap, typeMapRegistry, srcParam, ctxtParam), srcParam, ctxtParam);
        }

        private static Expression BuildValueResolverExpr(ConstructorParameterMap paramMap, TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam, ParameterExpression ctxtParam)
        {
            if (paramMap.CustomExpression != null)
                return paramMap.CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull();

            if (paramMap.CustomValueResolver != null)
            {
                return Invoke(Constant(paramMap.CustomValueResolver), srcParam, ctxtParam);
            }

            if (!paramMap.SourceMembers.Any() && paramMap.Parameter.IsOptional)
            {
                return Constant(paramMap.Parameter.DefaultValue);
            }

            if (typeMapRegistry.GetTypeMap(new TypePair(paramMap.SourceType, paramMap.DestinationType)) == null
                && paramMap.Parameter.IsOptional)
            {
                return Constant(paramMap.Parameter.DefaultValue);
            }

            var valueResolverExpr = paramMap.SourceMembers.Aggregate(
                (Expression) Convert(srcParam, paramMap._ctorMap.TypeMap.SourceType),
                (inner, getter) => getter.MemberInfo is MethodInfo
                    ? getter.MemberInfo.IsStatic()
                        ? Call(null, (MethodInfo) getter.MemberInfo, inner)
                        : (Expression) Call(inner, (MethodInfo) getter.MemberInfo)
                    : MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                );
            valueResolverExpr = valueResolverExpr.IfNotNull();

            if ((paramMap.SourceType.IsEnumerableType() && paramMap.SourceType != typeof (string))
                || typeMapRegistry.GetTypeMap(new TypePair(paramMap.SourceType, paramMap.DestinationType)) != null
                || ((!EnumMapper.EnumToEnumMapping(new TypePair(paramMap.SourceType, paramMap.DestinationType)) ||
                     EnumMapper.EnumToNullableTypeMapping(new TypePair(paramMap.SourceType, paramMap.DestinationType))) &&
                    EnumMapper.EnumToEnumMapping(new TypePair(paramMap.SourceType, paramMap.DestinationType)))
                || !paramMap.DestinationType.IsAssignableFrom(paramMap.SourceType))
            {
                /*
                var value = context.Mapper.Map(result, null, sourceType, destinationType, context);
                 */

                var mapperProp = MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof (IRuntimeMapper).GetMethod("Map",
                    new[] {typeof (object), typeof (object), typeof (Type), typeof (Type), typeof (ResolutionContext)});
                valueResolverExpr = Call(
                    mapperProp,
                    mapMethod,
                    valueResolverExpr.ToObject(),
                    Constant(null),
                    Constant(paramMap.SourceType),
                    Constant(paramMap.DestinationType),
                    ctxtParam
                    );
            }


            return valueResolverExpr;
        }
    }
}