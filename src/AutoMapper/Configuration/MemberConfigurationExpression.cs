namespace AutoMapper.Configuration;
public interface IPropertyMapConfiguration
{
    void Configure(TypeMap typeMap);
    MemberInfo DestinationMember { get; }
    LambdaExpression SourceExpression { get; }
    LambdaExpression GetDestinationExpression();
    IPropertyMapConfiguration Reverse();
    bool Ignored => false;
}
public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
{
    private MemberInfo[] _sourceMembers;
    private readonly Type _sourceType;
    protected List<Action<PropertyMap>> PropertyMapActions { get; } = new List<Action<PropertyMap>>();
    public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
    {
        DestinationMember = destinationMember;
        _sourceType = sourceType;
    }
    public MemberInfo DestinationMember { get; }
    public void MapAtRuntime() => PropertyMapActions.Add(pm => pm.Inline = false);
    public void NullSubstitute(object nullSubstitute) => PropertyMapActions.Add(pm => pm.NullSubstitute = nullSubstitute);
    public void MapFrom<TValueResolver>() where TValueResolver : IValueResolver<TSource, TDestination, TMember> =>
        MapFromCore(new(typeof(TValueResolver), typeof(IValueResolver<TSource, TDestination, TMember>)));
    protected void MapFromCore(ClassValueResolver config) => SetResolver(config);
    protected void SetResolver(IValueResolver config) => PropertyMapActions.Add(pm => pm.Resolver = config);
    public void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
            MapFromCore<TValueResolver, TSourceMember>(sourceMember);
    public void MapFrom<TValueResolver, TSourceMember>(string sourceMemberName) where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
            MapFromCore<TValueResolver, TSourceMember>(null, sourceMemberName);
    private void MapFromCore<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember, string sourceMemberName = null) where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
        MapFromCore(new(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
        {
            SourceMemberName = sourceMemberName,
            SourceMemberLambda = sourceMember
        });
    public void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver) =>
        MapFromCore(new(valueResolver, typeof(IValueResolver<TSource, TDestination, TMember>)));
    public void MapFrom<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember) =>
        MapFromCore(new(valueResolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
        {
            SourceMemberLambda = sourceMember
        });
    public void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction) =>
        MapFromResult((src, dest, destMember, ctxt) => mappingFunction(src, dest));
    public void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction) =>
        MapFromResult((src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember));
    public void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction) =>
        MapFromResult((src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember, ctxt));
    private void MapFromResult<TResult>(Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr) => 
        SetResolver(new FuncResolver(expr));
    public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression) => MapFromUntyped(mapExpression);
    internal void MapFromUntyped(LambdaExpression sourceExpression)
    {
        SourceExpression = sourceExpression;
        PropertyMapActions.Add(pm => pm.MapFrom(sourceExpression));
    }
    public void MapFrom(string sourceMembersPath)
    {
        _sourceMembers = ReflectionHelper.GetMemberPath(_sourceType, sourceMembersPath);
        PropertyMapActions.Add(pm => pm.MapFrom(sourceMembersPath, _sourceMembers));
    }
    public void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition) =>
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember, ctxt));
    public void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition) =>
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember));
    public void Condition(Func<TSource, TDestination, TMember, bool> condition) => 
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember));
    public void Condition(Func<TSource, TDestination, bool> condition) => ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest));
    public void Condition(Func<TSource, bool> condition) => ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src));
    private void ConditionCore(Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr) =>
        PropertyMapActions.Add(pm => pm.Condition = expr);
    public void PreCondition(Func<TSource, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src));
    public void PreCondition(Func<ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(ctxt));
    public void PreCondition(Func<TSource, ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src, ctxt));
    public void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src, dest, ctxt));
    private void PreConditionCore(Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr) =>
        PropertyMapActions.Add(pm => pm.PreCondition = expr);
    public void AddTransform(Expression<Func<TMember, TMember>> transformer) =>
        PropertyMapActions.Add(pm => pm.AddValueTransformation(new ValueTransformerConfiguration(pm.DestinationType, transformer)));
    public void ExplicitExpansion() => PropertyMapActions.Add(pm => pm.ExplicitExpansion = true);
    public void Ignore() => Ignore(ignorePaths: true);
    public void Ignore(bool ignorePaths)
    {
        Ignored = true;
        PropertyMapActions.Add(pm =>
        {
            pm.Ignored = true;
            if (ignorePaths && pm.TypeMap.PathMaps.Count > 0)
            {
                pm.TypeMap.IgnorePaths(DestinationMember);
            }
        });
    }
    public void AllowNull() => SetAllowNull(true);
    public void DoNotAllowNull() => SetAllowNull(false);
    private void SetAllowNull(bool value) => PropertyMapActions.Add(pm => pm.AllowNull = value);
    public void UseDestinationValue() => SetUseDestinationValue(true);
    private void SetUseDestinationValue(bool value) => PropertyMapActions.Add(pm => pm.UseDestinationValue = value);
    public void SetMappingOrder(int mappingOrder) => PropertyMapActions.Add(pm => pm.MappingOrder = mappingOrder);
    public void ConvertUsing<TValueConverter, TSourceMember>() where TValueConverter : IValueConverter<TSourceMember, TMember> => 
        ConvertUsingCore<TValueConverter, TSourceMember>();
    public void ConvertUsing<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) where TValueConverter : IValueConverter<TSourceMember, TMember> => 
        ConvertUsingCore<TValueConverter, TSourceMember>(sourceMember);
    public void ConvertUsing<TValueConverter, TSourceMember>(string sourceMemberName) where TValueConverter : IValueConverter<TSourceMember, TMember> => 
        ConvertUsingCore<TValueConverter, TSourceMember>(null, sourceMemberName);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter) => ConvertUsingCore(valueConverter);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, Expression<Func<TSource, TSourceMember>> sourceMember) =>
        ConvertUsingCore(valueConverter, sourceMember);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, string sourceMemberName)  =>
        ConvertUsingCore(valueConverter, null, sourceMemberName);
    private void ConvertUsingCore<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember = null, string sourceMemberName = null) =>
        ConvertUsingCore(new(typeof(TValueConverter), typeof(IValueConverter<TSourceMember, TMember>))
        {
            SourceMemberLambda = sourceMember,
            SourceMemberName = sourceMemberName
        });
    protected void ConvertUsingCore(ValueConverter converter) => SetResolver(converter);
    private void ConvertUsingCore<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter,
        Expression<Func<TSource, TSourceMember>> sourceMember = null, string sourceMemberName = null) =>
        ConvertUsingCore(new(valueConverter, typeof(IValueConverter<TSourceMember, TMember>))
        {
            SourceMemberLambda = sourceMember,
            SourceMemberName = sourceMemberName
        });
    public void Configure(TypeMap typeMap)
    {
        var destMember = DestinationMember;
        if(destMember.DeclaringType.ContainsGenericParameters)
        {
            destMember = typeMap.DestinationSetters.Single(m => m.Name == destMember.Name);
        }
        var propertyMap = typeMap.FindOrCreatePropertyMapFor(destMember, typeof(TMember) == typeof(object) ? destMember.GetMemberType() : typeof(TMember));
        Apply(propertyMap);
    }
    private void Apply(PropertyMap propertyMap)
    {
        foreach(var action in PropertyMapActions)
        {
            action(propertyMap);
        }
    }
    public LambdaExpression SourceExpression { get; private set; }
    public bool Ignored { get; private set; }
    public LambdaExpression GetDestinationExpression() => DestinationMember.Lambda();
    public IPropertyMapConfiguration Reverse()
    {
        var destinationType = DestinationMember.DeclaringType;
        if (_sourceMembers != null)
        {
            if (_sourceMembers.Length > 1)
            {
                return null;
            }
            var reversedMemberConfiguration = new MemberConfigurationExpression<TDestination, TSource, object>(_sourceMembers[0], destinationType);
            reversedMemberConfiguration.MapFrom(DestinationMember.Name);
            return reversedMemberConfiguration;
        }
        if (destinationType.IsGenericTypeDefinition) // .ForMember("InnerSource", o => o.MapFrom(s => s))
        {
            return null;
        }
        return PathConfigurationExpression<TDestination, TSource, object>.Create(SourceExpression, GetDestinationExpression());
    }
    public void DoNotUseDestinationValue() => SetUseDestinationValue(false);
}