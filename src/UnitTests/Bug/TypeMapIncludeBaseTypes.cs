namespace AutoMapper.UnitTests.Bug;

public abstract class TypeMapIncludeBaseTypes
{
    public abstract class Source { public int? A { get; set; } }
    public class SourceA : Source { }
    public class SourceB : Source { }
    public abstract class Target { public int? A { get; set; } }
    public class TargetA : Target { }
    public class TargetB : Target { }

    public class IncludeFromBase : TypeMapIncludeBaseTypes
    {
        protected override IGlobalConfiguration CreateConfigurationProvider()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Target>()
                .Include<SourceA, TargetA>()
                .Include<SourceB, TargetB>();

                cfg.CreateMap<SourceA, TargetA>();

                cfg.CreateMap<SourceB, TargetB>();
            });
        }
    }

    public class IncludeFromDerived : TypeMapIncludeBaseTypes
    {
        protected override IGlobalConfiguration CreateConfigurationProvider()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Target>();

                cfg.CreateMap<SourceA, TargetA>()
                .IncludeBase<Source, Target>();

                cfg.CreateMap<SourceB, TargetB>()
                .IncludeBase<Source, Target>();
            });
        }
    }

    [Fact]
    public void TypeMap_Should_include_derivied_types()
    {
        var config = CreateConfigurationProvider();
        var typeMap = config.ResolveTypeMap(typeof(Source), typeof(Target));

        var typePairs = new[]{
            new TypePair(typeof(SourceA), typeof(TargetA)),
            new TypePair(typeof(SourceB), typeof(TargetB)),
        };

        typeMap.IncludedDerivedTypes.SequenceEqual(typePairs).ShouldBeTrue();
    }

    [Fact]
    public void TypeMap_Should_include_base_types()
    {
        var config = CreateConfigurationProvider();
        var typeMap = config.ResolveTypeMap(typeof(SourceA), typeof(TargetA));

        var typePairs = new[]{
            new TypePair(typeof(Source), typeof(Target))
        };

        typeMap.IncludedBaseTypes.SequenceEqual(typePairs).ShouldBeTrue();
    }

    protected abstract IGlobalConfiguration CreateConfigurationProvider();
}
