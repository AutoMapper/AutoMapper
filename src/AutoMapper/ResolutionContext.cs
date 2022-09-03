namespace AutoMapper;

/// <summary>
/// Context information regarding resolution of a destination value
/// </summary>
public class ResolutionContext : IInternalRuntimeMapper
{
    private Dictionary<ContextCacheKey, object> _instanceCache;
    private Dictionary<TypePair, int> _typeDepth;
    private readonly IInternalRuntimeMapper _mapper;
    private readonly IMappingOperationOptions _options;
    internal ResolutionContext(IInternalRuntimeMapper mapper, IMappingOperationOptions options = null)
    {
        _mapper = mapper;
        _options = options;
    }
    /// <summary>
    /// The items passed in the options of the Map call.
    /// </summary>
    public IDictionary<string, object> Items
    {
        get
        {
            if (_options == null)
            {
                ThrowInvalidMap();
            }
            return _options.Items;
        }
    }
    /// <summary>
    /// Current mapper
    /// </summary>
    public IRuntimeMapper Mapper => this;
    ResolutionContext IInternalRuntimeMapper.DefaultContext => _mapper.DefaultContext;
    /// <summary>
    /// Instance cache for resolving circular references
    /// </summary>
    public Dictionary<ContextCacheKey, object> InstanceCache
    {
        get
        {
            CheckDefault();
            return _instanceCache ??= new();
        }
    }
    /// <summary>
    /// Instance cache for resolving keeping track of depth
    /// </summary>
    private Dictionary<TypePair, int> TypeDepth
    {
        get
        {
            CheckDefault();
            return _typeDepth ??= new();
        }
    }
    TDestination IMapperBase.Map<TDestination>(object source) => ((IMapperBase)this).Map(source, default(TDestination));
    TDestination IMapperBase.Map<TSource, TDestination>(TSource source) => _mapper.Map(source, default(TDestination), this);
    TDestination IMapperBase.Map<TSource, TDestination>(TSource source, TDestination destination) => _mapper.Map(source, destination, this);
    object IMapperBase.Map(object source, Type sourceType, Type destinationType) => _mapper.Map(source, (object)null, this, sourceType, destinationType);
    object IMapperBase.Map(object source, object destination, Type sourceType, Type destinationType) => _mapper.Map(source, destination, this, sourceType, destinationType);
    TDestination IInternalRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context,
        Type sourceType, Type destinationType, MemberMap memberMap) => _mapper.Map(source, destination, context, sourceType, destinationType, memberMap);
    internal object CreateInstance(Type type) => ServiceCtor()(type) ?? throw new AutoMapperMappingException("Cannot create an instance of type " + type);
    private Func<Type, object> ServiceCtor() => _options?.ServiceCtor ?? _mapper.ServiceCtor;
    internal object GetDestination(object source, Type destinationType) => source == null ? null : InstanceCache.GetValueOrDefault(new(source, destinationType));
    internal void CacheDestination(object source, Type destinationType, object destination)
    {
        if (source == null)
        {
            return;
        }
        InstanceCache[new(source, destinationType)] = destination;
    }
    internal void IncrementTypeDepth(TypeMap typeMap) => TypeDepth[typeMap.Types]++;
    internal void DecrementTypeDepth(TypeMap typeMap) => TypeDepth[typeMap.Types]--;
    internal bool OverTypeDepth(TypeMap typeMap)
    {
        if (!TypeDepth.TryGetValue(typeMap.Types, out int depth))
        {
            TypeDepth[typeMap.Types] = 1;
            depth = 1;
        }
        return depth > typeMap.MaxDepth;
    }
    internal bool IsDefault => this == _mapper.DefaultContext;
    Func<Type, object> IInternalRuntimeMapper.ServiceCtor => ServiceCtor();
    internal static void CheckContext(ref ResolutionContext resolutionContext)
    {
        if (resolutionContext.IsDefault)
        {
            resolutionContext = new(resolutionContext._mapper);
        }
    }
    internal TDestination MapInternal<TSource, TDestination>(TSource source, TDestination destination, MemberMap memberMap) =>
        _mapper.Map(source, destination, this, memberMap: memberMap);
    internal object Map(object source, object destination, Type sourceType, Type destinationType, MemberMap memberMap) =>
        _mapper.Map(source, destination, this, sourceType, destinationType, memberMap);
    private void CheckDefault()
    {
        if (IsDefault)
        {
            ThrowInvalidMap();
        }
    }
    private static void ThrowInvalidMap() => throw new InvalidOperationException("Context.Items are only available when using a Map overload that takes Action<IMappingOperationOptions>!");
}
public readonly record struct ContextCacheKey(object Source, Type DestinationType);