using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace AllowDuplicateCallsPerMapping
    {
        [TestFixture]
        public class When_setting_AllowDuplicateCallsPerMapping_Should_Honour_Rule : AutoMapperSpecBase
        {
            public class Source
            {
            }

            public class Destination
            {
            }

            protected override void Establish_context()
            {
            }

            protected override void Because_of()
            {
            }

            [Test, ExpectedException(typeof(DuplicateCallsPerMappingException))]
            public void Should_Dissallow_Double_Configuration()
            {
                Mapper.Initialize(config =>
                {
                    config.AllowDuplicateCallsPerMapping = false;
                    config.CreateMap<Source, Destination>();
                    config.CreateMap<Source, Destination>();
                });
            }

            [Test]
            public void Should_Allow_Double_Configuration()
            {
                Mapper.Initialize(config =>
                {
                    config.AllowDuplicateCallsPerMapping = true;
                    config.CreateMap<Source, Destination>();
                    config.CreateMap<Source, Destination>();
                });
            }

            [Test]
            public void Default_Intiailize_Should_Be_Allow()
            {
                Mapper.Initialize(config =>
                {
                    config.CreateMap<Source, Destination>();
                    config.CreateMap<Source, Destination>();
                });
            }

            [Test]
            public void Default_Should_Be_Allow()
            {
                Mapper.CreateMap<Source, Destination>();
                Mapper.CreateMap<Source, Destination>();
            }
        }
    }
}

