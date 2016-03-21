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
    using Configuration;
    using Execution;

    public class ValueResolverConfiguration
    {
        public IValueResolver Instance { get; private set; }
        public Type Type { get; private set; }
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
        private readonly LinkedList<IValueResolver> _sourceValueResolvers = new LinkedList<IValueResolver>();
        private readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();
        private bool _ignored;
        private int _mappingOrder;
        private IValueResolver _customResolver;
        private IValueResolver _customMemberResolver;
        private bool _sealed;
        private Func<object, object, ResolutionContext, bool> _condition = (_, __, ___) => true;
        private bool _hasCondition;
        private Func<ResolutionContext, bool> _preCondition = _ => true;
        private bool _hasPreCondition;
        private Action<object, ResolutionContext> _mapperFunc;
        private MemberInfo _sourceMember;
        private bool _hasCustomValue;

        public PropertyMap(IMemberAccessor destinationProperty, TypeMap typeMap)
        {
            _typeMap = typeMap;
            UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            if (inheritedMappedProperty.IsIgnored())
                Ignore();
            else
            {
                foreach (var sourceValueResolver in inheritedMappedProperty.GetSourceValueResolvers())
                {
                    ChainResolver(sourceValueResolver);
                }
                _memberChain.AddRange(inheritedMappedProperty._memberChain);
            }
            ApplyCondition(inheritedMappedProperty._condition);
            SetNullSubstitute(inheritedMappedProperty.NullSubstitute);
            SetMappingOrder(inheritedMappedProperty._mappingOrder);
            CustomExpression = inheritedMappedProperty.CustomExpression;
        }

        public IMemberAccessor DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.MemberType;

        public IEnumerable<IMemberGetter> SourceMembers => _memberChain;

        public LambdaExpression CustomExpression { get; private set; }

        public Type SourceType { get; private set; }

        public MemberInfo SourceMember
        {
            get
            {
                return _sourceMember ?? _memberChain.LastOrDefault()?.MemberInfo;
            }
            internal set { _sourceMember = value; }
        }

        public bool UseDestinationValue { get; set; }

        internal bool HasCustomValueResolver { get; private set; }

        public bool ExplicitExpansion { get; set; }

        public object CustomValue { get; private set; }
        public object NullSubstitute { get; private set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public void ChainMembers(IEnumerable<IMemberGetter> members)
        {
            _memberChain.AddRange(members);
        }

        public IEnumerable<IValueResolver> GetSourceValueResolvers()
        {
            if (_customMemberResolver != null)
                yield return _customMemberResolver;

            if (_customResolver != null)
                yield return _customResolver;

            foreach (var resolver in _sourceValueResolvers)
            {
                yield return resolver;
            }

            if (NullSubstitute != null)
                yield return new NullReplacementMethod(NullSubstitute);
        }

        public void RemoveLastResolver()
        {
            _sourceValueResolvers.RemoveLast();
        }

        internal void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
            {
                return;
            }

            var resolvers = GetSourceValueResolvers().ToArray();

            SourceType = resolvers.OfType<IMemberResolver>().LastOrDefault()?.MemberType;

            if (!CanResolveValue())
            {
                _mapperFunc = (_, __) => { };
                return;
            }

            Func<object, ResolutionContext, object> valueResolverFunc = BuildValueResolverFunc(resolvers);

            var original = valueResolverFunc;

            if (SourceType == null 
                || (SourceType.IsEnumerableType() && SourceType != typeof (string)) 
                || typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationPropertyType)) != null 
                || ((!EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)) ||
                  EnumMapper.EnumToNullableTypeMapping(new TypePair(SourceType, DestinationPropertyType))) &&
                 EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)))
                || !DestinationPropertyType.IsAssignableFrom(SourceType))
            {
                valueResolverFunc =
                    (mappedObject, context) =>
                        context.Mapper.Map(original(mappedObject, context), GetDestinationValue(mappedObject),
                            original(mappedObject, context)?.GetType() ?? SourceType ?? context.SourceType,
                            DestinationPropertyType, context);
            }

            if (_hasCondition)
            {
                var inner = valueResolverFunc;

                valueResolverFunc = (mappedObject, context) =>
                    ShouldAssignValue(original(mappedObject, context), GetDestinationValue(mappedObject), context)
                        ? inner(mappedObject, context)
                        : GetDestinationValue(mappedObject);
            }

            _mapperFunc = (mappedObject, context) => DestinationProperty.SetValue(mappedObject, valueResolverFunc(mappedObject, context));

            if (_hasPreCondition)
            {
                var inner = _mapperFunc;
                _mapperFunc = (mappedObject, context) =>
                {
                    if (!ShouldAssignValuePreResolving(context))
                        return;

                    inner(mappedObject, context);
                };
            }

            _sealed = true;
        }

        private Func<object, ResolutionContext, object> BuildValueResolverFunc(IValueResolver[] resolvers)
        {
            Func<ResolutionContext, object> valueResolverFunc;
            if (resolvers.All(r => r is IMemberGetter)
                && SourceType != null
                && (!SourceType.IsEnumerableType() || SourceType == typeof (string))
                &&
                ((EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)) &&
                  !EnumMapper.EnumToNullableTypeMapping(new TypePair(SourceType, DestinationPropertyType))) ||
                 !EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationPropertyType)))
                )
            {
                var innerResolver =
                    resolvers.Cast<IMemberResolver>()
                        .Aggregate<IMemberResolver, LambdaExpression>((Expression<Func<ResolutionContext, object>>)
                            (ctxt => ctxt.SourceValue),
                            (expression, resolver) =>
                                (LambdaExpression) new ExpressionConcatVisitor(resolver.GetExpression).Visit(expression));
                var outerResolver =
                    (Expression<Func<ResolutionContext, object>>)
                        Expression.Lambda(Expression.Convert(innerResolver.Body, typeof (object)), innerResolver.Parameters);

                valueResolverFunc = outerResolver.Compile();

                if (!_typeMap.Profile.AllowNullDestinationValues)
                {
                    var inner = valueResolverFunc;

                    valueResolverFunc = ctxt => inner(ctxt) ?? ObjectCreator.CreateNonNullValue(SourceType);
                }
            }
            else
            {
                valueResolverFunc = resolvers.Aggregate<IValueResolver, Func<ResolutionContext, object>>(
                    ctxt => ctxt.SourceValue,
                    (inner, res) => ctxt => res.Resolve(inner(ctxt), ctxt));
            }
            return (src, ctxt) => valueResolverFunc(ctxt);
        }

        public void ChainResolver(IValueResolver valueResolver)
        {
            _sourceValueResolvers.AddLast(valueResolver);
        }

        public void AssignCustomExpression(LambdaExpression customExpression)
        {
            CustomExpression = customExpression;
        }

        public void AssignCustomValueResolver(IValueResolver valueResolver)
        {
            _ignored = false;
            _customResolver = valueResolver;
            ResetSourceMemberChain();
            HasCustomValueResolver = true;
        }

        public void ChainTypeMemberForResolver(IValueResolver valueResolver)
        {
            ResetSourceMemberChain();
            _customMemberResolver = valueResolver;
        }

        public void ChainConstructorForResolver(IValueResolver valueResolver)
        {
            _customResolver = valueResolver;
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
            return _sourceValueResolvers.Count > 0 || HasCustomValueResolver || _ignored;
        }

        public bool CanResolveValue()
        {
            return (_sourceValueResolvers.Count > 0 || HasCustomValueResolver) && !_ignored;
        }

        public void SetNullSubstitute(object nullSubstitute)
        {
            NullSubstitute = nullSubstitute;
        }

        public void AssignCustomValue(object value)
        {
            CustomValue = value;
            _hasCustomValue = true;
        }

        private void ResetSourceMemberChain()
        {
            _sourceValueResolvers.Clear();
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
            _hasCondition = true;
        }

        public void ApplyPreCondition(Func<ResolutionContext, bool> condition)
        {
            _preCondition = condition;
            _hasPreCondition = true;
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
            AssignCustomValueResolver(
                new NullReferenceExceptionSwallowingResolver(
                    new ExpressionBasedResolver<TSource, TMember>(sourceMember)
                    )
                );
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

        public void MapValue(object mappedObject, ResolutionContext context)
        {
            _mapperFunc(mappedObject, context);
        }

        private class ExpressionConcatVisitor : ExpressionVisitor
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

        private class ReplaceExpressionVisitor : ExpressionVisitor
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

    }
}