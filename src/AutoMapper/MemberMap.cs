namespace AutoMapper;
/// <summary>
/// The base class for member maps (property, constructor and path maps).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class MemberMap : IValueResolver
{
    protected MemberMap(TypeMap typeMap = null) => TypeMap = typeMap;
    internal static readonly MemberMap Instance = new();
    public TypeMap TypeMap { get; protected set; }
    public LambdaExpression CustomMapExpression => Resolver?.ProjectToExpression;
    public bool IsResolveConfigured => Resolver != null && Resolver != this;
    public void MapFrom(LambdaExpression lambda) => SetResolver(new ExpressionResolver(lambda));
    public void SetResolver(IValueResolver resolver)
    {
        Resolver = resolver;
        Ignored = false;
    }
    public virtual Type SourceType => default;
    public virtual MemberInfo[] SourceMembers { get => Array.Empty<MemberInfo>(); set { } }
    public virtual IncludedMember IncludedMember => null;
    public virtual string DestinationName => default;
    public virtual Type DestinationType { get => default; protected set { } }
    public virtual TypePair Types() => new(SourceType, DestinationType);
    public bool CanResolveValue => !Ignored && Resolver != null;
    public bool IsMapped => Ignored || CanResolveValue;
    public virtual bool Ignored { get => default; set { } }
    public virtual bool? ExplicitExpansion { get => default; set { } }
    public virtual bool Inline { get; set; } = true;
    public virtual bool? AllowNull { get => null; set { } }
    public virtual bool CanBeSet => true;
    public virtual bool? UseDestinationValue { get => default; set { } }
    public virtual object NullSubstitute { get => default; set { } }
    public virtual LambdaExpression PreCondition { get => default; set { } }
    public virtual LambdaExpression Condition { get => default; set { } }
    public IValueResolver Resolver { get; protected set; }
    public virtual IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => Array.Empty<ValueTransformerConfiguration>();
    public MemberInfo SourceMember => Resolver?.GetSourceMember(this);
    public string GetSourceMemberName() => Resolver?.SourceMemberName ?? SourceMember?.Name;
    public bool MustUseDestination => UseDestinationValue is true || !CanBeSet;
    public void MapFrom(string sourceMembersPath, MemberInfo[] members)
    {
        var sourceType = TypeMap.SourceType;
        var sourceMembers = sourceType.ContainsGenericParameters ? members :
            members[0].DeclaringType.ContainsGenericParameters ? ReflectionHelper.GetMemberPath(sourceType, sourceMembersPath, TypeMap) : members;
        SetResolver(new MemberPathResolver(sourceMembers));
    }
    public override string ToString() => DestinationName;
    public Expression ChainSourceMembers(Expression source) => SourceMembers.Chain(source);
    public Expression ChainSourceMembers(IGlobalConfiguration configuration, Expression source, Expression defaultValue)
    {
        var expression = ChainSourceMembers(source);
        return IncludedMember == null && SourceMembers.Length < 2 ? expression : expression.NullCheck(configuration, this, defaultValue);
    }
    public bool AllowsNullDestinationValues => Profile?.AllowsNullDestinationValuesFor(this) ?? true;
    public ProfileMap Profile => TypeMap?.Profile;
    protected Type GetSourceType() => Resolver?.ResolvedType ?? DestinationType;
    public void MapByConvention(MemberInfo[] sourceMembers)
    {
        Debug.Assert(sourceMembers.Length > 0);
        SourceMembers = sourceMembers;
        Resolver = this;
    }
    Expression IValueResolver.GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source, Expression destination, Expression destinationMember) =>
        ChainSourceMembers(configuration, source, destinationMember);
    MemberInfo IValueResolver.GetSourceMember(MemberMap memberMap) => SourceMembers[0];
    Type IValueResolver.ResolvedType => SourceMembers[^1].GetMemberType();
}
public readonly record struct ValueTransformerConfiguration(Type ValueType, LambdaExpression TransformerExpression)
{
    public bool IsMatch(MemberMap memberMap) => ValueType.IsAssignableFrom(memberMap.SourceType) && memberMap.DestinationType.IsAssignableFrom(ValueType);
}
public static class ValueTransformerConfigurationExtensions
{
    /// <summary>
    /// Apply a transformation function after any resolved destination member value with the given type
    /// </summary>
    /// <typeparam name="TValue">Value type to match and transform</typeparam>
    /// <param name="valueTransformers">Value transformer list</param>
    /// <param name="transformer">Transformation expression</param>
    public static void Add<TValue>(this List<ValueTransformerConfiguration> valueTransformers, Expression<Func<TValue, TValue>> transformer) => 
        valueTransformers.Add(new ValueTransformerConfiguration(typeof(TValue), transformer));
}