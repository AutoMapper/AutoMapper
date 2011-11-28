using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
    namespace ReverseMapping
    {
        public class When_reverse_mapping_classes_with_simple_properties : AutoMapperSpecBase
        {
            private Source _source;

            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ReverseMap();
                });
            }

            protected override void Because_of()
            {
                var dest = new Destination
                {
                    Value = 10
                };
                _source = Mapper.Map<Destination, Source>(dest);
            }

            [Test]
            public void Should_create_a_map_with_the_reverse_items()
            {
                _source.Value.ShouldEqual(10);
            }
        }
    }
}