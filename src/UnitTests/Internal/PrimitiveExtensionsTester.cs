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

        [Fact]
        public void Should_not_flag_only_enumerable_type_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(string)).ShouldBeFalse();
        }

        [Fact]
        public void Should_flag_list_as_writable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(int[])).ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_generic_list_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(List<int>)).ShouldBeTrue();
        }

        [Fact]
        public void Should_flag_dictionary_as_writeable_collection()
        {
            PrimitiveHelper.IsListOrDictionaryType(typeof(Dictionary<string, int>)).ShouldBeTrue();
        }
    }
}