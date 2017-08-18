using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;

namespace AutoMapper.Internal
{
    using static Expression;

    public static class ExpressionFactory
    {
        public static MemberExpression MemberAccesses(string members, Expression obj) =>
            (MemberExpression) ReflectionHelper.GetMemberPath(obj.Type, members).MemberAccesses(obj);

        public static Expression GetSetter(MemberExpression memberExpression)
        {
            var propertyOrField = memberExpression.Member;
            return ReflectionHelper.CanBeSet(propertyOrField) ?
                        MakeMemberAccess(memberExpression.Expression, propertyOrField) :
                        null;
        }

        public static MethodInfo Method<T>(Expression<Func<T>> expression) => GetExpressionBodyMethod(expression);

        public static MethodInfo Method<TType, TResult>(Expression<Func<TType, TResult>> expression) => GetExpressionBodyMethod(expression);

        private static MethodInfo GetExpressionBodyMethod(LambdaExpression expression) => ((MethodCallExpression) expression.Body).Method;

        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            if(collection.Type.IsArray)
            {
                return ForEachArrayItem(collection, arrayItem => Block(new[] { loopVar }, Assign(loopVar, arrayItem), loopContent));
            }
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

        public static Expression ToObject(Expression expression) => ToType(expression, typeof(object));

        public static Expression ToType(Expression expression, Type type) => expression.Type == type ? expression : Convert(expression, type);

        public static Expression ConsoleWriteLine(string value, params Expression[] values) =>
            Call(typeof(Debug).GetDeclaredMethod("WriteLine", new[] {typeof(string), typeof(object[])}),
                Constant(value),
                NewArrayInit(typeof(object), values.Select(ToObject).ToArray()));

        public static Expression ReplaceParameters(LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Length, exp.Parameters.Count); i++)
                replaceExp = Replace(replaceExp, exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression ConvertReplaceParameters(LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Length, exp.Parameters.Count); i++)
                replaceExp = new ConvertingVisitor(exp.Parameters[i], replace[i]).Visit(replaceExp);
            return replaceExp;
        }

        public static Expression Replace(Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        public static LambdaExpression Concat(LambdaExpression expr, LambdaExpression concat) => (LambdaExpression)new ExpressionConcatVisitor(expr).Visit(concat);

        public static Expression IfNotNull(Expression expression, Type destinationType)
        {
            var target = expression;
            Expression nullConditions = Constant(false);
            do
            {
                if(target is MemberExpression member)
                {
                    target = member.Expression;
                    if(!member.Member.IsStatic())
                    {
                        NullCheck();
                    }
                }
                else if(target is MethodCallExpression method)
                {
                    target = method.Method.IsStatic() ? method.Arguments[0] : method.Object;
                    NullCheck();
                }
                else if(target?.NodeType == ExpressionType.Parameter)
                {
                    var returnType = Nullable.GetUnderlyingType(destinationType) == expression.Type ? destinationType : expression.Type;
                    var nullCheck = Condition(nullConditions, Default(returnType), ToType(expression, returnType));
                    return nullCheck;
                }
                else
                {
                    return expression;
                }
            }
            while(true);
            void NullCheck()
            {
                if(target.Type.IsValueType())
                {
                    return;
                }
                nullConditions = OrElse(Equal(target, Constant(null, target.Type)), nullConditions);
            }
        }

        public static Expression RemoveIfNotNull(Expression expression, params Expression[] expressions) => new RemoveIfNotNullVisitor(expressions).Visit(expression);

        public static Expression IfNullElse(Expression expression, Expression then, Expression @else = null)
        {
            var isNull = expression.Type.IsValueType() && !expression.Type.IsNullableType() ? (Expression) Constant(false) : Equal(expression, Constant(null));
            return Condition(isNull, then, ToType(@else ?? Default(then.Type), then.Type));
        }

        internal class RemoveIfNotNullVisitor : ExpressionVisitor
        {
            private readonly Expression[] _expressions;

            public RemoveIfNotNullVisitor(params Expression[] expressions) => _expressions = expressions;

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

            protected override Expression VisitParameter(ParameterExpression node) => 
                node == _oldParam 
                    ? ToType(_newParam, _oldParam.Type) 
                    : base.VisitParameter(node);

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

            public ExpressionConcatVisitor(LambdaExpression overrideExpression) => _overrideExpression = overrideExpression;

            public override Expression Visit(Expression node)
            {
                if (_overrideExpression == null)
                    return node;
                if (node.NodeType != ExpressionType.Lambda && node.NodeType != ExpressionType.Parameter)
                {
                    var expression = node;
                    if (node.Type == typeof(object))
                        expression = Convert(node, _overrideExpression.Parameters[0].Type);

                    return ReplaceParameters(_overrideExpression, expression);
                }
                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node) => Lambda(Visit(node.Body), node.Parameters);
        }
    }
}