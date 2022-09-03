namespace AutoMapper.UnitTests.Bug
{
    namespace NullableConverterBug
    {
        namespace AutoMapperIssue
        {
            public class TestProblem
            {
                [Fact]
                public void Example()
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<int?, Entity>()
                            .ConvertUsing<NullableIntToEntityConverter>();

                        cfg.CreateMap<int, Entity>()
                            .ConvertUsing<IntToEntityConverter>();
                    });

                    var guids = new List<int?>()
                    {
                        1,
                        2,
                        null
                    };

                    var result = config.CreateMapper().Map<List<Entity>>(guids);

                    result[2].ShouldBeNull();
                }
            }

            public class IntToEntityConverter : ITypeConverter<int, Entity>
            {
                public Entity Convert(int source, Entity destination, ResolutionContext context)
                {
                    return new Entity() { Id = source };
                }
            }

            public class NullableIntToEntityConverter : ITypeConverter<int?, Entity>
            {
                public Entity Convert(int? source, Entity destination, ResolutionContext context)
                {
                    if (source.HasValue)
                    {
                        return new Entity() { Id = source.Value };
                    }

                    return null;
                }
            }

            public class Entity
            {
                public int Id { get; set; }

                public override string ToString()
                {
                    return Id.ToString();
                }
            }
        }
    }
}