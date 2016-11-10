using System.Collections;
using System.Collections.Generic;
using Xunit;
using Should;

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
            typeof(DestinationClass).GetFieldOrProperty("Value").ShouldNotBeNull();
        }

        [Fact]
        public void Should_not_flag_only_enumerable_type_as_writeable_collection()
        {
            typeof(string).IsListOrDictionaryType().ShouldBeFalse();
        }

        [Fact]
        public void Should_flag_list_as_writable_collection()
        {
            typeof(int[]).IsListOrDictionaryType().ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_generic_list_as_writeable_collection()
        {
            typeof(List<int>).IsListOrDictionaryType().ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_dictionary_as_writeable_collection()
        {
            typeof(Dictionary<string, int>).IsListOrDictionaryType().ShouldBeTrue();
        }
    }
}