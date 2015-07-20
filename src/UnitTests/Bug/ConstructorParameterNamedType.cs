using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ConstructorParameterNamedType
    {
        public class SourceClass { }

        public class DestinationClass
        {
            public DestinationClass() { }

            public DestinationClass(int type)
            {
                Type = type;
            }

            public int Type { get; private set; }
        }

        [Fact]
        public void Should_handle_constructor_parameter_named_type()
        {
            Mapper.Initialize(c => c.CreateMap<SourceClass, DestinationClass>());
            new Action(Mapper.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>(ex=>ex.Errors[0].UnmappedPropertyNames[0].ShouldEqual("Type"));
        }
    }
}