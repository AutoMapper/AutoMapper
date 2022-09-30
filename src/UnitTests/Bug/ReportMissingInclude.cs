namespace AutoMapper.UnitTests.Bug;

public class ReportMissingInclude
{
    [Fact]
    public void ShouldDiscoverMissingMappingsInIncludedType()
    {
        new Action(() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<object, BaseType>().Include<object, ChildType>();
        })).ShouldThrowException<InvalidOperationException>(ex => ex.Message.ShouldStartWith($"Missing map from {typeof(object)} to {typeof(ChildType)}."));
    }

    public class BaseType { }

    public class ChildType : BaseType
    {
        public string Value { get; set; }
    }
}

public class ReportMissingIncludeCreateMissingMap
{
    [Fact]
    public void ShouldDiscoverMissingMappingsInIncludedType()
    {
        new Action(() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReportMissingIncludeCreateMissingMap, BaseType>().Include<ReportMissingIncludeCreateMissingMap, ChildType>();
        })).ShouldThrowException<InvalidOperationException>(ex => ex.Message.ShouldStartWith($"Missing map from {typeof(ReportMissingIncludeCreateMissingMap)} to {typeof(ChildType)}."));
    }

    public class BaseType { }

    public class ChildType : BaseType
    {
        public string Value { get; set; }
    }
}

public class ReportMissingIncludeBase
{
    [Fact]
    public void ShouldDiscoverMissingMappingsInIncludedType()
    {
        new Action(() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<object, ChildType>().IncludeBase<object, BaseType>();
        })).ShouldThrowException<InvalidOperationException>(ex => ex.Message.ShouldStartWith($"Missing map from {typeof(object)} to {typeof(BaseType)}."));
    }

    public class BaseType { }

    public class ChildType : BaseType
    {
        public string Value { get; set; }
    }
}

public class ReportMissingIncludeBaseCreateMissingMap
{
    [Fact]
    public void ShouldDiscoverMissingMappingsInIncludedType()
    {
        new Action(() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReportMissingIncludeBaseCreateMissingMap, ChildType>().IncludeBase<ReportMissingIncludeBaseCreateMissingMap, BaseType>();
        })).ShouldThrowException<InvalidOperationException>(ex => ex.Message.ShouldStartWith($"Missing map from {typeof(ReportMissingIncludeBaseCreateMissingMap)} to {typeof(BaseType)}."));
    }

    public class BaseType { }

    public class ChildType : BaseType
    {
        public string Value { get; set; }
    }
}