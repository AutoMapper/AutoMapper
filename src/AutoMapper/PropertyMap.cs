using AutoMapper.Configuration;
using AutoMapper.Mappers;
using AutoMapper.QueryableExtensions.Impl;

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

            var srcParam = Expression.Parameter(typeof(object), "src");
            var destParam = Expression.Parameter(typeof(object), "dest");
            var ctxtParam = Expression.Parameter(typeof(ResolutionContext), "ctxt");

            var valueResolverExpr = BuildValueResolverFunc(typeMapRegistry, srcParam, ctxtParam);
            var innerResolverExpr = valueResolverExpr;
            var destMember = Expression.MakeMemberAccess(
                Expression.Convert(destParam, _typeMap.DestinationType),
                DestinationProperty.MemberInfo);

            Expression getter;

            if (DestinationProperty.MemberInfo is PropertyInfo &&
                ((PropertyInfo) DestinationProperty.MemberInfo).GetGetMethod(true) == null)
            {
                getter = Expression.Default(_typeMap.DestinationType);
            }
            else
            {
                getter = destMember;
            }

            var destValueExpr = UseDestinationValue
                ? getter
                : Expression.Constant(null, DestinationPropertyType);

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
                var sourceExpr = Expression.Parameter(typeof (object), "source");

                var first = Expression.Assign(sourceExpr, valueResolverExpr.ToObject());
                var ifTrue = SourceType != null 
                    ? (Expression) Expression.Constant(SourceType, typeof(Type)) 
                    : Expression.MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("SourceType"));
                var ifFalse = Expression.Call(sourceExpr, typeof(object).GetMethod("GetType"));

                var mapperProp = Expression.MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof (IRuntimeMapper).GetMethod("Map", new[] {typeof (object), typeof (object), typeof (Type), typeof (Type), typeof (ResolutionContext)});
                var second = Expression.Call(
                    mapperProp,
                    mapMethod,
                    sourceExpr,
                    destValueExpr.ToObject(),
                    Expression.Condition(Expression.Equal(sourceExpr, Expression.Constant(null)), 
                        ifTrue,
                        ifFalse),
                    Expression.Constant(DestinationPropertyType),
                    ctxtParam
                    );
                valueResolverExpr = Expression.Block(typeof(object), new[] {sourceExpr}, first, second);
            }

            if (_condition != null)
            {
                //valueResolverFunc = (mappedObject, context) =>
                //    ShouldAssignValue(original(mappedObject, context), GetDestinationValue(mappedObject), context)
                //        ? inner(mappedObject, context)
                //        : GetDestinationValue(mappedObject);
                valueResolverExpr =
                    Expression.Condition(
                        Expression.Invoke(
                            Expression.Constant(_condition),
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
                    mapperExpr = Expression.Call(
                        Expression.Constant(field),
                        typeof (FieldInfo).GetMethod("SetValue", new[] {typeof (object), typeof (object)}),
                        destParam,
                        valueResolverExpr.ToObject());
                }
                else
                {
                    mapperExpr = Expression.Assign(getter,
                        Expression.Convert(valueResolverExpr, DestinationPropertyType));
                }
            }
            else
            {
                var setter = ((PropertyInfo) DestinationProperty.MemberInfo).GetSetMethod(true);
                if (setter == null)
                {
                    mapperExpr = Expression.Convert(valueResolverExpr, DestinationPropertyType);
                }
                else if (DestinationProperty.MemberInfo.DeclaringType.IsValueType())
                {
                    // setter.Invoke(destination, new [] { value })
                    mapperExpr = Expression.Call(
                        Expression.Constant(setter),
                        typeof(MethodInfo).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) }),
                        destParam,
                        Expression.NewArrayInit(typeof(object), valueResolverExpr.ToObject()));
                }
                else
                {
                    mapperExpr = Expression.Assign(destMember, Expression.Convert(valueResolverExpr, DestinationPropertyType));
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
                mapperExpr = Expression.IfThen(
                    Expression.Invoke(Expression.Constant(_preCondition), ctxtParam),
                    mapperExpr
                    );
            }

            _finalMapperExpr = Expression.Lambda<Action<object, object, ResolutionContext>>(mapperExpr, srcParam, destParam, ctxtParam);

            var mapperFunc = _finalMapperExpr.Compile();

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
                    ctor = Expression.Constant(ValueResolverConfig.Instance);
                }
                else if (ValueResolverConfig.Constructor != null)
                {
                    //ctor = ctxt => ValueResolverConfig.Constructor();
                    ctor = Expression.Invoke(Expression.Constant(ValueResolverConfig.Constructor));
                }
                else
                {
                    //ctor = ctxt => (IValueResolver) ctxt.Options.ServiceCtor(ValueResolverConfig.Type);
                    ctor = Expression.Convert(
                        Expression.Invoke(
                            Expression.MakeMemberAccess(
                                Expression.MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                                typeof (MappingOperationOptions).GetProperty("ServiceCtor"))
                            , Expression.Constant(ValueResolverConfig.Type)),
                        typeof (IValueResolver)
                        );
                }

                Expression sourceFunc;
                if (ValueResolverConfig.SourceMember != null)
                {
                    sourceFunc = new ConvertingVisitor(ValueResolverConfig.SourceMember.Parameters[0], srcParam).Visit(ValueResolverConfig.SourceMember.Body);
                }
                else if (ValueResolverConfig.SourceMemberName != null)
                {
                    sourceFunc = Expression.MakeMemberAccess(
                        Expression.Convert(srcParam, _typeMap.SourceType),
                        _typeMap.SourceType.GetFieldOrProperty(ValueResolverConfig.SourceMemberName));
                }
                else
                {
                    sourceFunc = srcParam;
                }

                //valueResolverFunc = ctxt => ctor(ctxt).Resolve(sourceFunc(ctxt), ctxt);
                valueResolverFunc = Expression.Call(ctor, typeof (IValueResolver).GetMethod("Resolve"), sourceFunc.ToObject(), ctxtParam);
            }
            else if (CustomValue != null)
            {
                //valueResolverFunc = ctxt => CustomValue;
                valueResolverFunc = Expression.Constant(CustomValue);
            }
            else if (_customResolverFunc != null)
            {
                //valueResolverFunc = ctxt => _customResolverFunc(ctxt.SourceValue, ctxt);
                valueResolverFunc = Expression.Invoke(Expression.Constant(_customResolverFunc), srcParam, ctxtParam);
            }
            else if (CustomExpression != null)
            {
                var expr = new ConvertingVisitor(CustomExpression.Parameters[0], srcParam).Visit(CustomExpression.Body);
                valueResolverFunc = new IfNotNullVisitor().Visit(expr);
            }
            else if (_sourceMember != null)
            {
                valueResolverFunc = Expression.MakeMemberAccess(Expression.Convert(srcParam, _typeMap.SourceType), _sourceMember);
            }
            else if (_memberChain.Any()
                && SourceType != null
                )
            {
                var last = _memberChain.Last();
                if (last.MemberInfo is PropertyInfo && ((PropertyInfo) last.MemberInfo).GetGetMethod(true) == null)
                {
                    valueResolverFunc = Expression.Default(last.MemberType);
                }
                else
                {
                    valueResolverFunc = _memberChain.Aggregate(
                        (Expression) Expression.Convert(srcParam, _typeMap.SourceType),
                        (inner, getter) => getter.MemberInfo is MethodInfo
                            ? getter.MemberInfo.IsStatic()
                                ? Expression.Call(null, (MethodInfo) getter.MemberInfo, inner)
                                : (Expression) Expression.Call(inner, (MethodInfo) getter.MemberInfo)
                            : Expression.MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                        );
                    valueResolverFunc = new IfNotNullVisitor().Visit(valueResolverFunc);
                }
            }
            else
            {
                //valueResolverFunc = ctxt => { throw new Exception("I done blowed up"); };
                valueResolverFunc = Expression.Throw(Expression.Constant(new Exception("I done blowed up")));
            }

            if (DestinationPropertyType == typeof (string) && valueResolverFunc.Type != typeof (string)
                && typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, DestinationPropertyType)) == null)
            {
                valueResolverFunc = Expression.Call(valueResolverFunc, valueResolverFunc.Type.GetMethod("ToString", new Type[0]));
            }

            if (NullSubstitute != null)
            {
                //var inner = valueResolverFunc;
                //valueResolverFunc = ctxt => inner(ctxt) ?? NullSubstitute;
                valueResolverFunc = Expression.MakeBinary(ExpressionType.Coalesce,
                    valueResolverFunc,
                    Expression.Constant(NullSubstitute));
            }
            else if (!_typeMap.Profile.AllowNullDestinationValues)
            {
                //var inner = valueResolverFunc;
                //valueResolverFunc = ctxt => inner(ctxt) ?? ObjectCreator.CreateNonNullValue(SourceType ?? DestinationPropertyType);
                var toCreate = SourceType ?? DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = Expression.MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        Expression.Call(
                            typeof (ObjectCreator).GetMethod("CreateNonNullValue"),
                            Expression.Constant(toCreate)
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
                ? Expression.MakeMemberAccess(Expression.Convert(_newParam, _oldParam.Type), node.Member)
                : base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node.Object == _oldParam
                ? Expression.Call(Expression.Convert(_newParam, _oldParam.Type), node.Method)
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
            var a = DelegateFactory.IfNotNullExpression(node);
            return Visit(a);
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
                    expression = Expression.Convert(node, _overrideExpression.Parameters[0].Type);


                var a = new ReplaceExpressionVisitor(_overrideExpression.Parameters[0], expression);
                var b = a.Visit(_overrideExpression.Body);
                return b;
            }
            return base.Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Expression.Lambda(Visit(node.Body), node.Parameters);
        }
    }
}