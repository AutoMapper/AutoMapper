using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper.Internal
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExpressionFactory
    {
        public static Expression Chain(this IEnumerable<Expression> expressions, Expression parameter) => expressions.Aggregate(parameter,
            (left, right) => right is LambdaExpression lambda ? lambda.ReplaceParameters(left) : right.Replace(right.GetChain().FirstOrDefault().Target, left));
        public static LambdaExpression Lambda(this MemberInfo member) => new[] { member }.Lambda();
        public static LambdaExpression Lambda(this IEnumerable<MemberInfo> members)
        {
            var source = Parameter(members.First().DeclaringType, "source");
            return Expression.Lambda(members.MemberAccesses(source), source);
        }

        public static Expression MemberAccesses(this IEnumerable<MemberInfo> members, Expression obj) =>
            members
                .Aggregate(
                        obj,
                        (inner, getter) => getter is MethodInfo method ?
                            (getter.IsStatic() ? Call(null, method, inner) : (Expression)Call(inner, method)) :
                            MakeMemberAccess(getter.IsStatic() ? null : inner, getter));

        public static IEnumerable<MemberInfo> GetMembersChain(this LambdaExpression lambda) => lambda.Body.GetMembersChain();

        public static MemberInfo GetMember(this LambdaExpression lambda) =>
            (lambda?.Body is MemberExpression memberExpression && memberExpression.Expression == lambda.Parameters[0]) ? memberExpression.Member : null;

        public static IEnumerable<MemberInfo> GetMembersChain(this Expression expression) => expression.GetChain().Select(m => m.MemberInfo);

        public static IEnumerable<Member> GetChain(this Expression expression)
        {
            return GetMembersCore().Reverse();
            IEnumerable<Member> GetMembersCore()
            {
                Expression target;
                MemberInfo memberInfo;
                while (expression != null)
                {
                    switch (expression)
                    {
                        case MemberExpression member:
                            target = member.Expression;
                            memberInfo = member.Member;
                            break;
                        case MethodCallExpression methodCall when methodCall.Method.IsStatic:
                            if (methodCall.Arguments.Count == 0 || !methodCall.Method.Has<ExtensionAttribute>())
                            {
                                yield break;
                            }
                            target = methodCall.Arguments[0];
                            memberInfo = methodCall.Method;
                            break;
                        case MethodCallExpression methodCall:
                            target = methodCall.Object;
                            memberInfo = methodCall.Method;
                            break;
                        default:
                            yield break;
                    }
                    yield return new Member(expression, memberInfo, target);
                    expression = target;
                }
            }
        }
        public readonly struct Member
        {
            public Member(Expression expression, MemberInfo memberInfo, Expression target)
            {
                Expression = expression;
                MemberInfo = memberInfo;
                Target = target;
            }
            public Expression Expression { get; }
            public MemberInfo MemberInfo { get; }
            public Expression Target { get; }
        }
        public static IEnumerable<MemberExpression> GetMemberExpressions(this Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
            {
                return Array.Empty<MemberExpression>();
            }
            return expression.GetChain().Select(m => m.Expression as MemberExpression).TakeWhile(m => m != null);
        }
        public static void EnsureMemberPath(this LambdaExpression exp, string name)
        {
            if (!exp.IsMemberPath())
            {
                throw new ArgumentOutOfRangeException(name, "Only member accesses are allowed. " + exp);
            }
        }
        public static bool IsMemberPath(this LambdaExpression lambda)
        {
            Expression currentExpression = null;
            foreach (var member in lambda.Body.GetChain())
            {
                currentExpression = member.Expression;
                if (!(currentExpression is MemberExpression))
                {
                    return false;
                }
            }
            return currentExpression == lambda.Body;
        }
        public static LambdaExpression MemberAccessLambda(Type type, string memberPath) =>
            ReflectionHelper.GetMemberPath(type, memberPath).Lambda();
        public static Expression GetSetter(MemberExpression memberExpression)
        {
            var propertyOrField = memberExpression.Member;
            return ReflectionHelper.CanBeSet(propertyOrField) ?
                        MakeMemberAccess(memberExpression.Expression, propertyOrField) :
                        null;
        }

        public static MethodInfo Method<T>(Expression<Func<T>> expression) => GetExpressionBodyMethod(expression);

        public static MethodInfo Method<TType, TResult>(Expression<Func<TType, TResult>> expression) => GetExpressionBodyMethod(expression);

        private static MethodInfo GetExpressionBodyMethod(LambdaExpression expression) => ((MethodCallExpression)expression.Body).Method;

        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            if (collection.Type.IsArray)
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
                Using(enumeratorVar,
                    Loop(
                        IfThenElse(
                            Equal(moveNextCall, Constant(true)),
                            Block(new[] { loopVar },
                                Assign(loopVar, ToType(Property(enumeratorVar, "Current"), loopVar.Type)),
                                loopContent
                            ),
                            Break(breakLabel)
                        ),
                    breakLabel))
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

        public static Expression ToObject(this Expression expression) => ToType(expression, typeof(object));

        public static Expression ToType(Expression expression, Type type) => expression.Type == type ? expression : Convert(expression, type);

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Length, exp.Parameters.Count); i++)
                replaceExp = Replace(replaceExp, exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Length, exp.Parameters.Count); i++)
                replaceExp = new ConvertingVisitor(exp.Parameters[i], replace[i]).Visit(replaceExp);
            return replaceExp;
        }

        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        public static LambdaExpression Concat(LambdaExpression expr, LambdaExpression concat) => (LambdaExpression)new ExpressionConcatVisitor(expr).Visit(concat);

        public static Expression NullCheck(this Expression expression, Type destinationType = null)
        {
            destinationType ??= expression.Type;
            var chain = expression.GetChain().ToArray();
            if (!(chain.FirstOrDefault().Target is ParameterExpression parameter))
            {
                return expression;
            }
            var variables = new List<ParameterExpression> { parameter };
            var nullConditions = new Stack<Expression>();
            var name = parameter.Name;
            foreach (var member in chain)
            {
                var variable = Variable(member.Target.Type, name);
                name += member.MemberInfo.Name;
                var assignment = Assign(variable, UpdateTarget(member.Target, variables[variables.Count - 1]));
                variables.Add(variable);
                nullConditions.Push(variable.Type.IsValueType ? (Expression) Block(assignment, Constant(false)) : Equal(assignment, Constant(null, variable.Type)));
            }
            var returnType = Nullable.GetUnderlyingType(destinationType) == expression.Type ? destinationType : expression.Type;
            var nullCheck = nullConditions.Aggregate((Expression)Constant(false), (left, right) => OrElse(right, left));
            var nonNullExpression = UpdateTarget(expression, variables[variables.Count - 1]);
            return Block(variables.Skip(1), Condition(nullCheck, Default(returnType), ToType(nonNullExpression, returnType)));
            static Expression UpdateTarget(Expression sourceExpression, Expression newTarget) =>
                sourceExpression switch
                {
                    MethodCallExpression methodCall when methodCall.Method.IsStatic => methodCall.Update(null, new[] { newTarget }.Concat(methodCall.Arguments.Skip(1))),
                    MethodCallExpression methodCall => methodCall.Update(newTarget, methodCall.Arguments),
                    MemberExpression memberExpression => memberExpression.Update(newTarget),
                    _ => sourceExpression,
                };
        }

        static readonly Expression<Action<IDisposable>> DisposeExpression = disposable => disposable.Dispose();

        public static Expression Using(Expression disposable, Expression body)
        {
            Expression disposeCall;
            if (typeof(IDisposable).IsAssignableFrom(disposable.Type))
            {
                disposeCall = DisposeExpression.ReplaceParameters(disposable);
            }
            else
            {
                if (disposable.Type.IsValueType)
                {
                    return body;
                }
                var disposableVariable = Variable(typeof(IDisposable), "disposableVariable");
                var assignDisposable = Assign(disposableVariable, TypeAs(disposable, typeof(IDisposable)));
                disposeCall = Block(new[] { disposableVariable }, assignDisposable, IfNullElse(disposableVariable, Empty(), DisposeExpression.ReplaceParameters(disposableVariable)));
            }
            return TryFinally(body, disposeCall);
        }

        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else = null)
        {
            var nonNullElse = ToType(@else ?? Default(then.Type), then.Type);
            if(expression.Type.IsValueType && !expression.Type.IsNullableType())
            {
                return nonNullElse;
            }
            return Condition(Equal(expression, Constant(null)), then, nonNullElse);
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

            protected override Expression VisitLambda<T>(Expression<T> node) => Expression.Lambda(Visit(node.Body), node.Parameters);
        }
    }
}
