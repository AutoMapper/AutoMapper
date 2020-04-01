using System.Collections;
using System.Collections.Generic;
using AutoMapper.Configuration.Internal;
using Xunit;
using Shouldly;

namespace AutoMapper.UnitTests
{
    using Configuration;

    public class PrimitiveExtensionsTester
    {
        interface Interface
        {
            int Value { get; }
        }

        class DestinationClass : Interface
        {
            int Interface.Value { get { return 123; } }

            public int PrivateProperty { get; private set; }

            public int PublicProperty { get; set; }
        }

        [Fact]
        public void Should_find_explicitly_implemented_member()
        {
            PrimitiveHelper.GetFieldOrProperty(typeof(DestinationClass), "Value").ShouldNotBeNull();
        }
    }
}