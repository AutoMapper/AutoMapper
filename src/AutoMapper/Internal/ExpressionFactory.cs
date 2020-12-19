using AutoMapper.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper.Internal
{
    using static Expression;
    using static ExpressionBuilder;
    using static ReflectionHelper;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExpressionFactory
    {
        public static readonly MethodInfo ObjectToString = typeof(object).GetMethod("ToString");
        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod("Dispose");
        public static readonly Expression False = Constant(false, typeof(bool));
        public static readonly Expression True = Constant(true, typeof(bool));
        public static readonly Expression Null = Constant(null, typeof(object));
        public static readonly Expression Empty = Empty();
        public static readonly Expression Zero = Constant(0, typeof(int));
        public static readonly ParameterExpression ExceptionParameter = Parameter(typeof(Exception), "ex");
        public static readonly ParameterExpression ContextParameter = Parameter(typeof(ResolutionContext), "context");
        public static bool IsQuery(this Expression expression) => expression is MethodCallExpression { Method: { IsStatic: true } method } && method.DeclaringType == typeof(Enumerable);
        public static Expression Chain(this IEnumerable<Expression> expressions, Expression parameter) => expressions.Aggregate(parameter,
            (left, right) => right is LambdaExpression lambda ? lambda.ReplaceParameters(left) : right.Replace(right.GetChain().FirstOrDefault().Target, left));
        public static LambdaExpression Lambda(this MemberInfo member) => new[] { member }.Lambda();
        public static LambdaExpression Lambda(this MemberInfo[] members)
        {
            var source = Parameter(members[0].DeclaringType, "source");
            return Expression.Lambda(members.Chain(source), source);
        }
        public static Expression Chain(this MemberInfo[] members, Expression target)
        {
            foreach (var member in members)
            {
                target = member switch
                {
                    PropertyInfo property => Expression.Property(target, property),
                    MethodInfo { IsStatic: true } getter => Expression.Call(getter, target),
                    FieldInfo field => Field(target, field),
                    MethodInfo getter => Expression.Call(target, getter),
                    _ => throw new ArgumentOutOfRangeException(nameof(member), member, "Unexpected member.")
                };
            }
            return target;
        }
        public static MemberInfo[] GetMembersChain(this LambdaExpression lambda) => lambda.Body.GetMembersChain();
        public static MemberInfo GetMember(this LambdaExpression lambda) =>
            (lambda?.Body is MemberExpression memberExpression && memberExpression.Expression == lambda.Parameters[0]) ? memberExpression.Member : null;
        public static MemberInfo[] GetMembersChain(this Expression expression) => expression.GetChain().ToMemberInfos();
        public static MemberInfo[] ToMemberInfos(this Stack<Member> chain)
        {
            var members = new MemberInfo[chain.Count];
            int index = 0;
            foreach (var member in chain)
            {
                members[index++] = member.MemberInfo;
            }
            return members;
        }
        public static Stack<Member> GetChain(this Expression expression)
        {
            var stack = new Stack<Member>();
            while (expression != null)
            {
                var member = expression switch
                {
                    MemberExpression{ Expression: Expression target, Member: MemberInfo propertyOrField } => 
                        new Member(expression, propertyOrField, target),
                    MethodCallExpression { Method: var instanceMethod, Object: Expression target } =>
                        new Member(expression, instanceMethod, target),
                    MethodCallExpression { Method: var extensionMethod, Arguments: { Count: >0 } arguments } when extensionMethod.Has<ExtensionAttribute>() => 
                        new Member(expression, extensionMethod, arguments[0]),
                    _ => default
                };
                if (member.Expression == null)
                {
                    break;
                }
                stack.Push(member);
                expression = member.Target;
            }
            return stack;
        }
        public static IEnumerable<MemberExpression> GetMemberExpressions(this Expression expression)
        {
            if (expression is not MemberExpression memberExpression)
            {
                return Array.Empty<MemberExpression>();
            }
            return expression.GetChain().Select(m => m.Expression as MemberExpression).TakeWhile(m => m != null);
        }
        public static bool IsMemberPath(this LambdaExpression lambda, out Stack<Member> members)
        {
            Expression currentExpression = null;
            members = lambda.Body.GetChain();
            foreach (var member in members)
            {
                currentExpression = member.Expression;
                if (!(currentExpression is MemberExpression))
                {
                    return false;
                }
            }
            return currentExpression == lambda.Body;
        }
        public static LambdaExpression MemberAccessLambda(Type type, string memberPath) => GetMemberPath(type, memberPath).Lambda();
        public static Expression ForEach(ParameterExpression loopVar, Expression collection, Expression loopContent)
        {
            if (collection.Type.IsArray)
            {
                return ForEachArrayItem(loopVar, collection, loopContent);
            }
            var getEnumerator = collection.Type.GetInheritedMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(collection, getEnumerator);
            var enumeratorType = getEnumeratorCall.Type;
            var enumeratorVar = Variable(enumeratorType, "enumerator");
            var enumeratorAssign = Assign(enumeratorVar, getEnumeratorCall);
            var moveNext = enumeratorType.GetInheritedMethod("MoveNext");
            var moveNextCall = Expression.Call(enumeratorVar, moveNext);
            var breakLabel = Label("LoopBreak");
            var loop = Block(new[] { enumeratorVar, loopVar },
                enumeratorAssign,
                Using(enumeratorVar,
                    Loop(
                        IfThenElse(
                            moveNextCall,
                            Block(Assign(loopVar, ToType(Property(enumeratorVar, "Current"), loopVar.Type)), loopContent),
                            Break(breakLabel)
                        ),
                    breakLabel)));
            return loop;
        }
        public static Expression ForEachArrayItem(ParameterExpression loopVar, Expression array, Expression loopContent)
        {
            var breakLabel = Label("LoopBreak");
            var index = Variable(typeof(int), "sourceArrayIndex");
            var initialize = Assign(index, Constant(0, typeof(int)));
            var loop = Block(new[] { index, loopVar },
                initialize,
                Loop(
                    IfThenElse(
                        LessThan(index, ArrayLength(array)),
                        Block(Assign(loopVar, ArrayAccess(array, index)), loopContent, PostIncrementAssign(index)),
                        Break(breakLabel)
                    ),
                breakLabel));
            return loop;
        }
        // Expression.Property(string) is inefficient because it does a case insensitive match
        public static MemberExpression Property(Expression target, string name) => Expression.Property(target, target.Type.GetProperty(name));
        // Expression.Call(string) is inefficient because it does a case insensitive match
        public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            var method = FindMethod(type, methodName, arguments);
            if (typeArguments != null)
            {
                method = method.MakeGenericMethod(typeArguments);
            }
            return Expression.Call(method, arguments);
            static MethodInfo FindMethod(Type type, string methodName, Expression[] arguments)
            {
                var methods = type.GetMember(methodName, MemberTypes.Method, TypeExtensions.StaticFlags & ~BindingFlags.NonPublic);
                if (methods.Length == 1)
                {
                    return (MethodInfo)methods[0];
                }
                foreach (MethodInfo foundMethod in methods)
                {
                    var parameters = foundMethod.GetParameters();
                    if (parameters.Length != arguments.Length)
                    {
                        continue;
                    }
                    if (foundMethod.IsGenericMethodDefinition)
                    {
                        return foundMethod;
                    }
                    int index = 0;
                    bool match = true;
                    foreach (var argument in arguments)
                    {
                        if (!parameters[index++].ParameterType.IsAssignableFrom(argument.Type))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return foundMethod;
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Cannot find suitable method {type}.{methodName}({string.Join(", ", arguments.Select(a=>a.Type))} parameters).");
            }
        }
        public static Expression ToObject(this Expression expression) => ToType(expression, typeof(object));
        public static Expression ToType(Expression expression, Type type) => expression.Type == type ? expression : Convert(expression, type);
        public static Expression ReplaceParameters(this LambdaExpression initialLambda, params Expression[] newParameters) =>
            new ParameterReplaceVisitor().Replace(initialLambda, newParameters);
        public static Expression ConvertReplaceParameters(this LambdaExpression initialLambda, params Expression[] newParameters) =>
            new ConvertParameterReplaceVisitor().Replace(initialLambda, newParameters);
        private static Expression Replace(this ParameterReplaceVisitor visitor, LambdaExpression initialLambda, params Expression[] newParameters)
        {
            var newLambda = initialLambda.Body;
            for (var i = 0; i < Math.Min(newParameters.Length, initialLambda.Parameters.Count); i++)
            {
                visitor.Replace(initialLambda.Parameters[i], newParameters[i]);
                newLambda = visitor.Visit(newLambda);
            }
            return newLambda;
        }
        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceVisitor(old, replace).Visit(exp);
        public static Expression NullCheck(this Expression expression, Type destinationType = null)
        {
            var chain = expression.GetChain();
            if (chain.Count == 0 || chain.Peek().Target is not ParameterExpression parameter)
            {
                return expression;
            }
            var variables = new ParameterExpression[chain.Count];
            var nullCheck = False;
            var name = parameter.Name;
            int index = 0;
            foreach (var member in chain)
            {
                var variable = Variable(member.Target.Type, name);
                name += member.MemberInfo.Name;
                var target = index == 0 ? parameter : variables[index - 1];
                var assignment = Assign(variable, UpdateTarget(member.Target, target));
                variables[index++] = variable;
                var nullCheckVariable = variable.Type.IsValueType ? (Expression)Block(assignment, False) : ReferenceEqual(assignment, Null);
                nullCheck = OrElse(nullCheck, nullCheckVariable);
            }
            var nonNullExpression = UpdateTarget(expression, variables[variables.Length - 1]);
            var returnType = (destinationType != null && Nullable.GetUnderlyingType(destinationType) == expression.Type) ? destinationType : expression.Type;
            return Block(variables, Condition(nullCheck, Default(returnType), ToType(nonNullExpression, returnType)));
            static Expression UpdateTarget(Expression sourceExpression, Expression newTarget) =>
                sourceExpression switch
                {
                    MemberExpression memberExpression => memberExpression.Update(newTarget),
                    MethodCallExpression { Method: { IsStatic: true } } methodCall => methodCall.Update(null, new[] { newTarget }.Concat(methodCall.Arguments.Skip(1))),
                    MethodCallExpression methodCall => methodCall.Update(newTarget, methodCall.Arguments),
                    _ => sourceExpression,
                };
        }
        public static Expression Using(Expression disposable, Expression body)
        {
            Expression disposeCall;
            if (typeof(IDisposable).IsAssignableFrom(disposable.Type))
            {
                disposeCall = Expression.Call(disposable, DisposeMethod);
            }
            else
            {
                if (disposable.Type.IsValueType)
                {
                    return body;
                }
                var disposableVariable = Variable(typeof(IDisposable), "disposableVariable");
                var assignDisposable = Assign(disposableVariable, TypeAs(disposable, typeof(IDisposable)));
                disposeCall = Block(new[] { disposableVariable }, assignDisposable, IfNullElse(disposableVariable, Empty, Expression.Call(disposableVariable, DisposeMethod)));
            }
            return TryFinally(body, disposeCall);
        }
        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else)
        {
            if (expression.Type.IsValueType)
            {
                return expression.Type.IsNullableType() ? Condition(Property(expression, "HasValue"), ToType(@else, then.Type), then) : @else;
            }
            else
            {
                return Condition(ReferenceEqual(expression, Null), then, ToType(@else, then.Type));
            }
        }
        class ReplaceVisitorBase : ExpressionVisitor
        {
            protected Expression _oldNode;
            protected Expression _newNode;
            public virtual void Replace(Expression oldNode, Expression newNode)
            {
                _oldNode = oldNode;
                _newNode = newNode;
            }
        }
        class ReplaceVisitor : ReplaceVisitorBase
        {
            public ReplaceVisitor(Expression oldNode, Expression newNode) => Replace(oldNode, newNode);
            public override Expression Visit(Expression node) => _oldNode == node ? _newNode : base.Visit(node);
        }
        class ParameterReplaceVisitor : ReplaceVisitorBase
        {
            protected override Expression VisitParameter(ParameterExpression node) => _oldNode == node ? _newNode : base.VisitParameter(node);
        }
        class ConvertParameterReplaceVisitor : ParameterReplaceVisitor
        {
            public override void Replace(Expression oldNode, Expression newNode) => base.Replace(oldNode, ToType(newNode, oldNode.Type));
        }
        public static Expression MapCollectionExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            MethodInfo addMethod;
            bool isIList;
            Type destinationCollectionType, destinationElementType;
            GetDestinationType();
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = sourceExpression.Type.GetICollectionType()?.GenericTypeArguments[0] ?? GetEnumerableElementType(sourceExpression.Type);
            var itemParam = Parameter(sourceElementType, "item");
            var itemExpr = MapExpression(configurationProvider, profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
            Expression destination, assignNewExpression;
            UseDestinationValue();
            var addItems = ForEach(itemParam, sourceExpression, Expression.Call(destination, addMethod, itemExpr));
            var overMaxDepth = OverMaxDepth(memberMap?.TypeMap);
            if (overMaxDepth != null)
            {
                addItems = Condition(overMaxDepth, Empty, addItems);
            }
            var clearMethod = isIList ? IListClear : destinationCollectionType.GetMethod("Clear");
            var checkNull = Block(new[] { newExpression, passedDestination },
                    Assign(passedDestination, destExpression),
                    assignNewExpression,
                    Expression.Call(destination, clearMethod),
                    addItems,
                    destination);
            if (memberMap != null)
            {
                return checkNull;
            }
            return CheckContext();
            void GetDestinationType()
            {
                destinationCollectionType = destExpression.Type.GetICollectionType();
                destinationElementType = destinationCollectionType?.GenericTypeArguments[0] ?? GetEnumerableElementType(destExpression.Type);
                if (destinationCollectionType == null && destExpression.Type.IsInterface)
                {
                    destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                    destExpression = ToType(destExpression, destinationCollectionType);
                }
                if (destinationCollectionType == null)
                {
                    destinationCollectionType = typeof(IList);
                    addMethod = IListAdd;
                    isIList = true;
                }
                else
                {
                    isIList = destExpression.Type.IsListType();
                    addMethod = destinationCollectionType.GetMethod("Add");
                }
            }
            void UseDestinationValue()
            {
                if (memberMap is { UseDestinationValue: true })
                {
                    destination = passedDestination;
                    assignNewExpression = Empty;
                }
                else
                {
                    destination = newExpression;
                    var createInstance = ObjectFactory.GenerateConstructorExpression(passedDestination.Type);
                    var shouldCreateDestination = ReferenceEqual(passedDestination, Null);
                    if (memberMap is { CanBeSet: true })
                    {
                        var isReadOnly = isIList ? Expression.Property(passedDestination, IListIsReadOnly) : Property(ToType(passedDestination, destinationCollectionType), "IsReadOnly");
                        shouldCreateDestination = OrElse(shouldCreateDestination, isReadOnly);
                    }
                    assignNewExpression = Assign(newExpression, Condition(shouldCreateDestination, ToType(createInstance, passedDestination.Type), passedDestination));
                }
            }
            Expression CheckContext()
            {
                var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
                if (elementTypeMap == null)
                {
                    return checkNull;
                }
                var checkContext = ExpressionBuilder.CheckContext(elementTypeMap);
                if (checkContext == null)
                {
                    return checkNull;
                }
                return Block(checkContext, checkNull);
            }
        }
        public static Expression MapReadOnlyCollection(Type genericCollectionType, Type genericReadOnlyCollectionType, IGlobalConfiguration configurationProvider,
            ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationTypeArguments = destExpression.Type.GenericTypeArguments;
            var closedCollectionType = genericCollectionType.MakeGenericType(destinationTypeArguments);
            var dict = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(closedCollectionType));
            var readOnlyClosedType = destExpression.Type.IsInterface ? genericReadOnlyCollectionType.MakeGenericType(destinationTypeArguments) : destExpression.Type;
            return New(readOnlyClosedType.GetConstructors()[0], dict);
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
        public readonly Expression Expression;
        public readonly MemberInfo MemberInfo;
        public readonly Expression Target;
    }
}