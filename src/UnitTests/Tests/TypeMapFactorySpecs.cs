using NBehave.Spec.NUnit;
using NUnit.Framework;
using System.Linq;

namespace AutoMapper.UnitTests.Tests
{
    public class TypeMapFactory_Context : SpecBase
    {
        protected override void Establish_context()
        {
            Factory = new TypeMapFactory();
        }

        protected TypeMapFactory Factory
        {
            get; set;
        }
    }

    public class When_constructing_type_maps_with_matching_property_names : TypeMapFactory_Context
    {
        public class Source
        {
            public int Value { get; set; }
            public int SomeOtherValue { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
            public int SomeOtherValue { get; set; }
        }

        [Test]
        public void Should_map_properties_with_same_name()
        {
            var typeMap = Factory.CreateTypeMap(typeof (Source), typeof (Destination));

            var propertyMaps = typeMap.GetPropertyMaps();
            
            propertyMaps.Count().ShouldEqual(2);
        }
    }

}