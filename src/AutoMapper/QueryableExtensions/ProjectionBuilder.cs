using System.Runtime.CompilerServices;
namespace AutoMapper.QueryableExtensions.Impl;
using ParameterBag = IDictionary<string, object>;
using TypePairCount = Dictionary<ProjectionRequest, int>;
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IProjectionBuilder
{
    QueryExpressions GetProjection(Type sourceType, Type destinationType, object parameters, MemberPath[] membersToExpand);
    QueryExpressions CreateProjection(in ProjectionRequest request, LetPropertyMaps letPropertyMaps);
}
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IProjectionMapper
{
    bool IsMatch(TypePair context);
    Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps);
}
[EditorBrowsable(EditorBrowsableState.Never)]
public class ProjectionBuilder : IProjectionBuilder
{
    internal static List<IProjectionMapper> DefaultProjectionMappers() =>
        new(capacity: 5)
        {
            new AssignableProjectionMapper(),
            new EnumerableProjectionMapper(),
            new NullableSourceProjectionMapper(),
            new StringProjectionMapper(),
            new EnumProjectionMapper(),
        };
    private readonly LockingConcurrentDictionary<ProjectionRequest, QueryExpressions> _projectionCache;
    private readonly IGlobalConfiguration _configuration;
    private readonly IProjectionMapper[] _projectionMappers;
    public ProjectionBuilder(IGlobalConfiguration configuration, IProjectionMapper[] projectionMappers)
    {
        _configuration = configuration;
        _projectionMappers = projectionMappers;
        _projectionCache = new(CreateProjection);
    }
    public QueryExpressions GetProjection(Type sourceType, Type destinationType, object parameters, MemberPath[] membersToExpand)
    {
        var projectionRequest = new ProjectionRequest(sourceType, destinationType, membersToExpand, Array.Empty<ProjectionRequest>());
        var cachedExpressions = _projectionCache.GetOrAdd(projectionRequest);
        if (parameters == null && !_configuration.EnableNullPropagationForQueryMapping)
        {
            return cachedExpressions;
        }
        return cachedExpressions.Prepare(_configuration.EnableNullPropagationForQueryMapping, parameters);
    }
    private QueryExpressions CreateProjection(ProjectionRequest request) => 
        CreateProjection(request, new FirstPassLetPropertyMaps(_configuration, MemberPath.Empty, new()));
    public QueryExpressions CreateProjection(in ProjectionRequest request, LetPropertyMaps letPropertyMaps)
    {
        var instanceParameter = Parameter(request.SourceType, "dto"+ request.SourceType.Name);
        var typeMap = _configuration.ResolveTypeMap(request.SourceType, request.DestinationType) ?? throw TypeMap.MissingMapException(request.SourceType, request.DestinationType);
        var projection = CreateProjectionCore(request, instanceParameter, typeMap, letPropertyMaps);
        return letPropertyMaps.Count > 0 ?
            letPropertyMaps.GetSubQueryExpression(this, projection, typeMap, request, instanceParameter) :
            new(projection, instanceParameter);
    }
    private Expression CreateProjectionCore(ProjectionRequest request, Expression instanceParameter, TypeMap typeMap, LetPropertyMaps letPropertyMaps)
    {
        var customProjection = typeMap.CustomMapExpression?.ReplaceParameters(instanceParameter);
        if (customProjection != null)
        {
            return customProjection;
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
            foreach (var propertyMap in typeMap.PropertyMaps.Where(pm => 
                pm.CanResolveValue && pm.DestinationMember.CanBeSet() && !typeMap.ConstructorParameterMatches(pm.DestinationName))
                .OrderBy(pm => pm.DestinationMember.MetadataToken))
            {
                var propertyProjection = TryProjectMember(propertyMap);
                if (propertyProjection != null)
                {
                    propertiesProjections.Add(Bind(propertyMap.DestinationMember, propertyProjection));
                }
            }
        }
        Expression TryProjectMember(MemberMap memberMap, Expression defaultSource = null)
        {
            var memberProjection = new MemberProjection(memberMap);
            letPropertyMaps.Push(memberProjection);
            var memberExpression = ShouldExpand() ? ProjectMemberCore() : null;
            letPropertyMaps.Pop();
            return memberExpression;
            bool ShouldExpand() => memberMap.ExplicitExpansion != true || request.ShouldExpand(letPropertyMaps.GetCurrentPath());
            Expression ProjectMemberCore()
            {
                var memberTypeMap = _configuration.ResolveTypeMap(memberMap.SourceType, memberMap.DestinationType);
                var resolvedSource = ResolveSource();
                memberProjection.Expression ??= resolvedSource;
                var memberRequest = request.InnerRequest(resolvedSource.Type, memberMap.DestinationType);
                if (memberRequest.AlreadyExists && depth >= _configuration.RecursiveQueriesMaxDepth)
                {
                    return null;
                }
                Expression mappedExpression;
                if (memberTypeMap != null)
                {
                    mappedExpression = CreateProjectionCore(memberRequest, resolvedSource, memberTypeMap, letPropertyMaps);
                    if (mappedExpression != null && memberTypeMap.CustomMapExpression == null && memberMap.AllowsNullDestinationValues && 
                        resolvedSource is not ParameterExpression && !resolvedSource.Type.IsCollection())
                    {
                        // Handles null source property so it will not create an object with possible non-nullable properties which would result in an exception.
                        mappedExpression = resolvedSource.IfNullElse(Constant(null, mappedExpression.Type), mappedExpression);
                    }
                }
                else
                {
                    var projectionMapper = GetProjectionMapper();
                    mappedExpression = projectionMapper.Project(_configuration, memberRequest, resolvedSource, letPropertyMaps);
                }
                return mappedExpression == null ? null : memberMap.ApplyTransformers(mappedExpression, _configuration);
                Expression ResolveSource()
                {
                    var customSource = memberMap.IncludedMember?.ProjectToCustomSource;
                    var resolvedSource = memberMap switch
                    {
                        { CustomMapExpression: LambdaExpression mapFrom } => MapFromExpression(mapFrom),
                        { SourceMembers.Length: > 0  } => memberMap.ChainSourceMembers(CheckCustomSource()),
                        _ => defaultSource ?? throw CannotMap(memberMap, request.SourceType)
                    };
                    if (NullSubstitute())
                    {
                        return memberMap.NullSubstitute(resolvedSource);
                    }
                    return resolvedSource;
                    Expression MapFromExpression(LambdaExpression mapFrom)
                    {
                        if (memberTypeMap == null || mapFrom.IsMemberPath(out _) || mapFrom.Body is ParameterExpression)
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
                    var context = memberMap.Types();
                    foreach (var mapper in _projectionMappers)
                    {
                        if (mapper.IsMatch(context))
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
                New(constructorMap.Ctor, constructorMap.CtorParams.Select(map => TryProjectMember(map, map.DefaultValue(null)) ?? Default(map.DestinationType))),
            _ => New(typeMap.DestinationType)
        };
    }
    private static AutoMapperMappingException CannotMap(MemberMap memberMap, Type sourceType) => new(
        $"Unable to create a map expression from {memberMap.SourceMember?.DeclaringType?.Name}.{memberMap.SourceMember?.Name} ({sourceType}) to {memberMap.DestinationType.Name}.{memberMap.DestinationName} ({memberMap.DestinationType})",
        null, memberMap);
    [EditorBrowsable(EditorBrowsableState.Never)]
    class FirstPassLetPropertyMaps : LetPropertyMaps
    {
        readonly Stack<MemberProjection> _currentPath = new();
        readonly List<SubQueryPath> _savedPaths = new();
        readonly MemberPath _parentPath;
        public FirstPassLetPropertyMaps(IGlobalConfiguration configuration, MemberPath parentPath, TypePairCount builtProjections) : base(configuration, builtProjections) 
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
        public override LetPropertyMaps New() => new FirstPassLetPropertyMaps(Configuration, GetCurrentPath(), BuiltProjections);
        public override QueryExpressions GetSubQueryExpression(ProjectionBuilder builder, Expression projection, TypeMap typeMap, in ProjectionRequest request, ParameterExpression instanceParameter)
        {
            var letMapInfos = _savedPaths.Select(path =>
                (path.LetExpression,
                MapFromSource : path.GetSourceExpression(instanceParameter),
                Property : path.GetPropertyDescription(),
                path.Marker)).ToArray();
            var properties = letMapInfos.Select(m => m.Property).Concat(GePropertiesVisitor.Retrieve(projection, instanceParameter));
            var letType = ProxyGenerator.GetSimilarType(typeof(object), properties);
            TypeMap letTypeMap;
            lock(Configuration)
            {
                letTypeMap = new(request.SourceType, letType, typeMap.Profile, null);
            }
            var secondParameter = Parameter(letType, "dtoLet");
            ReplaceSubQueries();
            var letClause = builder.CreateProjectionCore(request, instanceParameter, letTypeMap, base.New());
            return new(Lambda(projection, secondParameter), Lambda(letClause, instanceParameter));
            void ReplaceSubQueries()
            {
                foreach(var letMapInfo in letMapInfos)
                {
                    var letProperty = letType.GetProperty(letMapInfo.Property.Name);
                    var letPropertyMap = letTypeMap.FindOrCreatePropertyMapFor(letProperty, letMapInfo.Property.Type);
                    letPropertyMap.MapFrom(Lambda(letMapInfo.LetExpression.ReplaceParameters(letMapInfo.MapFromSource), secondParameter));
                    projection = projection.Replace(letMapInfo.Marker, Property(secondParameter, letProperty));
                }
                projection = new ReplaceMemberAccessesVisitor(instanceParameter, secondParameter).Visit(projection);
            }
        }
        readonly record struct SubQueryPath(MemberProjection[] Members, LambdaExpression LetExpression, Expression Marker)
        {
            public SubQueryPath(MemberProjection[] members, LambdaExpression letExpression) : this(members, letExpression, Default(letExpression.Body.Type)){ }
            public Expression GetSourceExpression(Expression parameter)
            {
                Expression sourceExpression = parameter;
                for (int index = 0; index < Members.Length - 1; index++)
                {
                    var sourceMember = Members[index].Expression;
                    if (sourceMember is LambdaExpression lambda)
                    {
                        sourceExpression = lambda.ReplaceParameters(sourceExpression);
                    }
                    else
                    {
                        var chain = sourceMember.GetChain();
                        if (chain.TryPeek(out var first))
                        {
                            sourceExpression = sourceMember.Replace(first.Target, sourceExpression);
                        }
                    }
                }
                return sourceExpression;
            }
            public PropertyDescription GetPropertyDescription() => new("__" + string.Join("#", Members.Select(p => p.MemberMap.DestinationName)), LetExpression.Body.Type);
            internal bool IsEquivalentTo(SubQueryPath other) => LetExpression == other.LetExpression && Members.Length == other.Members.Length &&
                Members.Take(Members.Length - 1).Zip(other.Members, (left, right) => left.MemberMap == right.MemberMap).All(item => item);
        }
        class GePropertiesVisitor : ExpressionVisitor
        {
            private readonly Expression _target;
            public List<MemberInfo> Members { get; } = new();
            public GePropertiesVisitor(Expression target) => _target = target;
            protected override Expression VisitMember(MemberExpression node)
            {
                if(node.Expression == _target)
                {
                    Members.TryAdd(node.Member);
                }
                return base.VisitMember(node);
            }
            public static IEnumerable<PropertyDescription> Retrieve(Expression expression, Expression target)
            {
                var visitor = new GePropertiesVisitor(target);
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
                return PropertyOrField(_newObject, node.Member.Name);
            }
        }
    }
}
[EditorBrowsable(EditorBrowsableState.Never)]
public class LetPropertyMaps
{
    protected LetPropertyMaps(IGlobalConfiguration configuration, TypePairCount builtProjections)
    {
        Configuration = configuration;
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
    public virtual Expression GetSubQueryMarker(LambdaExpression letExpression) => letExpression.Body;
    public virtual void Push(MemberProjection memberProjection) { }
    public virtual MemberPath GetCurrentPath() => MemberPath.Empty;
    public virtual void Pop() {}
    public virtual int Count => 0;
    public IGlobalConfiguration Configuration { get; }
    public virtual LetPropertyMaps New() => new(Configuration, BuiltProjections);
    public virtual QueryExpressions GetSubQueryExpression(ProjectionBuilder builder, Expression projection, TypeMap typeMap, in ProjectionRequest request, ParameterExpression instanceParameter)
        => default;
}
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct QueryExpressions(LambdaExpression Projection, LambdaExpression LetClause = null)
{
    public QueryExpressions(Expression projection, ParameterExpression parameter) : this(projection == null ? null : Lambda(projection, parameter)) { }
    public bool Empty => Projection == null;
    public T Chain<T>(T source, Func<T, LambdaExpression, T> select) => LetClause == null ? select(source, Projection) : select(select(source, LetClause), Projection);
    internal QueryExpressions Prepare(bool enableNullPropagationForQueryMapping, object parameters)
    {
        return new(Prepare(Projection), Prepare(LetClause));
        LambdaExpression Prepare(Expression cachedExpression)
        {
            var result = parameters == null ? cachedExpression : ParameterExpressionVisitor.SetParameters(parameters, cachedExpression);
            return (LambdaExpression)(enableNullPropagationForQueryMapping ? NullsafeQueryRewriter.NullCheck(result) : result);
        }
    }
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
public readonly record struct ProjectionRequest(Type SourceType, Type DestinationType, MemberPath[] MembersToExpand, ICollection<ProjectionRequest> PreviousRequests)
{
    public ProjectionRequest InnerRequest(Type sourceType, Type destinationType)
    {
        var previousRequests = PreviousRequests.ToList();
        previousRequests.TryAdd(this);
        return new(sourceType, destinationType, MembersToExpand, previousRequests);
    }
    public bool AlreadyExists => PreviousRequests.Contains(this);
    public bool Equals(ProjectionRequest other) => SourceType == other.SourceType && DestinationType == other.DestinationType &&
        MembersToExpand.SequenceEqual(other.MembersToExpand);
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
    public bool ShouldExpand(MemberPath currentPath)
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