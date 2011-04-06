
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Mappers;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
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

        [Test]
        public void ShouldMapToNewISet()
        {
            var config = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.AllMappers());
            config.CreateMap<SourceWithIEnumerable, TargetWithISet>()
                  .ForMember(dest => dest.Stuff, opt => opt.MapFrom(src => src.Stuff.Select(s => s.Value)));

            config.AssertConfigurationIsValid();

            var engine = new MappingEngine(config);

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

            var target = engine.Map<SourceWithIEnumerable, TargetWithISet>(source);
        }
    }
}
