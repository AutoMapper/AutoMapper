﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Configuration;
using AutoMapper.Execution;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.QueryableExtensions
{
    using static ExpressionFactory;
    using ParameterBag = IDictionary<string, object>;
    using TypePairCount = IDictionary<ExpressionRequest, int>;

    public interface IExpressionBuilder
    {
        LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, object parameters, MemberInfo[] membersToExpand);
        LambdaExpression[] CreateMapExpression(ExpressionRequest request, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps);
        Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps);
    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        internal static List<IExpressionResultConverter> DefaultResultConverters() =>
            new List<IExpressionResultConverter>
            {
                new MemberResolverExpressionResultConverter(),
                new MemberGetterExpressionResultConverter()
            };

        internal static List<IExpressionBinder> DefaultBinders() =>
            new List<IExpressionBinder>
            {
                new CustomProjectionExpressionBinder(),
                new NullableDestinationExpressionBinder(),
                new NullableSourceExpressionBinder(),
                new AssignableExpressionBinder(),
                new EnumerableExpressionBinder(),
                new MappedTypeExpressionBinder(),
                new StringExpressionBinder()
            };

        private readonly LockingConcurrentDictionary<ExpressionRequest, LambdaExpression[]> _expressionCache;
        private readonly IConfigurationProvider _configurationProvider;

        public ExpressionBuilder(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            _expressionCache = new LockingConcurrentDictionary<ExpressionRequest, LambdaExpression[]>(CreateMapExpression);
        }

        public LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, object parameters,
            MemberInfo[] membersToExpand)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }
            if (membersToExpand == null)
            {
                throw new ArgumentNullException(nameof(membersToExpand));
            }

            var cachedExpressions = _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand, null));

            return cachedExpressions.Select(e => Prepare(e, parameters)).Cast<LambdaExpression>().ToArray();
        }

        private Expression Prepare(Expression cachedExpression, object parameters)
        {
            Expression result;
            if(parameters != null)
            {
                var visitor = ParameterExpressionVisitor.Create(parameters);
                result = visitor.Visit(cachedExpression);
            }
            else
            {
                result = cachedExpression;
            }
            // perform null-propagation if this feature is enabled.
            if(_configurationProvider.EnableNullPropagationForQueryMapping)
            {
                var nullVisitor = new NullsafeQueryRewriter();
                return nullVisitor.Visit(result);
            }
            return result;
        }

        private LambdaExpression[] CreateMapExpression(ExpressionRequest request) => CreateMapExpression(request, new Dictionary<ExpressionRequest, int>(),
            new FirstPassLetPropertyMaps(_configurationProvider)
            );

        public LambdaExpression[] CreateMapExpression(ExpressionRequest request, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Parameter(request.SourceType, "dto"+ request.SourceType.Name);
            var expressions = new QueryExpressions(CreateMapExpressionCore(request, instanceParameter, typePairCount, letPropertyMaps, out var typeMap));
            if(letPropertyMaps.Count > 0)
            {
                expressions = letPropertyMaps.GetSubQueryExpression(this, expressions.First, typeMap, request, instanceParameter, typePairCount);
            }
            if(expressions.First == null)
            {
                return null;
            }
            var firstLambda = Lambda(expressions.First, instanceParameter);
            if(expressions.Second == null)
            {
                return new[] { firstLambda };
            }
            return new[] { firstLambda, Lambda(expressions.Second, expressions.SecondParameter) };
        }

        public Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            return CreateMapExpressionCore(request, instanceParameter, typePairCount, letPropertyMaps, out var _);
        }

        private Expression CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps, out TypeMap typeMap)
        {
            typeMap = _configurationProvider.ResolveTypeMap(request.SourceType, request.DestinationType);

            if(typeMap == null)
            {
                throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            }

            if(typeMap.CustomMapExpression != null)
            {
                return typeMap.CustomMapExpression.ReplaceParameters(instanceParameter);
            }
            return CreateMapExpressionCore(request, instanceParameter, typePairCount, typeMap, letPropertyMaps);
        }

        private Expression CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, TypeMap typeMap, LetPropertyMaps letPropertyMaps)
        {
            var bindings = new List<MemberBinding>();
            var depth = GetDepth(request, typePairCount);
            if(typeMap.MaxDepth > 0 && depth >= typeMap.MaxDepth)
            {
                if(typeMap.Profile.AllowNullDestinationValues)
                {
                    return null;
                }
            }
            else
            {
                bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount, letPropertyMaps);
            }
            Expression constructorExpression = DestinationConstructorExpression(typeMap, instanceParameter);
            if(instanceParameter is ParameterExpression)
                constructorExpression = ((LambdaExpression)constructorExpression).ReplaceParameters(instanceParameter);
            var visitor = new NewFinderVisitor();
            visitor.Visit(constructorExpression);

            var expression = MemberInit(
                visitor.NewExpression,
                bindings.ToArray()
                );
            return expression;
        }

        private static int GetDepth(ExpressionRequest request, TypePairCount typePairCount)
        {
            if (typePairCount.TryGetValue(request, out int visitCount))
            {
                visitCount = visitCount + 1;
            }
            typePairCount[request] = visitCount;
            return visitCount;
        }

        private LambdaExpression DestinationConstructorExpression(TypeMap typeMap, Expression instanceParameter)
        {
            var ctorExpr = typeMap.CustomCtorExpression;
            if (ctorExpr != null)
            {
                return ctorExpr;
            }
            var newExpression = typeMap.ConstructorMap?.CanResolve == true
                ? typeMap.ConstructorMap.NewExpression(instanceParameter)
                : New(typeMap.DestinationTypeToUse);

            return Lambda(newExpression);
        }

        private class NewFinderVisitor : ExpressionVisitor
        {
            public NewExpression NewExpression { get; private set; }

            protected override Expression VisitNew(NewExpression node)
            {
                NewExpression = node;
                return base.VisitNew(node);
            }
        }

        private List<MemberBinding> CreateMemberBindings(ExpressionRequest request, TypeMap typeMap, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var bindings = new List<MemberBinding>();
            foreach (var propertyMap in typeMap.PropertyMaps
                                               .Where(pm => (!pm.ExplicitExpansion || request.MembersToExpand.Contains(pm.DestinationMember)) && 
                                                            pm.CanResolveValue && ReflectionHelper.CanBeSet(pm.DestinationMember))
                                               .OrderBy(pm => pm.DestinationName))
            {
                var propertyExpression = new PropertyExpression(propertyMap);
                letPropertyMaps.Push(propertyExpression);

                CreateMemberBinding(propertyExpression);

                letPropertyMaps.Pop();
            }
            return bindings;
            void CreateMemberBinding(PropertyExpression propertyExpression)
            {
                var propertyMap = propertyExpression.PropertyMap;
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter, letPropertyMaps);
                propertyExpression.Expression = result.ResolutionExpression;
                var propertyTypeMap = _configurationProvider.ResolveTypeMap(result.Type, propertyMap.DestinationType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationType, request.MembersToExpand, request);
                if(propertyRequest.AlreadyExists)
                {
                    return;
                }
                var binder = _configurationProvider.Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));
                if(binder == null)
                {
                    var message =
                        $"Unable to create a map expression from {propertyMap.SourceMember?.DeclaringType?.Name}.{propertyMap.SourceMember?.Name} ({result.Type}) to {propertyMap.DestinationMember.DeclaringType?.Name}.{propertyMap.DestinationName} ({propertyMap.DestinationType})";
                    throw new AutoMapperMappingException(message, null, typeMap.Types, typeMap, propertyMap);
                }
                var bindExpression = binder.Build(_configurationProvider, propertyMap, propertyTypeMap, propertyRequest, result, typePairCount, letPropertyMaps);
                if(bindExpression == null)
                {
                    return;
                }
                var rhs = propertyMap.ValueTransformers
                    .Concat(typeMap.ValueTransformers)
                    .Concat(typeMap.Profile.ValueTransformers)
                    .Where(vt => vt.IsMatch(propertyMap))
                    .Aggregate(bindExpression.Expression, (current, vtConfig) => ToType(ReplaceParameters(vtConfig.TransformerExpression, ToType(current, vtConfig.ValueType)), propertyMap.DestinationType));

                bindExpression = bindExpression.Update(rhs);

                bindings.Add(bindExpression);
            }
        }

        private ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType, Expression instanceParameter, LetPropertyMaps letPropertyMaps)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);

            var matchingExpressionConverter = _configurationProvider.ResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, propertyMap));
            result = matchingExpressionConverter?.GetExpressionResolutionResult(result, propertyMap, letPropertyMaps) 
                ?? throw new Exception("Can't resolve this to Queryable Expression");

            if(propertyMap.NullSubstitute != null && result.Type.IsNullableType())
            {
                var currentChild = result.ResolutionExpression;
                var currentChildType = result.Type;
                var nullSubstitute = propertyMap.NullSubstitute;

                var newParameter = result.ResolutionExpression;
                var converter = new NullSubstitutionConversionVisitor(newParameter, nullSubstitute);

                currentChild = converter.Visit(currentChild);
                currentChildType = currentChildType.GetTypeOfNullable();

                return new ExpressionResolutionResult(currentChild, currentChildType);
            }

            return result;
        }

        private class NullSubstitutionConversionVisitor : ExpressionVisitor
        {
            private readonly Expression _newParameter;
            private readonly object _nullSubstitute;

            public NullSubstitutionConversionVisitor(Expression newParameter, object nullSubstitute)
            {
                _newParameter = newParameter;
                _nullSubstitute = nullSubstitute;
            }

            protected override Expression VisitMember(MemberExpression node) => node == _newParameter ? NullCheck(node) : node;

            private Expression NullCheck(Expression input)
            {
                var underlyingType = input.Type.GetTypeOfNullable();
                var nullSubstitute = ToType(Constant(_nullSubstitute), underlyingType);
                return Condition(Property(input, "HasValue"), Property(input, "Value"), nullSubstitute, underlyingType);
            }
        }

        private abstract class ParameterExpressionVisitor : ExpressionVisitor
        {
            public static ParameterExpressionVisitor Create(object parameters) =>
                parameters is ParameterBag dictionary ? (ParameterExpressionVisitor) new ConstantExpressionReplacementVisitor(dictionary) : new ObjectParameterExpressionReplacementVisitor(parameters);

            protected abstract Expression GetValue(string name);

            protected override Expression VisitMember(MemberExpression node)
            {
                if(!node.Member.DeclaringType.Has<CompilerGeneratedAttribute>())
                {
                    return base.VisitMember(node);
                }
                var parameterName = node.Member.Name;
                var parameterValue = GetValue(parameterName);
                if(parameterValue == null)
                {
                    const string vbPrefix = "$VB$Local_";
                    if(!parameterName.StartsWith(vbPrefix, StringComparison.Ordinal) || (parameterValue = GetValue(parameterName.Substring(vbPrefix.Length))) == null)
                    {
                        return base.VisitMember(node);
                    }
                }
                return ToType(parameterValue, node.Member.GetMemberType());
            }

            private class ObjectParameterExpressionReplacementVisitor : ParameterExpressionVisitor
            {
                private readonly object _parameters;

                public ObjectParameterExpressionReplacementVisitor(object parameters) => _parameters = parameters;

                protected override Expression GetValue(string name)
                {
                    var matchingMember = _parameters.GetType().GetDeclaredProperty(name);
                    return matchingMember != null ? Property(Constant(_parameters), matchingMember) : null;
                }
            }
            private class ConstantExpressionReplacementVisitor : ParameterExpressionVisitor
            {
                private readonly ParameterBag _paramValues;

                public ConstantExpressionReplacementVisitor(ParameterBag paramValues) => _paramValues = paramValues;

                protected override Expression GetValue(string name) =>
                    _paramValues.TryGetValue(name, out object parameterValue) ? Constant(parameterValue) : null;
            }
        }

        public class FirstPassLetPropertyMaps : LetPropertyMaps
        {
            Stack<PropertyExpression> _currentPath = new Stack<PropertyExpression>();
            List<PropertyPath> _savedPaths = new List<PropertyPath>();
            IConfigurationProvider _configurationProvider;

            public FirstPassLetPropertyMaps(IConfigurationProvider configurationProvider) =>
                _configurationProvider = configurationProvider;

            public override Expression GetSubQueryMarker()
            {
                var propertyMap = _currentPath.Peek().PropertyMap;
                var mapFrom = propertyMap.CustomMapExpression;
                if(!IsSubQuery() || _configurationProvider.ResolveTypeMap(propertyMap.SourceType, propertyMap.DestinationType) == null)
                {
                    return null;
                }
                var type = mapFrom.Body.Type;
                var marker = Constant(null, type);
                _savedPaths.Add(new PropertyPath(_currentPath.Reverse().ToArray(), marker));
                return marker;
                bool IsSubQuery()
                {
                    if(!(mapFrom.Body is MethodCallExpression methodCall))
                    {
                        return false;
                    }
                    var method = methodCall.Method;
                    return method.IsStatic && method.DeclaringType == typeof(Enumerable);
                }
            }

            public override void Push(PropertyExpression propertyExpression) => _currentPath.Push(propertyExpression);

            public override void Pop() => _currentPath.Pop();

            public override int Count => _savedPaths.Count;

            public override LetPropertyMaps New() => new FirstPassLetPropertyMaps(_configurationProvider);

            public override QueryExpressions GetSubQueryExpression(ExpressionBuilder builder, Expression projection, TypeMap typeMap, ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount)
            {
                var letMapInfos = _savedPaths.Select(path =>
                new {
                    MapFrom = path.Last.CustomMapExpression,
                    MapFromSource = path.GetSourceExpression(instanceParameter),
                    Property = path.GetPropertyDescription(),
                    path.Marker
                }).ToArray();

                var properties = letMapInfos.Select(m => m.Property).Concat(GetMemberAccessesVisitor.Retrieve(projection, instanceParameter));

                var letType = ProxyGenerator.GetSimilarType(typeof(object), properties);
                var typeMapFactory = new TypeMapFactory();
                TypeMap firstTypeMap;
                lock(_configurationProvider)
                {
                    firstTypeMap = typeMapFactory.CreateTypeMap(request.SourceType, letType, typeMap.Profile);
                }
                var secondParameter = Parameter(letType, "dtoLet");

                ReplaceSubQueries();

                var firstExpression = builder.CreateMapExpressionCore(request, instanceParameter, typePairCount, firstTypeMap, Default);
                return new QueryExpressions(firstExpression, projection, secondParameter);

                void ReplaceSubQueries()
                {
                    foreach(var letMapInfo in letMapInfos)
                    {
                        var letProperty = letType.GetDeclaredProperty(letMapInfo.Property.Name);
                        var letPropertyMap = firstTypeMap.FindOrCreatePropertyMapFor(letProperty);
                        letPropertyMap.CustomMapExpression =
                            Lambda(letMapInfo.MapFrom.ReplaceParameters(letMapInfo.MapFromSource), (ParameterExpression)instanceParameter);
                        projection = projection.Replace(letMapInfo.Marker, MakeMemberAccess(secondParameter, letProperty));
                    }
                    projection = new ReplaceMemberAccessesVisitor(instanceParameter, secondParameter).Visit(projection);
                }
            }

            class GetMemberAccessesVisitor : ExpressionVisitor
            {
                private readonly Expression _target;

                public List<MemberInfo> Members { get; } = new List<MemberInfo>();

                public GetMemberAccessesVisitor(Expression target)
                {
                    _target = target;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    if(node.Expression == _target)
                    {
                        Members.Add(node.Member);
                    }
                    return base.VisitMember(node);
                }

                public static IEnumerable<PropertyDescription> Retrieve(Expression expression, Expression target)
                {
                    var visitor = new GetMemberAccessesVisitor(target);
                    visitor.Visit(expression);
                    return visitor.Members.Select(member => new PropertyDescription(member.Name, member.GetMemberType()));
                }
            }

            class ReplaceMemberAccessesVisitor : ExpressionVisitor
            {
                private readonly Expression _oldObject, _newObject;

                public ReplaceMemberAccessesVisitor(Expression oldObject, Expression newObject)
                {
                    _oldObject = oldObject;
                    _newObject = newObject;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    if(node.Expression != _oldObject)
                    {
                        return base.VisitMember(node);
                    }
                    return MakeMemberAccess(_newObject, _newObject.Type.GetFieldOrProperty(node.Member.Name));
                }
            }
        }
    }

    public class LetPropertyMaps
    {
        public static readonly LetPropertyMaps Default = new LetPropertyMaps();

        protected LetPropertyMaps() { }

        public virtual Expression GetSubQueryMarker() => null;

        public virtual void Push(PropertyExpression propertyExpression) {}

        public virtual void Pop() {}

        public virtual int Count => 0;

        public virtual LetPropertyMaps New() => Default;

        public virtual QueryExpressions GetSubQueryExpression(ExpressionBuilder builder, Expression projection, TypeMap typeMap, ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount)
            => new QueryExpressions(projection);

        public struct PropertyPath
        {
            private readonly PropertyExpression[] _properties; 

            public PropertyPath(PropertyExpression[] properties, Expression marker)
            {
                _properties = properties;
                Marker = marker;
            }
            public Expression Marker { get; }
            public PropertyMap Last => _properties[_properties.Length - 1].PropertyMap;
            public Expression GetSourceExpression(Expression parameter) =>
                _properties.Take(_properties.Length - 1).SelectMany(p=>new MemberPath(p.Expression).Members).MemberAccesses(parameter);
            public PropertyDescription GetPropertyDescription() => new PropertyDescription("__" + string.Join("#", _properties.Select(p => p.PropertyMap.DestinationName)), Last.SourceType);
        }
    }

    public struct QueryExpressions
    {
        public QueryExpressions(Expression first, Expression second = null, ParameterExpression secondParameter = null)
        {
            First = first;
            Second = second;
            SecondParameter = secondParameter;
        }
        public Expression First { get; }
        public Expression Second { get; }
        public ParameterExpression SecondParameter { get; }
    }

    public static class ExpressionBuilderExtensions
    {
        public static Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(
            this IExpressionBuilder expressionBuilder)
        {
            return (Expression<Func<TSource, TDestination>>) expressionBuilder.GetMapExpression(typeof(TSource),
                typeof(TDestination), null, new MemberInfo[0])[0];
        }
    }

    public class PropertyExpression
    {
        public PropertyExpression(PropertyMap propertyMap) => PropertyMap = propertyMap;
        public Expression Expression { get; set; }
        public PropertyMap PropertyMap { get; }
    }
}