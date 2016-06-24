using System.Collections;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionExtensions
    {
        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Label("LoopBreak");

            var loop = Block(new[] { enumeratorVar },
                enumeratorAssign,
                Loop(
                    IfThenElse(
                        Equal(moveNextCall, Constant(true)),
                        Block(new[] { loopVar },
                            Assign(loopVar, Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static Expression ToObject(this Expression expression)
        {
            return expression.ToType(typeof(object));
        }

        public static Expression ToType(this Expression expression, Type type)
        {
            return expression.Type == type ? expression : Convert(expression, type);
        }

        public static Expression Assign(this Expression left, Expression right)
        {
            return Expression.Assign(left, right);
        }
        
        public static Expression Invk(this Expression expression, params Expression[] arguments)
        {
            return Invoke(expression, arguments);
        }

        public static Expression Condition(this Expression condition, Expression left, Expression right)
        {
            return Expression.Condition(condition, left, right);
        }

        public static MethodCallExpression Call(this Expression expression, string method, Type type = null, params Expression[] arguments)
        {
            return Expression.Call(expression,
                type == null
                    ? expression.Type.GetMethod(method)
                    : expression.Type.GetMethod(method).MakeGenericMethod(type), arguments);
        }
        public static MethodCallExpression Call(this Expression expression, string method, params Expression[] arguments)
        {
            return Call(expression, method, null, arguments);
        }

        public static Expression Index(this Expression array, int index)
        {
            return ArrayIndex(array, Constant(index));
        }

        public static Expression Property(this Expression left, string property)
        {
            return Expression.Property(left, property);
        }
        
        private static readonly ExpressionVisitor IfNullVisitor = new IfNotNullVisitor();

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = replaceExp.Replace(exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = new ConvertingVisitor(exp.Parameters[i], replace[i]).Visit(replaceExp);
            return replaceExp;
        }

        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat) => (LambdaExpression)new ExpressionConcatVisitor(expr).Visit(concat);

        public static Expression IfNotNull(this Expression expression) => IfNullVisitor.Visit(expression);
        public static Expression RemoveIfNotNull(this Expression expression, params Expression[] expressions) => new RemoveIfNotNullVisitor(expressions).Visit(expression);

        public static Expression IfNullElse(this Expression expression, params Expression[] ifElse)
        {
            return ifElse.Any()
                ? Condition(NotEqual(expression, Default(expression.Type)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
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
        internal class RemoveIfNotNullVisitor : ExpressionVisitor
        {
            private readonly Expression[] _expressions;
            private readonly IList<Expression> AllreadyUpdated = new List<Expression>();

            public RemoveIfNotNullVisitor(params Expression[] expressions)
            {
                _expressions = expressions;
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                var member = node.IfFalse as MemberExpression;
                var binary = node.Test as BinaryExpression;
                if(member == null || binary == null || !_expressions.Contains(binary.Left) || !(binary.Right is DefaultExpression))
                    return base.VisitConditional(node);
                return node.IfFalse;
            }
        }

        internal class ConvertingVisitor : ExpressionVisitor
        {
            private readonly Expression _newParam;
            private readonly ParameterExpression _oldParam;

            public ConvertingVisitor(ParameterExpression oldParam, Expression newParam)
            {
                _newParam = newParam;
                _oldParam = oldParam;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return node.Expression == _oldParam
                    ? MakeMemberAccess(ToType(_newParam, _oldParam.Type), node.Member)
                    : base.VisitMember(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? _newParam : base.VisitParameter(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Object == _oldParam
                    ? Expression.Call(_newParam.ToType(_oldParam.Type), node.Method)
                    : base.VisitMethodCall(node);
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

    }
}