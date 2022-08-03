using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
namespace AutoMapper.Execution
{
    using Internal;
    using static Expression;
    using static Internal.ReflectionHelper;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExpressionBuilder
    {
        public static readonly MethodInfo ObjectToString = typeof(object).GetMethod(nameof(object.ToString));
        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        public static readonly Expression True = Constant(true, typeof(bool));
        public static readonly Expression Null = Constant(null, typeof(object));
        public static readonly Expression Empty = Empty();
        public static readonly Expression Zero = Constant(0, typeof(int));
        public static readonly ParameterExpression ExceptionParameter = Parameter(typeof(Exception), "ex");
        public static readonly ParameterExpression ContextParameter = Parameter(typeof(ResolutionContext), "context");
        public static readonly MethodInfo IListClear = typeof(IList).GetMethod(nameof(IList.Clear));
        public static readonly MethodInfo IListAdd = typeof(IList).GetMethod(nameof(IList.Add));
        public static readonly MethodInfo IncTypeDepthInfo = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.IncrementTypeDepth));
        public static readonly MethodInfo DecTypeDepthInfo = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.DecrementTypeDepth));
        private static readonly MethodInfo ContextCreate = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.CreateInstance));
        public static readonly MethodInfo OverTypeDepthMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.OverTypeDepth));
        public static readonly MethodInfo CacheDestinationMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.CacheDestination));
        public static readonly MethodInfo GetDestinationMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.GetDestination));
        private static readonly MethodCallExpression CheckContextCall = Expression.Call(
            typeof(ResolutionContext).GetStaticMethod(nameof(ResolutionContext.CheckContext)), ContextParameter);
        private static readonly MethodInfo ContextMapMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.MapInternal));
        private static readonly MethodInfo ArrayEmptyMethod = typeof(Array).GetStaticMethod(nameof(Array.Empty));
        private static readonly ParameterExpression Disposable = Variable(typeof(IDisposable), "disposableEnumerator");
        private static readonly ParameterExpression[] DisposableArray = new[] { Disposable };
        private static readonly Expression DisposeCall = IfNullElse(Disposable, Empty, Expression.Call(Disposable, DisposeMethod));
        private static readonly ParameterExpression Index = Variable(typeof(int), "sourceArrayIndex");
        private static readonly BinaryExpression ResetIndex = Assign(Index, Zero);
        private static readonly UnaryExpression IncrementIndex = PostIncrementAssign(Index);

        public static Expression MapExpression(this IGlobalConfiguration configurationProvider, ProfileMap profileMap, TypePair typePair, Expression source,
            MemberMap propertyMap = null, Expression destination = null)
        {
            destination ??= Default(typePair.DestinationType);
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            Expression mapExpression = null;
            bool hasTypeConverter;
            if (typeMap != null)
            {
                hasTypeConverter = typeMap.HasTypeConverter;
                if (!typeMap.HasDerivedTypesToInclude)
                {
                    configurationProvider.Seal(typeMap);
                    if (typeMap.MapExpression != null)
                    {
                        mapExpression = typeMap.Invoke(source, destination);
                    }
                }
            }
            else
            {
                hasTypeConverter = false;
                var mapper = configurationProvider.FindMapper(typePair);
                mapExpression = mapper?.MapExpression(configurationProvider, profileMap, propertyMap, source, destination);
            }
            mapExpression ??= ContextMap(typePair, source, destination, propertyMap);
            if (!hasTypeConverter)
            {
                mapExpression = NullCheckSource(profileMap, source, destination, mapExpression, propertyMap);
            }
            return ToType(mapExpression, typePair.DestinationType);
        }
        public static Expression NullCheckSource(ProfileMap profileMap, Expression sourceParameter, Expression destinationParameter, Expression mapExpression, MemberMap memberMap)
        {
            var sourceType = sourceParameter.Type;
            if (sourceType.IsValueType && !sourceType.IsNullableType())
            {
                return mapExpression;
            }
            var destinationType = destinationParameter.Type;
            var isCollection = destinationType.IsCollection();
            var mustUseDestination = memberMap is { MustUseDestination: true };
            var ifSourceNull = mustUseDestination ? ClearDestinationCollection() : DefaultDestination();
            return sourceParameter.IfNullElse(ifSourceNull, mapExpression);
            Expression ClearDestinationCollection()
            {
                if (!isCollection)
                {
                    return destinationParameter;
                }
                MethodInfo clearMethod;
                var destinationVariable = Variable(destinationParameter.Type, "collectionDestination");
                Expression collection = destinationVariable;
                if (destinationType.IsListType())
                {
                    clearMethod = IListClear;
                }
                else
                {
                    var destinationCollectionType = destinationType.GetICollectionType();
                    if (destinationCollectionType == null)
                    {
                        if (!mustUseDestination)
                        {
                            return destinationParameter;
                        }
                        var destinationElementType = GetEnumerableElementType(destinationType);
                        destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                        collection = TypeAs(collection, destinationCollectionType);
                    }
                    clearMethod = destinationCollectionType.GetMethod("Clear");
                }
                return Block(new[] { destinationVariable },
                    Assign(destinationVariable, destinationParameter),
                    Condition(ReferenceEqual(collection, Null), Empty, Expression.Call(collection, clearMethod)),
                    destinationVariable);
            }
            Expression DefaultDestination()
            {
                if ((isCollection && profileMap.AllowsNullCollectionsFor(memberMap)) || (!isCollection && profileMap.AllowsNullDestinationValuesFor(memberMap)))
                {
                    return destinationParameter.NodeType == ExpressionType.Default ? destinationParameter : Default(destinationType);
                }
                if (destinationType.IsArray)
                {
                    var destinationElementType = destinationType.GetElementType();
                    var rank = destinationType.GetArrayRank();
                    return rank == 1 ? 
                        Expression.Call(ArrayEmptyMethod.MakeGenericMethod(destinationElementType)) : 
                        NewArrayBounds(destinationElementType, Enumerable.Repeat(Zero, rank));
                }
                return ObjectFactory.GenerateConstructorExpression(destinationType);
            }
        }
        public static Expression ServiceLocator(Type type) => Expression.Call(ContextParameter, ContextCreate, Constant(type));
        public static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression destinationParameter, MemberMap memberMap)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Expression.Call(ContextParameter, mapMethod, sourceParameter, destinationParameter, Constant(memberMap, typeof(MemberMap)));
        }
        public static Expression CheckContext(TypeMap typeMap)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                return CheckContextCall;
            }
            return null;
        }
        public static Expression OverMaxDepth(TypeMap typeMap) => typeMap?.MaxDepth > 0 ? Expression.Call(ContextParameter, OverTypeDepthMethod, Constant(typeMap)) : null;
        public static Expression NullSubstitute(this MemberMap memberMap, Expression sourceExpression) =>
            Coalesce(sourceExpression, ToType(Constant(memberMap.NullSubstitute), sourceExpression.Type));
        public static Expression ApplyTransformers(this MemberMap memberMap, Expression source)
        {
            var perMember = memberMap.ValueTransformers;
            var perMap = memberMap.TypeMap.ValueTransformers;
            var perProfile = memberMap.TypeMap.Profile.ValueTransformers;
            if (perMember.Count == 0 && perMap.Count == 0 && perProfile.Count == 0)
            {
                return source;
            }
            var transformers = perMember.Concat(perMap).Concat(perProfile);
            return Apply(transformers, memberMap, source);
            static Expression Apply(IEnumerable<ValueTransformerConfiguration> transformers, MemberMap memberMap, Expression source) => 
                transformers.Where(vt => vt.IsMatch(memberMap)).Aggregate(source,
                    (current, vtConfig) => ToType(vtConfig.TransformerExpression.ReplaceParameters(ToType(current, vtConfig.ValueType)), memberMap.DestinationType));
        }
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
        public static MemberInfo[] GetMembersChain(this LambdaExpression lambda) => lambda.Body.GetChain().ToMemberInfos();
        public static MemberInfo GetMember(this LambdaExpression lambda) =>
            (lambda?.Body is MemberExpression memberExpression && memberExpression.Expression == lambda.Parameters[0]) ? memberExpression.Member : null;
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
                    MemberExpression { Expression: Expression target, Member: MemberInfo propertyOrField } =>
                        new Member(expression, propertyOrField, target),
                    MethodCallExpression { Method: var instanceMethod, Object: Expression target } =>
                        new Member(expression, instanceMethod, target),
                    MethodCallExpression { Method: var extensionMethod, Arguments: { Count: > 0 } arguments } when extensionMethod.Has<ExtensionAttribute>() =>
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
                if (currentExpression is not MemberExpression)
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
            var getEnumeratorCall = Call(collection, "GetEnumerator");
            var enumeratorVar = Variable(getEnumeratorCall.Type, "enumerator");
            var breakLabel = Label("LoopBreak");
            var loop = Block(new[] { enumeratorVar, loopVar },
                Assign(enumeratorVar, getEnumeratorCall),
                Using(enumeratorVar,
                    Loop(
                        IfThenElse(
                            Call(enumeratorVar, "MoveNext"),
                            Block(Assign(loopVar, ToType(Property(enumeratorVar, "Current"), loopVar.Type)), loopContent),
                            Break(breakLabel)
                        ),
                    breakLabel)));
            return loop;
            static Expression ForEachArrayItem(ParameterExpression loopVar, Expression array, Expression loopContent)
            {
                var breakLabel = Label("LoopBreak");
                var loop = Block(new[] { Index, loopVar },
                    ResetIndex,
                    Loop(
                        IfThenElse(
                            LessThan(Index, ArrayLength(array)),
                            Block(Assign(loopVar, ArrayAccess(array, Index)), loopContent, IncrementIndex),
                            Break(breakLabel)
                        ),
                    breakLabel));
                return loop;
            }
            static Expression Using(Expression target, Expression body)
            {
                Expression finallyDispose;
                if (typeof(IDisposable).IsAssignableFrom(target.Type))
                {
                    finallyDispose = Expression.Call(target, DisposeMethod);
                }
                else
                {
                    if (target.Type.IsValueType)
                    {
                        return body;
                    }
                    var assignDisposable = Assign(Disposable, TypeAs(target, typeof(IDisposable)));
                    finallyDispose = Block(DisposableArray, assignDisposable, DisposeCall);
                }
                return TryFinally(body, finallyDispose);
            }
        }
        // Expression.Property(string) is inefficient because it does a case insensitive match
        public static MemberExpression Property(Expression target, string name) =>
            Expression.Property(target, target.Type.GetInheritedProperty(name));
        // Expression.Call(string) is inefficient because it does a case insensitive match
        public static MethodCallExpression Call(Expression target, string name, params Expression[] arguments) =>
            Expression.Call(target, target.Type.GetInheritedMethod(name), arguments);
        public static Expression ToObject(this Expression expression) => expression.Type.IsValueType ? Convert(expression, typeof(object)) : expression;
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
        public static Expression NullCheck(this Expression expression, Type destinationType = null, Expression defaultValue = null)
        {
            var chain = expression.GetChain();
            if (chain.Count == 0 || chain.Peek().Target is not ParameterExpression parameter)
            {
                return expression;
            }
            var returnType = (destinationType != null && destinationType != expression.Type && Nullable.GetUnderlyingType(destinationType) == expression.Type) ?
                destinationType : expression.Type;
            var defaultReturn = (defaultValue is { NodeType: ExpressionType.Default } && defaultValue.Type == returnType) ? defaultValue : Default(returnType);
            ParameterExpression[] variables = null;
            var name = parameter.Name;
            int index = 0;
            var nullCheckedExpression = NullCheck(parameter);
            return variables == null ? nullCheckedExpression : Block(variables, nullCheckedExpression);
            Expression NullCheck(ParameterExpression variable)
            {
                var member = chain.Pop();
                if (chain.Count == 0)
                {
                    return variable.IfNullElse(defaultReturn, UpdateTarget(expression, variable));
                }
                variables ??= new ParameterExpression[chain.Count];
                name += member.MemberInfo.Name;
                var newVariable = Variable(member.Expression.Type, name);
                variables[index++] = newVariable;
                var assignment = Assign(newVariable, UpdateTarget(member.Expression, variable));
                return variable.IfNullElse(defaultReturn, Block(assignment, NullCheck(newVariable)));
            }
            static Expression UpdateTarget(Expression sourceExpression, Expression newTarget) => sourceExpression switch
            {
                MemberExpression memberExpression => memberExpression.Update(newTarget),
                MethodCallExpression { Object: null, Arguments: var args } methodCall when args[0] != newTarget =>
                    ExtensionMethod(methodCall.Method, newTarget, args),
                MethodCallExpression { Object: Expression target } methodCall when target != newTarget => 
                    Expression.Call(newTarget, methodCall.Method, methodCall.Arguments),
                _ => sourceExpression,
            };
            static MethodCallExpression ExtensionMethod(MethodInfo method, Expression newTarget, ReadOnlyCollection<Expression> args)
            {
                var newArgs = args.ToArray();
                newArgs[0] = newTarget;
                return Expression.Call(method, newArgs);
            }
        }
        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else) => expression.Type.IsValueType ?
            (expression.Type.IsNullableType() ? Condition(Property(expression, "HasValue"), ToType(@else, then.Type), then) : @else) :
            Condition(ReferenceEqual(expression, Null), then, ToType(@else, then.Type));
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