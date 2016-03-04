namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly LinkedList<IValueResolver> _sourceValueResolvers = new LinkedList<IValueResolver>();
        private bool _ignored;
        private int _mappingOrder;
        private IValueResolver _customResolver;
        private IValueResolver _customMemberResolver;
        private bool _sealed;
        private Func<object, object, ResolutionContext, bool> _condition = (_, __, ___) => true;
        private bool _hasCondition;
        private Func<ResolutionContext, bool> _preCondition = _ => true;
        private bool _hasPreCondition;
        private Func<ResolutionContext, object> _valueResolverFunc;
        private Action<object, ResolutionContext> _mapperFunc;
        private MemberInfo _sourceMember;

        public PropertyMap(IMemberAccessor destinationProperty)
        {
            UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty)
            : this(inheritedMappedProperty.DestinationProperty)
        {
            if (inheritedMappedProperty.IsIgnored())
                Ignore();
            else
            {
                foreach (var sourceValueResolver in inheritedMappedProperty.GetSourceValueResolvers())
                {
                    ChainResolver(sourceValueResolver);
                }
            }
            ApplyCondition(inheritedMappedProperty._condition);
            SetNullSubstitute(inheritedMappedProperty.NullSubstitute);
            SetMappingOrder(inheritedMappedProperty._mappingOrder);
            CustomExpression = inheritedMappedProperty.CustomExpression;
        }

        public IMemberAccessor DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.MemberType;

        public LambdaExpression CustomExpression { get; private set; }

        public Type SourceType { get; private set; }

        public MemberInfo SourceMember
        {
            get
            {
                return _sourceMember ?? GetSourceValueResolvers().OfType<IMemberGetter>().LastOrDefault()?.MemberInfo;
            }
            internal set { _sourceMember = value; }
        }

        public bool UseDestinationValue { get; set; }

        internal bool HasCustomValueResolver { get; private set; }

        public bool ExplicitExpansion { get; set; }

        public object NullSubstitute { get; private set; }

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

        public object ResolveValue(ResolutionContext context)
        {
            return _valueResolverFunc(context);
        }

        internal void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
            {
                return;
            }

            var resolvers = GetSourceValueResolvers().ToArray();


            SourceType = resolvers.OfType<IMemberResolver>().LastOrDefault()?.MemberType;

            if (resolvers.All(r => r is IMemberResolver)
                && CanResolveValue()
                && !_hasPreCondition
                && !_hasCondition
                && SourceType != null
                && typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationPropertyType)) == null
                && DestinationPropertyType.IsAssignableFrom(SourceType))
            {
                // Build assignable expression here
                var expression2 = resolvers.OfType<IMemberResolver>().Aggregate<IMemberResolver, LambdaExpression>((Expression<Func<ResolutionContext, object>>)
                    (ctxt => ctxt.SourceValue),
                    (expression, resolver) => new ExpressionConcatVisitor(resolver.GetExpression).Visit(expression) as LambdaExpression);
                _valueResolverFunc = (Expression.Lambda(Expression.Convert(expression2.Body, typeof(object)), expression2.Parameters) as Expression<Func<ResolutionContext, object>>).Compile();

                _mapperFunc = (mappedObject, context) =>
                {
                    var result = ResolveValue(context);
                    DestinationProperty.SetValue(mappedObject, result);
                };
                return;
            }
            _valueResolverFunc = resolvers.Aggregate<IValueResolver, Func<ResolutionContext, object>>(
                    ctxt => ctxt.SourceValue,
                    (inner, res) => ctxt => res.Resolve(inner(ctxt), ctxt));

            _mapperFunc = (mappedObject, context) =>
            {
                if (!CanResolveValue() || !ShouldAssignValuePreResolving(context))
                    return;

                var result = ResolveValue(context);

                object destinationValue = GetDestinationValue(mappedObject);

                var declaredSourceType = SourceType ?? context.SourceType;
                var sourceType = result?.GetType() ?? declaredSourceType;
                var destinationType = DestinationProperty.MemberType;

                if (!ShouldAssignValue(result, destinationValue, context))
                    return;

                object propertyValueToAssign = context.Mapper.Map(result, destinationValue, sourceType, destinationType, context);

                DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
            };

            _sealed = true;
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
                if (node.NodeType != ExpressionType.Lambda && node.NodeType != ExpressionType.Parameter)
                {
                    var expression = node;
                    if (node.Type == typeof(object))
                        expression = Expression.Convert(node, _overrideExpression.Parameters[0].Type);
                    var body = _overrideExpression.Body as MemberExpression;
                    expression = Expression.PropertyOrField(expression, body.Member.Name);

                    return expression;
                }
                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda(Visit(node.Body), node.Parameters);
            }
        }
    }
}