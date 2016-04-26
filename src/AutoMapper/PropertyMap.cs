using static System.Linq.Expressions.Expression;

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
        private readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();
        private MemberInfo _sourceMember;

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

        public bool Ignored { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomResolver { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomExpression { get; private set; }
        public MemberInfo CustomSourceMember { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object CustomValue { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public MemberInfo SourceMember
        {
            get
            {
                if (CustomSourceMember != null)
                    return CustomSourceMember;

                if (CustomExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
                }

                return _memberChain.LastOrDefault()?.MemberInfo;
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
                return SourceMember?.GetMemberType();
            }
        }

        public void ChainMembers(IEnumerable<IMemberGetter> members)
        {
            var getters = members as IList<IMemberGetter> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if (!CanResolveValue() && inheritedMappedProperty.Ignored)
            {
                Ignored = true;
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            CustomValue = CustomValue ?? inheritedMappedProperty.CustomValue;
            CustomSourceMember = CustomSourceMember ?? inheritedMappedProperty.CustomSourceMember;
        }

        public bool IsMapped()
        {
            return _memberChain.Count > 0 
                || ValueResolverConfig != null 
                || CustomResolver != null 
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null
                || Ignored;
        }

        public bool CanResolveValue()
        {
            return (_memberChain.Count > 0
                || ValueResolverConfig != null
                || CustomResolver != null
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null) && !Ignored;
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