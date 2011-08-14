using AutoMapper.Configuration;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Configuration
{
    namespace MapperConfigurationSpecs
    {
        public class when_configuring_two_flat_types : AutoMapperSpecBase
        {
            static MapperConfiguration _configuration;

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
                _configuration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Source, Destination>();
                });
            }

            [Test]
            public void should_record_the_type_map()
            {
                //_configuration.TypeMaps.Count().ShouldEqual(1);
                
            }
        }
    }
}