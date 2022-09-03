namespace AutoMapper.UnitTests.Bug;

public class CollectionMapperMapsIEnumerableToISetIncorrectly
{
    public class TypeWithStringProperty
    {
        public string Value { get; set; }
    }

    public class SourceWithIEnumerable
    {
        public IEnumerable<TypeWithStringProperty> Stuff { get; set; }
    }

    public class TargetWithISet
    {
        public ISet<string> Stuff { get; set; }
    }

    [Fact]
    public void ShouldMapToNewISet()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<SourceWithIEnumerable, TargetWithISet>()
                .ForMember(dest => dest.Stuff, opt => opt.MapFrom(src => src.Stuff.Select(s => s.Value))));

        var source = new SourceWithIEnumerable
        {
            Stuff = new[]
                        {
                            new TypeWithStringProperty { Value = "Microphone" }, 
                            new TypeWithStringProperty { Value = "Check" }, 
                            new TypeWithStringProperty { Value = "1, 2" }, 
                            new TypeWithStringProperty { Value = "What is this?" }
                        }
        };

        var target = config.CreateMapper().Map<SourceWithIEnumerable, TargetWithISet>(source);
    }
}
