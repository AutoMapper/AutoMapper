using NUnit.Framework;

namespace AutoMapper.UnitTests.MappingInheritance
{
    [TestFixture]
    public class BeforeAfterMapShouldBeInherited
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }

        public class ModelObject
        {
            public string CalulateFrom { get; set; }
        }

        public class ModelSubObject : ModelObject
        {
            
        }

        public class DtoObject
        {
            public string CalculatedProp { get; set; }
        }

        public class DtoSubObject : DtoObject
        {
            
        }

        [Test]
        public void after_map_should_be_inherited_in_specific_mappings()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .AfterMap((m, d) =>
                              {
                                  d.CalculatedProp = m.CalulateFrom;
                              })
                .Include<ModelSubObject,DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            var result = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject {CalulateFrom = "Test"});

            Assert.AreEqual("Test", result.CalculatedProp);
        }

        [Test]
        public void before_map_should_be_inherited_in_specific_mappings()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .BeforeMap((m, d) =>
                {
                    d.CalculatedProp = m.CalulateFrom;
                })
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            var result = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject { CalulateFrom = "Test" });

            Assert.AreEqual("Test", result.CalculatedProp);
        }
    }
}
