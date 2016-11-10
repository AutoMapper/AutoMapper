using AutoMapper;
using NUnit.Framework;

namespace AutoMapperSamples
{
    namespace ConfigurationValidation
    {
        [TestFixture]
        public class BadConfigurationThrowing
        {
            public class Source
            {
                public int SomeValue { get; set; }
            }

            public class Destination
            {
                public int SomeValuefff { get; set; }
            }

            [Test, ExpectedException(typeof(AutoMapperConfigurationException))]
            public void Example()
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

                config.AssertConfigurationIsValid(); // Throws an exception
            }

            [Test]
            public void ExampleIgnoring()
            {
                var config = new MapperConfiguration(cfg => 
                    cfg.CreateMap<Source, Destination>().ForMember(m => m.SomeValuefff, opt => opt.Ignore())
                    );

                config.AssertConfigurationIsValid();
            }
        }
    }
}