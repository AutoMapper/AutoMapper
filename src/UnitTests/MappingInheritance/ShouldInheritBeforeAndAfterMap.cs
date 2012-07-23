using AutoMapper.Mappers;
using NUnit.Framework;

namespace AutoMapper.UnitTests.MappingInheritance
{
    [TestFixture]
    public class ShouldInheritBeforeAndAfterMap
    {
        public class BaseClass
        {
            public string Prop { get; set; }
        } 
        public class Class : BaseClass {}

        public class BaseDto
        {
            public string DifferentProp { get; set; }            
        }
        public class Dto : BaseDto {}

        [Test]
        public void should_inherit_base_beforemap()
        {
            // arrange
            var source = new Class{ Prop = "test" };
            var configurationProvider = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.AllMappers());
            configurationProvider
                .CreateMap<BaseClass, BaseDto>()
                .BeforeMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            configurationProvider.CreateMap<Class, Dto>();
            var mappingEngine = new MappingEngine(configurationProvider);

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            Assert.AreEqual("test", dest.DifferentProp);
        }

        [Test]
        public void should_inherit_base_aftermap()
        {
            // arrange
            var source = new Class { Prop = "test" };
            var configurationProvider = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.AllMappers());
            configurationProvider
                .CreateMap<BaseClass, BaseDto>()
                .AfterMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            configurationProvider.CreateMap<Class, Dto>();
            var mappingEngine = new MappingEngine(configurationProvider);

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            Assert.AreEqual("test", dest.DifferentProp);
        }
    }
}