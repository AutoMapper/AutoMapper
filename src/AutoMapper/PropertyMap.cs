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
        private readonly TypeMap _typeMap;
        private readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();
        private bool _ignored;
        private int _mappingOrder;
        private Func<object, ResolutionContext, object> _customResolverFunc;
        private bool _sealed;
        private Func<object, object, ResolutionContext, bool> _condition;
        private Func<ResolutionContext, bool> _preCondition;
        private Action<object, ResolutionContext> _mapperFunc;
        private MemberInfo _sourceMember;
        private LambdaExpression _customExpression;
        private Expression<Action<object, object, ResolutionContext>> _finalMapperExpr;

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
            var destParam = Parameter(typeof(object), "dest");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");

            var valueResolverExpr = BuildValueResolverFunc(typeMapRegistry, srcParam, ctxtParam);
            var innerResolverExpr = valueResolverExpr;
            var destMember = MakeMemberAccess(
                Convert(destParam, _typeMap.DestinationType),
                DestinationProperty.MemberInfo);

            Expression getter;

            if (DestinationProperty.MemberInfo is PropertyInfo &&
                ((PropertyInfo) DestinationProperty.MemberInfo).GetGetMethod(true) == null)
            {
                getter = Default(_typeMap.DestinationType);
            }
            else
            {
                getter = destMember;
            }

            var destValueExpr = UseDestinationValue
                ? getter
                : Constant(null, DestinationPropertyType);

            if (SourceType == null 
                || (SourceType.IsEnumerableType() && SourceType != typeof (string)) 
                || typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationPropertyType)) != null 
                || ((!EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)) ||
                  EnumMapper.EnumToNullableTypeMapping(new TypePair(SourceType, DestinationPropertyType))) &&
                 EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)))
                || !DestinationPropertyType.IsAssignableFrom(SourceType))
            {
                //valueResolverExpr = 
                //    (mappedObject, context) =>
                //    {
                //        var source = original(mappedObject, context);

                //        return context.Mapper.Map(source, GetDestinationValue(mappedObject),
                //            source?.GetType() ?? SourceType ?? context.SourceType,
                //            DestinationPropertyType, context);
                //    };
                var sourceExpr = Parameter(typeof (object), "source");

                var first = Assign(sourceExpr, valueResolverExpr.ToObject());
                var ifTrue = SourceType != null 
                    ? (Expression) Constant(SourceType, typeof(Type)) 
                    : MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("SourceType"));
                var ifFalse = Call(sourceExpr, typeof(object).GetMethod("GetType"));

                var mapperProp = MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof (IRuntimeMapper).GetMethod("Map", new[] {typeof (object), typeof (object), typeof (Type), typeof (Type), typeof (ResolutionContext)});
                var second = Call(
                    mapperProp,
                    mapMethod,
                    sourceExpr,
                    destValueExpr.ToObject(),
                    Condition(Equal(sourceExpr, Constant(null)), 
                        ifTrue,
                        ifFalse),
                    Constant(DestinationPropertyType),
                    ctxtParam
                    );
                valueResolverExpr = Block(typeof(object), new[] {sourceExpr}, first, second);
            }

            if (_condition != null)
            {
                //valueResolverFunc = (mappedObject, context) =>
                //    ShouldAssignValue(original(mappedObject, context), GetDestinationValue(mappedObject), context)
                //        ? inner(mappedObject, context)
                //        : GetDestinationValue(mappedObject);
                valueResolverExpr =
                    Condition(
                        Invoke(
                            Constant(_condition),
                            innerResolverExpr.ToObject(),
                            destValueExpr.ToObject(), 
                            ctxtParam
                            ),
                        valueResolverExpr.ToObject(),
                        destValueExpr.ToObject()
                        );
            }

            //_mapperFunc = (mappedObject, context) => DestinationProperty.SetValue(mappedObject, valueResolverFunc(mappedObject, context));
            Expression mapperExpr;
            if (DestinationProperty.MemberInfo is FieldInfo)
            {
                if (DestinationProperty.MemberInfo.DeclaringType.IsValueType())
                {
                    //field.SetValue(destination, value);
                    var field = (FieldInfo) DestinationProperty.MemberInfo;
                    mapperExpr = Call(
                        Constant(field),
                        typeof (FieldInfo).GetMethod("SetValue", new[] {typeof (object), typeof (object)}),
                        destParam,
                        valueResolverExpr.ToObject());
                }
                else
                {
                    mapperExpr = Assign(getter,
                        Convert(valueResolverExpr, DestinationPropertyType));
                }
            }
            else
            {
                var setter = ((PropertyInfo) DestinationProperty.MemberInfo).GetSetMethod(true);
                if (setter == null)
                {
                    mapperExpr = Convert(valueResolverExpr, DestinationPropertyType);
                }
                else if (DestinationProperty.MemberInfo.DeclaringType.IsValueType())
                {
                    // setter.Invoke(destination, new [] { value })
                    mapperExpr = Call(
                        Constant(setter),
                        typeof(MethodInfo).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) }),
                        destParam,
                        NewArrayInit(typeof(object), valueResolverExpr.ToObject()));
                }
                else
                {
                    mapperExpr = Assign(destMember, Convert(valueResolverExpr, DestinationPropertyType));
                }
            }

            if (_preCondition != null)
            {
                //var inner = _mapperFunc;
                //_mapperFunc = (mappedObject, context) =>
                //{
                //    if (!ShouldAssignValuePreResolving(context))
                //        return;

                //    inner(mappedObject, context);
                //};
                mapperExpr = IfThen(
                    Invoke(Constant(_preCondition), ctxtParam),
                    mapperExpr
                    );
            }

            _finalMapperExpr = Lambda<Action<object, object, ResolutionContext>>(mapperExpr, srcParam, destParam, ctxtParam);
#if NET45
            var gen = DebugInfoGenerator.CreatePdbGenerator();

            var mapperFunc = _finalMapperExpr.Compile();
#else
            var mapperFunc = _finalMapperExpr.Compile();
#endif
            _mapperFunc = (dest, ctxt) => GetValue(mapperFunc, ctxt, dest);

            _sealed = true;
        }

        private void GetValue(Action<object, object, ResolutionContext> mapperFunc, ResolutionContext ctxt, object dest)
        {
            mapperFunc(ctxt.SourceValue, dest, ctxt);
        }

        private Expression BuildValueResolverFunc(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression ctxtParam
            )
        {
            Expression valueResolverFunc;

            if (ValueResolverConfig != null)
            {
                Expression ctor;
                if (ValueResolverConfig.Instance != null)
                {
                    //ctor = ctxt => ValueResolverConfig.Instance;
                    ctor = Constant(ValueResolverConfig.Instance);
                }
                else if (ValueResolverConfig.Constructor != null)
                {
                    //ctor = ctxt => ValueResolverConfig.Constructor();
                    ctor = Invoke(Constant(ValueResolverConfig.Constructor));
                }
                else
                {
                    //ctor = ctxt => (IValueResolver) ctxt.Options.ServiceCtor(ValueResolverConfig.Type);
                    ctor = Convert(
                        Invoke(
                            MakeMemberAccess(
                                MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                                typeof (MappingOperationOptions).GetProperty("ServiceCtor"))
                            , Constant(ValueResolverConfig.Type)),
                        typeof (IValueResolver)
                        );
                }

                Expression sourceFunc;
                if (ValueResolverConfig.SourceMember != null)
                {
                    sourceFunc = ValueResolverConfig.SourceMember.ConvertReplaceParameters(srcParam);
                }
                else if (ValueResolverConfig.SourceMemberName != null)
                {
                    sourceFunc = MakeMemberAccess(
                        Convert(srcParam, _typeMap.SourceType),
                        _typeMap.SourceType.GetFieldOrProperty(ValueResolverConfig.SourceMemberName));
                }
                else
                {
                    sourceFunc = srcParam;
                }

                //valueResolverFunc = ctxt => ctor(ctxt).Resolve(sourceFunc(ctxt), ctxt);
                valueResolverFunc = Call(ctor, typeof (IValueResolver).GetMethod("Resolve"), sourceFunc.ToObject(), ctxtParam);
            }
            else if (CustomValue != null)
            {
                //valueResolverFunc = ctxt => CustomValue;
                valueResolverFunc = Constant(CustomValue);
            }
            else if (_customResolverFunc != null)
            {
                //valueResolverFunc = ctxt => _customResolverFunc(ctxt.SourceValue, ctxt);
                valueResolverFunc = Invoke(Constant(_customResolverFunc), srcParam, ctxtParam);
            }
            else if (CustomExpression != null)
            {
                valueResolverFunc = CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull();
            }
            else if (_sourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(Convert(srcParam, _typeMap.SourceType), _sourceMember);
            }
            else if (_memberChain.Any()
                && SourceType != null
                )
            {
                var last = _memberChain.Last();
                if (last.MemberInfo is PropertyInfo && ((PropertyInfo) last.MemberInfo).GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.MemberType);
                }
                else
                {
                    valueResolverFunc = _memberChain.Aggregate(
                        (Expression) Convert(srcParam, _typeMap.SourceType),
                        (inner, getter) => getter.MemberInfo is MethodInfo
                            ? getter.MemberInfo.IsStatic()
                                ? Call(null, (MethodInfo) getter.MemberInfo, inner)
                                : (Expression) Call(inner, (MethodInfo) getter.MemberInfo)
                            : MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                        );
                    valueResolverFunc = valueResolverFunc.IfNotNull();
                }
            }
            else
            {
                //valueResolverFunc = ctxt => { throw new Exception("I done blowed up"); };
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if (DestinationPropertyType == typeof (string) && valueResolverFunc.Type != typeof (string)
                && typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, DestinationPropertyType)) == null)
            {
                valueResolverFunc = Call(valueResolverFunc, valueResolverFunc.Type.GetMethod("ToString", new Type[0]));
            }

            if (NullSubstitute != null)
            {
                //var inner = valueResolverFunc;
                //valueResolverFunc = ctxt => inner(ctxt) ?? NullSubstitute;
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                    valueResolverFunc,
                    Constant(NullSubstitute));
            }
            else if (!_typeMap.Profile.AllowNullDestinationValues)
            {
                //var inner = valueResolverFunc;
                //valueResolverFunc = ctxt => inner(ctxt) ?? ObjectCreator.CreateNonNullValue(SourceType ?? DestinationPropertyType);
                var toCreate = SourceType ?? DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        Call(
                            typeof (ObjectCreator).GetMethod("CreateNonNullValue"),
                            Constant(toCreate)
                            ));
                }
            }

            //return (src, ctxt) => valueResolverFunc(ctxt);
            return valueResolverFunc;
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
    }
}