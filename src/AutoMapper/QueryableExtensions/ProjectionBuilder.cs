using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Execution;
using AutoMapper.Internal;
namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    using static ExpressionBuilder;
    using ParameterBag = IDictionary<string, object>;
    using TypePairCount = IDictionary<ProjectionRequest, int>;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IProjectionBuilder
    {
        QueryExpressions GetProjection(Type sourceType, Type destinationType, object parameters, MemberPath[] membersToExpand);
        QueryExpressions CreateProjection(in ProjectionRequest request, LetPropertyMaps letPropertyMaps);
        Expression CreateInnerProjection(in ProjectionRequest request, Expression instanceParameter, LetPropertyMaps letPropertyMaps);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IProjectionMapper
    {
        bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource);
        Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ProjectionBuilder : IProjectionBuilder
    {
        internal static List<IProjectionMapper> DefaultProjectionMappers() =>
            new List<IProjectionMapper>
            {
                new CustomProjectionMapper(),
                new MappedTypeProjectionMapper(),
                new AssignableProjectionMapper(),
                new EnumerableProjectionMapper(),
                new NullableSourceProjectionMapper(),
                new StringProjectionMapper(),
                new EnumProjectionMapper(),
            };
        private readonly LockingConcurrentDictionary<ProjectionRequest, QueryExpressions> _projectionCache;
        private readonly IGlobalConfiguration _configurationProvider;
        private readonly IProjectionMapper[] _projectionMappers;
        public ProjectionBuilder(IGlobalConfiguration configurationProvider, IProjectionMapper[] projectionMappers)
        {
            _configurationProvider = configurationProvider;
            _projectionMappers = projectionMappers;
            _projectionCache = new LockingConcurrentDictionary<ProjectionRequest, QueryExpressions>(CreateProjection);
        }
        public QueryExpressions GetProjection(Type sourceType, Type destinationType, object parameters, MemberPath[] membersToExpand)
        {
            var projectionRequest = new ProjectionRequest(sourceType, destinationType, membersToExpand, Array.Empty<ProjectionRequest>());
            var cachedExpressions = _projectionCache.GetOrAdd(projectionRequest);
            if (parameters == null && !_configurationProvider.EnableNullPropagationForQueryMapping)
            {
                return cachedExpressions;
            }
            return cachedExpressions.Prepare(_configurationProvider.EnableNullPropagationForQueryMapping, parameters);
        }
        private QueryExpressions CreateProjection(ProjectionRequest request) => 
            CreateProjection(request, new FirstPassLetPropertyMaps(_configurationProvider, MemberPath.Empty, new Dictionary<ProjectionRequest, int>()));
        public QueryExpressions CreateProjection(in ProjectionRequest request, LetPropertyMaps letPropertyMaps)
        {
            var instanceParameter = Parameter(request.SourceType, "dto"+ request.SourceType.Name);
            var projection = CreateProjectionCore(request, instanceParameter, letPropertyMaps, out var typeMap);
            return letPropertyMaps.Count > 0 ?
                letPropertyMaps.GetSubQueryExpression(this, projection, typeMap, request, instanceParameter) :
                new QueryExpressions(projection, instanceParameter);
        }
        public Expression CreateInnerProjection(in ProjectionRequest request, Expression instanceParameter, LetPropertyMaps letPropertyMaps) =>
            CreateProjectionCore(request, instanceParameter, letPropertyMaps, out var _);
        private Expression CreateProjectionCore(in ProjectionRequest request, Expression instanceParameter, LetPropertyMaps letPropertyMaps, out TypeMap typeMap)
        {
            typeMap = _configurationProvider.ResolveTypeMap(request.SourceType, request.DestinationType) ?? throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            return CreateProjectionCore(request, instanceParameter, typeMap, letPropertyMaps);
        }
        private Expression CreateProjectionCore(ProjectionRequest request, Expression instanceParameter, TypeMap typeMap, LetPropertyMaps letPropertyMaps)
        {
            if (typeMap.CustomMapExpression != null)
            {
                return typeMap.CustomMapExpression.ReplaceParameters(instanceParameter);
            }
            var propertiesProjections = new List<MemberBinding>();
            int depth;
            if (OverMaxDepth())
            {
                if (typeMap.Profile.AllowNullDestinationValues)
                {
                    return null;
                }
            }
            else
            {
                ProjectProperties();
            }
            var constructorExpression = CreateDestination();
            var expression = MemberInit(constructorExpression, propertiesProjections);
            return expression;
            bool OverMaxDepth()
            {
                depth = letPropertyMaps.IncrementDepth(request);
                return typeMap.MaxDepth > 0 && depth >= typeMap.MaxDepth;
            }
            void ProjectProperties()
            {
                foreach (var propertyMap in typeMap.PropertyMaps.Where(pm => pm.CanResolveValue && pm.DestinationMember.CanBeSet()).OrderBy(pm => pm.DestinationName))
                {
                    var propertyProjection = TryProjectMember(propertyMap, propertyMap.ExplicitExpansion);
                    if (propertyProjection != null)
                    {
                        propertiesProjections.Add(Bind(propertyMap.DestinationMember, propertyProjection));
                    }
                }
            }
            Expression TryProjectMember(MemberMap memberMap, bool? explicitExpansion = null)
            {
                var memberProjection = new MemberProjection(memberMap);
                letPropertyMaps.Push(memberProjection);
                var memberExpression = ShouldExpand() ? ProjectMemberCore() : null;
                letPropertyMaps.Pop();
                return memberExpression;
                bool ShouldExpand() => explicitExpansion != true || request.ShouldExpand(letPropertyMaps.GetCurrentPath());
                Expression ProjectMemberCore()
                {
                    var memberTypeMap = _configurationProvider.ResolveTypeMap(memberMap.SourceType, memberMap.DestinationType);
                    var resolvedSource = ResolveSource();
                    memberProjection.Expression ??= resolvedSource;
                    var memberRequest = new ProjectionRequest(resolvedSource.Type, memberMap.DestinationType, request.MembersToExpand, request.GetPreviousRequestsAndSelf());
                    if (memberRequest.AlreadyExists && depth >= _configurationProvider.RecursiveQueriesMaxDepth)
                    {
                        return null;
                    }
                    var projectionMapper = GetProjectionMapper();
                    var mappedExpression = projectionMapper.Project(_configurationProvider, memberMap, memberTypeMap, memberRequest, resolvedSource, letPropertyMaps);
                    return mappedExpression == null ? null : memberMap.ApplyTransformers(mappedExpression);
                    Expression ResolveSource()
                    {
                        var customSource = memberMap.IncludedMember?.ProjectToCustomSource;
                        var resolvedSource = memberMap switch
                        {
                            { CustomMapExpression: LambdaExpression mapFrom } => MapFromExpression(mapFrom),
                            { SourceMembers: { Length: >0 } sourceMembers } => sourceMembers.Chain(CheckCustomSource()),
                            _ => throw CannotMap(memberMap, request.SourceType)
                        };
                        if (NullSubstitute())
                        {
                            return memberMap.NullSubstitute(resolvedSource);
                        }
                        return resolvedSource;
                        Expression MapFromExpression(LambdaExpression mapFrom)
                        {
                            if (memberTypeMap == null || !IsQuery())
                            {
                                return mapFrom.ReplaceParameters(CheckCustomSource());
                            }
                            if (customSource == null)
                            {
                                memberProjection.Expression = mapFrom;
                                return letPropertyMaps.GetSubQueryMarker(mapFrom);
                            }
                            var newMapFrom = IncludedMember.Chain(customSource, mapFrom);
                            memberProjection.Expression = newMapFrom;
                            return letPropertyMaps.GetSubQueryMarker(newMapFrom);
                            bool IsQuery() => mapFrom.Body is MethodCallExpression { Method: { IsStatic: true } method } &&
                                method.DeclaringType == typeof(Enumerable);
                        }
                        bool NullSubstitute() => memberMap.NullSubstitute != null && resolvedSource is MemberExpression && (resolvedSource.Type.IsNullableType() || resolvedSource.Type == typeof(string));
                        Expression CheckCustomSource()
                        {
                            if (customSource == null)
                            {
                                return instanceParameter;
                            }
                            return customSource.IsMemberPath(out _) ? customSource.ReplaceParameters(instanceParameter) : letPropertyMaps.GetSubQueryMarker(customSource);
                        }
                    }
                    IProjectionMapper GetProjectionMapper()
                    {
                        foreach (var mapper in _projectionMappers)
                        {
                            if (mapper.IsMatch(memberMap, memberTypeMap, resolvedSource))
                            {
                                return mapper;
                            }
                        }
                        throw CannotMap(memberMap, resolvedSource.Type);
                    }
                }
            }
            NewExpression CreateDestination() => typeMap switch
            {
                { CustomCtorExpression: LambdaExpression ctorExpression } => (NewExpression)ctorExpression.ReplaceParameters(instanceParameter),
                { ConstructorMap: { CanResolve: true } constructorMap } => 
                    New(constructorMap.Ctor, constructorMap.CtorParams.Select(map => TryProjectMember(map) ?? Default(map.DestinationType))),
                _ => New(typeMap.DestinationTypeToUse)
            };
        }
        private static AutoMapperMappingException CannotMap(MemberMap memberMap, Type sourceType) => new AutoMapperMappingException(
            $"Unable to create a map expression from {memberMap.SourceMember?.DeclaringType?.Name}.{memberMap.SourceMember?.Name} ({sourceType}) to {memberMap.DestinationType.Name}.{memberMap.DestinationName} ({memberMap.DestinationType})",
            null, memberMap);
        [EditorBrowsable(EditorBrowsableState.Never)]
        class FirstPassLetPropertyMaps : LetPropertyMaps
        {
            readonly Stack<MemberProjection> _currentPath = new Stack<MemberProjection>();
            readonly List<SubQueryPath> _savedPaths = new List<SubQueryPath>();
            readonly MemberPath _parentPath;
            public FirstPassLetPropertyMaps(IGlobalConfiguration configurationProvider, MemberPath parentPath, TypePairCount builtProjections) : base(configurationProvider, builtProjections) 
                => _parentPath = parentPath;
            public override Expression GetSubQueryMarker(LambdaExpression letExpression)
            {
                var subQueryPath = new SubQueryPath(_currentPath.Reverse().ToArray(), letExpression);
                var existingPath = _savedPaths.SingleOrDefault(s => s.IsEquivalentTo(subQueryPath));
                if (existingPath.Marker != null)
                {
                    return existingPath.Marker;
                }
                _savedPaths.Add(subQueryPath);
                return subQueryPath.Marker;
            }
            public override void Push(MemberProjection memberProjection) => _currentPath.Push(memberProjection);
            public override MemberPath GetCurrentPath() => _parentPath.Concat(
                _currentPath.Reverse().Select(p => (p.MemberMap as PropertyMap)?.DestinationMember).Where(p => p != null));
            public override void Pop() => _currentPath.Pop();
            public override int Count => _savedPaths.Count;
            public override LetPropertyMaps New() => new FirstPassLetPropertyMaps(ConfigurationProvider, GetCurrentPath(), BuiltProjections);
            public override QueryExpressions GetSubQueryExpression(ProjectionBuilder builder, Expression projection, TypeMap typeMap, in ProjectionRequest request, ParameterExpression instanceParameter)
            {
                var letMapInfos = _savedPaths.Select(path =>
                new {
                    path.LetExpression,
                    MapFromSource = path.GetSourceExpression(instanceParameter),
                    Property = path.GetPropertyDescription(),
                    path.Marker
                }).ToArray();
                var properties = letMapInfos.Select(m => m.Property).Concat(GetMemberAccessesVisitor.Retrieve(projection, instanceParameter));
                var letType = ProxyGenerator.GetSimilarType(typeof(object), properties);
                TypeMap letTypeMap;
                lock(ConfigurationProvider)
                {
                    letTypeMap = new TypeMap(request.SourceType, letType, typeMap.Profile);
                }
                var secondParameter = Parameter(letType, "dtoLet");
                ReplaceSubQueries();
                var letClause = builder.CreateProjectionCore(request, instanceParameter, letTypeMap, base.New());
                return new QueryExpressions(Lambda(projection, secondParameter), Lambda(letClause, instanceParameter));
                void ReplaceSubQueries()
                {
                    foreach(var letMapInfo in letMapInfos)
                    {
                        var letProperty = letType.GetProperty(letMapInfo.Property.Name);
                        var letPropertyMap = letTypeMap.FindOrCreatePropertyMapFor(letProperty, letMapInfo.Property.Type);
                        letPropertyMap.CustomMapExpression = Lambda(letMapInfo.LetExpression.ReplaceParameters(letMapInfo.MapFromSource), secondParameter);
                        projection = projection.Replace(letMapInfo.Marker, Property(secondParameter, letProperty));
                    }
                    projection = new ReplaceMemberAccessesVisitor(instanceParameter, secondParameter).Visit(projection);
                }
            }
            readonly struct SubQueryPath
            {
                private readonly MemberProjection[] _members;
                public readonly Expression Marker;
                public readonly LambdaExpression LetExpression;
                public SubQueryPath(MemberProjection[] members, LambdaExpression letExpression)
                {
                    _members = members;
                    Marker = Default(letExpression.Body.Type);
                    LetExpression = letExpression;
                }
                public Expression GetSourceExpression(Expression parameter) => _members.Take(_members.Length - 1).Select(p => p.Expression).Aggregate(parameter,
                    (left, right) => right is LambdaExpression lambda ? lambda.ReplaceParameters(left) : right.Replace(right.GetChain().Peek().Target, left));
                public PropertyDescription GetPropertyDescription() => new PropertyDescription("__" + string.Join("#", _members.Select(p => p.MemberMap.DestinationName)), LetExpression.Body.Type);
                internal bool IsEquivalentTo(in SubQueryPath other) => LetExpression == other.LetExpression && _members.Length == other._members.Length &&
                    _members.Take(_members.Length - 1).Zip(other._members, (left, right) => left.MemberMap == right.MemberMap).All(item => item);
            }
            class GetMemberAccessesVisitor : ExpressionVisitor
            {
                private readonly Expression _target;
                public List<MemberInfo> Members { get; } = new List<MemberInfo>();
                public GetMemberAccessesVisitor(Expression target) => _target = target;
                protected override Expression VisitMember(MemberExpression node)
                {
                    if (node.Expression == _target)
                    {
                        Members.Add(node.Member);
                    }
                    else if (node.Expression is ConditionalExpression conditionalExpression && (conditionalExpression.IfTrue == _target || conditionalExpression.IfFalse == _target))
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
                        if (node.Expression == _oldObject)
                        {
                            return PropertyOrField(_newObject, node.Member.Name);
                        }
                        else if (node.Expression is ConditionalExpression conditionalExpression && (conditionalExpression.IfTrue == _oldObject || conditionalExpression.IfFalse == _oldObject))
                        {
                            var test = Visit(conditionalExpression.Test);
                            var ifTrue = conditionalExpression.IfTrue == _oldObject ? _newObject : Visit(conditionalExpression.IfTrue);
                            var ifFalse = conditionalExpression.IfFalse == _oldObject ? _newObject : Visit(conditionalExpression.IfFalse);

                            if (ifTrue.Type != ifFalse.Type)
                            {
                                if (ifTrue == _newObject && ifFalse is ConstantExpression oldConstantExpr)
                                {
                                    ifFalse = Constant(oldConstantExpr.Value, _newObject.Type);
                                }
                                else if (ifFalse == _newObject && ifTrue is ConstantExpression oldConstantExp)
                                {
                                    ifTrue = Constant(oldConstantExp.Value, _newObject.Type);
                                }
                            }

                            var result = Condition(test, ifTrue, ifFalse);

                            return PropertyOrField(result, node.Member.Name);
                        }
                    return base.VisitMember(node);
                }

                protected override Expression VisitConditional(ConditionalExpression node)
                {
                    var test = Visit(node.Test);
                    var ifTrue = node.IfTrue == _oldObject ? _newObject : Visit(node.IfTrue);
                    var ifFalse = node.IfFalse == _oldObject ? _newObject : Visit(node.IfFalse);

                    if (ifTrue.Type != ifFalse.Type)
                    {
                        if (ifTrue == _newObject && ifFalse is ConstantExpression oldConstantExpr)
                        {
                            ifFalse = Constant(oldConstantExpr.Value, _newObject.Type);
                        }
                        else if (ifFalse == _newObject && ifTrue is ConstantExpression oldConstantExp)
                        {
                            ifTrue = Constant(oldConstantExp.Value, _newObject.Type);
                        }
                    }
                    var result = Condition(test, ifTrue, ifFalse);

                    return result;
                }
            }
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LetPropertyMaps
    {
        protected LetPropertyMaps(IGlobalConfiguration configurationProvider, TypePairCount builtProjections)
        {
            ConfigurationProvider = configurationProvider;
            BuiltProjections = builtProjections;
        }
        protected TypePairCount BuiltProjections { get; }
        public int IncrementDepth(in ProjectionRequest request)
        {
            if (BuiltProjections.TryGetValue(request, out var depth))
            {
                depth++;
            }
            BuiltProjections[request] = depth;
            return depth;
        }
        public virtual Expression GetSubQueryMarker(LambdaExpression letExpression) => null;
        public virtual void Push(MemberProjection memberProjection) { }
        public virtual MemberPath GetCurrentPath() => MemberPath.Empty;
        public virtual void Pop() {}
        public virtual int Count => 0;
        public IGlobalConfiguration ConfigurationProvider { get; }
        public virtual LetPropertyMaps New() => new LetPropertyMaps(ConfigurationProvider, BuiltProjections);
        public virtual QueryExpressions GetSubQueryExpression(ProjectionBuilder builder, Expression projection, TypeMap typeMap, in ProjectionRequest request, ParameterExpression instanceParameter)
            => default;
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct QueryExpressions
    {
        public readonly LambdaExpression LetClause;
        public readonly LambdaExpression Projection;
        public QueryExpressions(Expression projection, ParameterExpression parameter) : this(projection == null ? null : Lambda(projection, parameter)) { }
        public QueryExpressions(LambdaExpression projection, LambdaExpression letClause  = null)
        {
            LetClause = letClause;
            Projection = projection;
        }
        public bool Empty => Projection == null;
        internal T Chain<T>(T source, Func<T, LambdaExpression, T> select) => 
            LetClause == null ? select(source, Projection) : select(select(source, LetClause), Projection);
        internal QueryExpressions Prepare(bool enableNullPropagationForQueryMapping, object parameters)
        {
            return new QueryExpressions(Prepare(Projection), Prepare(LetClause));
            LambdaExpression Prepare(Expression cachedExpression)
            {
                var result = parameters == null ? cachedExpression : ParameterExpressionVisitor.SetParameters(parameters, cachedExpression);
                return (LambdaExpression)(enableNullPropagationForQueryMapping ? NullsafeQueryRewriter.NullCheck(result) : result);
            }
        }
    }
    public static class ExpressionBuilderExtensions
    {
        public static Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(this IProjectionBuilder expressionBuilder) => 
            (Expression<Func<TSource, TDestination>>) expressionBuilder.GetProjection(typeof(TSource), typeof(TDestination), null, Array.Empty<MemberPath>()).Projection;
    }
    public class MemberProjection
    {
        public MemberProjection(MemberMap memberMap) => MemberMap = memberMap;
        public Expression Expression { get; set; }
        public MemberMap MemberMap { get; }
    }
    abstract class ParameterExpressionVisitor : ExpressionVisitor
    {
        public static Expression SetParameters(object parameters, Expression expression)
        {
            var visitor = parameters is ParameterBag dictionary ? (ParameterExpressionVisitor)new ConstantExpressionReplacementVisitor(dictionary) : new ObjectParameterExpressionReplacementVisitor(parameters);
            return visitor.Visit(expression);
        }
        protected abstract Expression GetValue(string name);
        protected override Expression VisitMember(MemberExpression node)
        {
            if (!node.Member.DeclaringType.Has<CompilerGeneratedAttribute>())
            {
                return base.VisitMember(node);
            }
            var parameterName = node.Member.Name;
            var parameterValue = GetValue(parameterName);
            if (parameterValue == null)
            {
                const string VbPrefix = "$VB$Local_";
                if (!parameterName.StartsWith(VbPrefix, StringComparison.Ordinal) || (parameterValue = GetValue(parameterName.Substring(VbPrefix.Length))) == null)
                {
                    return base.VisitMember(node);
                }
            }
            return ToType(parameterValue, node.Member.GetMemberType());
        }
        class ObjectParameterExpressionReplacementVisitor : ParameterExpressionVisitor
        {
            private readonly object _parameters;
            public ObjectParameterExpressionReplacementVisitor(object parameters) => _parameters = parameters;
            protected override Expression GetValue(string name)
            {
                var matchingMember = _parameters.GetType().GetProperty(name);
                return matchingMember != null ? Property(Constant(_parameters), matchingMember) : null;
            }
        }
        class ConstantExpressionReplacementVisitor : ParameterExpressionVisitor
        {
            private readonly ParameterBag _paramValues;
            public ConstantExpressionReplacementVisitor(ParameterBag paramValues) => _paramValues = paramValues;
            protected override Expression GetValue(string name) => _paramValues.TryGetValue(name, out object parameterValue) ? Constant(parameterValue) : null;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public readonly struct ProjectionRequest : IEquatable<ProjectionRequest>
    {
        public readonly Type SourceType;
        public readonly Type DestinationType;
        public readonly MemberPath[] MembersToExpand;
        private readonly ICollection<ProjectionRequest> _previousRequests;
        public ProjectionRequest(Type sourceType, Type destinationType, MemberPath[] membersToExpand, ICollection<ProjectionRequest> previousRequests)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand;
            _previousRequests = previousRequests;
        }
        internal bool AlreadyExists => _previousRequests.Contains(this);
        internal ICollection<ProjectionRequest> GetPreviousRequestsAndSelf() => new HashSet<ProjectionRequest>(_previousRequests.Concat(new[] { this }));
        public bool Equals(ProjectionRequest other) => SourceType == other.SourceType && DestinationType == other.DestinationType &&
                MembersToExpand.SequenceEqual(other.MembersToExpand);
        public override bool Equals(object obj) => obj is ProjectionRequest request && Equals(request);
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(SourceType);
            hashCode.Add(DestinationType);
            foreach (var member in MembersToExpand)
            {
                hashCode.Add(member);
            }
            return hashCode.ToHashCode();
        }
        public static bool operator ==(in ProjectionRequest left, in ProjectionRequest right) => Equals(left, right);
        public static bool operator !=(in ProjectionRequest left, in ProjectionRequest right) => !Equals(left, right);
        public bool ShouldExpand(in MemberPath currentPath)
        {
            foreach (var memberToExpand in MembersToExpand)
            {
                if (memberToExpand.StartsWith(currentPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}