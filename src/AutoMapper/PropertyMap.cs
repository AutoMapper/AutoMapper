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
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        private MemberInfo _sourceMember;
        private LambdaExpression _customExpression;

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

        // TODO: make this expression based
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

    }
}