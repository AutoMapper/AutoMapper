using System.Collections;

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
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionExtensions
    {
        public static MethodInfo Method<T>(Expression<Func<T>> expression)
        {
            return ((MethodCallExpression) expression.Body).Method;
        }
        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            if(collection.Type.IsArray)
            {
                return ForEachArrayItem(collection, arrayItem => Block(new[] { loopVar }, Assign(loopVar, arrayItem), loopContent));
            }
            var elementType = loopVar.Type;
            var getEnumerator = collection.Type.GetInheritedMethod("GetEnumerator");
            var getEnumeratorCall = Call(collection, getEnumerator);
            var enumeratorType = getEnumeratorCall.Type;
            var enumeratorVar = Variable(enumeratorType, "enumerator");
            var enumeratorAssign = Assign(enumeratorVar, getEnumeratorCall);

            var moveNext = enumeratorType.GetInheritedMethod("MoveNext");
            var moveNextCall = Call(enumeratorVar, moveNext);

            var breakLabel = Label("LoopBreak");

            var loop = Block(new[] { enumeratorVar },
                enumeratorAssign,
                Loop(
                    IfThenElse(
                        Equal(moveNextCall, Constant(true)),
                        Block(new[] { loopVar },
                            Assign(loopVar, ToType(Property(enumeratorVar, "Current"), loopVar.Type)),
                            loopContent
                        ),
                        Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static Expression ForEachArrayItem(Expression array, Func<Expression, Expression> body)
        {
            var length = Property(array, "Length");
            return For(length, index => body(ArrayAccess(array, index)));
        }

        public static Expression For(Expression count, Func<Expression, Expression> body)
        {
            var breakLabel = Label("LoopBreak");
            var index = Variable(typeof(int), "sourceArrayIndex");
            var initialize = Assign(index, Constant(0, typeof(int)));
            var loop = Block(new[] { index },
                initialize,
                Loop(
                    IfThenElse(
                        LessThan(index, count),
                        Block(body(index), PostIncrementAssign(index)),
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
            return Call(typeof (Debug).GetDeclaredMethod("WriteLine", new[] {typeof (string), typeof(object[])}), 
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

        public static Expression IfNotNull(this Expression expression, Type destinationType)
        {
            var node = expression;
            var isMemberAccess = node.NodeType == ExpressionType.MemberAccess || (node.NodeType == ExpressionType.Call && ((MethodCallExpression)node).Arguments.Count == 0);
            while (isMemberAccess)
            {
                node = (node as MemberExpression)?.Expression 
                    ?? (node as MethodCallExpression)?.Object;
                isMemberAccess =
                    (node != null) && (
                    node.NodeType == ExpressionType.MemberAccess 
                    || (node.NodeType == ExpressionType.Call && ((MethodCallExpression)node).Arguments.Count == 0));
            }
            if(node != null && node.NodeType == ExpressionType.Parameter)
            {
                return new IfNotNullVisitor().VisitRoot(expression, destinationType);
            }
            return expression;
        }

        public static Expression RemoveIfNotNull(this Expression expression, params Expression[] expressions) => new RemoveIfNotNullVisitor(expressions).Visit(expression);

        public static Expression IfNullElse(this Expression expression, params Expression[] ifElse)
        {
            return ifElse.Any()
                ? Condition(NotEqual(expression, Default(expression.Type)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
        }

        internal class IfNotNullVisitor : ExpressionVisitor
        {
            private Expression nullConditions = Constant(false);

            public Expression VisitRoot(Expression node, Type destinationType)
            {
                var returnType = Nullable.GetUnderlyingType(destinationType) == node.Type ? destinationType : node.Type;
                var expression = base.Visit(node);
                var checkNull = Condition(nullConditions, Default(returnType), ToType(expression, returnType));
                return checkNull;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var returnNode = base.VisitMember(node);
                if(node.Expression == null || node.Expression.Type.IsValueType())
                {
                    return returnNode;
                }
                nullConditions = OrElse(nullConditions, Equal(node.Expression, Constant(null, node.Expression.Type)));
                return returnNode;
            }
        }

        internal class RemoveIfNotNullVisitor : ExpressionVisitor
        {
            private readonly Expression[] _expressions;

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
                if (node.Expression == _oldParam)
                {
                    node = MakeMemberAccess(ToType(_newParam, _oldParam.Type), node.Member);
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? ToType(_newParam, _oldParam.Type) : base.VisitParameter(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object == _oldParam)
                {
                    node = Call(ToType(_newParam, _oldParam.Type), node.Method, node.Arguments);
                }

                return base.VisitMethodCall(node);
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
                if (_oldExpression == node)
                    node = _newExpression;

                return base.Visit(node);
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