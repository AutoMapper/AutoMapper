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
            var getEnumeratorCall = Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

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

        public static Expression ToObject(Expression expression)
        {
            return expression.Type == typeof(object) ? expression : Convert(expression, typeof(object));
        }

        public static Expression ToType(Expression expression, Type type)
        {
            return expression.Type == type ? expression : Convert(expression, type);
        }

        public static Expression ConsoleWriteLine(string value, params Expression[] values)
        {
            return Call(typeof (Debug).GetMethod("WriteLine", new[] {typeof (string), typeof(object[])}), 
                Constant(value), 
                NewArrayInit(typeof(object), values.Select(ToObject).ToArray()));
        }

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

        public static Expression IfNotNull(this Expression expression) => new IfNotNullVisitor().Visit(expression);
        public static Expression RemoveIfNotNull(this Expression expression, params Expression[] expressions) => new RemoveIfNotNullVisitor(expressions).Visit(expression);

        public static Expression IfNullElse(this Expression expression, params Expression[] ifElse)
        {
            return ifElse.Any()
                ? Condition(NotEqual(expression, Default(expression.Type)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
        }

        internal class IfNotNullVisitor : ExpressionVisitor
        {
            private readonly HashSet<MemberExpression> _alreadyUpdated = new HashSet<MemberExpression>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if(_alreadyUpdated.Contains(node))
                {
                    return base.VisitMember(node);
                }
                _alreadyUpdated.Add(node);
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
                return node == _oldParam ? ToType(_newParam, _oldParam.Type) : base.VisitParameter(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Object == _oldParam
                    ? Call(ToType(_newParam, _oldParam.Type), node.Method)
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