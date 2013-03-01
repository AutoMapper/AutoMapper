using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace NullableConverterBug
    {
        namespace AutoMapperIssue
        {
            using System;
            using System.Collections.Generic;
            using AutoMapper;
            public class TestProblem
            {
                [Fact]
                public void Example()
                {
                    Mapper.CreateMap<int?, Entity>()
                            .ConvertUsing<NullableIntToEntityConverter>();

                    Mapper.CreateMap<int, Entity>()
                            .ConvertUsing<IntToEntityConverter>();

                    var guids = new List<int?>()
                    {
                        1,
                        2,
                        null
                    };

                    var result = Mapper.Map<List<Entity>>(guids);

                    result[2].ShouldBeNull();
                }
            }

            public class IntToEntityConverter : TypeConverter<int, Entity>
            {
                protected override Entity ConvertCore(int source)
                {
                    return new Entity() { Id = source };
                }
            }

            public class NullableIntToEntityConverter : TypeConverter<int?, Entity>
            {
                protected override Entity ConvertCore(int? source)
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