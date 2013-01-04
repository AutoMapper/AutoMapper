using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
    namespace NamedMappings
    {
        public class When_configuring_multiple_mappings_with_same_type_to_type_mapping_using_a_mapping_name : AutoMapperSpecBase
        {
            private Destination _result;
            private Destination _result2;
            
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    // Create default mapping
                    cfg.CreateMap<Source, Destination>();

                    // Create named mapping ignoring property Value2
                    cfg.CreateMap<Source, Destination>("namedMapping")
                        .ForMember(dest => dest.Value2, opt => opt.Ignore());
                });
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source { Value = 5, Value2 = 3 });
                _result2 = Mapper.Map<Source, Destination>(new Source { Value = 5, Value2 = 3 }, "namedMapping");
            }

            [Test]
            public void Should_use_default_mapping()
            {
                _result.Value.ShouldEqual(5);
                _result.Value2.ShouldEqual(3);
            }

            [Test]
            public void Should_use_named_mapping()
            {
                _result2.Value.ShouldEqual(5);
                _result2.Value2.ShouldEqual(default(int));
            }
        }
    }
}
