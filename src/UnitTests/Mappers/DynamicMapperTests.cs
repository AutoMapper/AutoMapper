using System;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using Should;
using Should.Core.Assertions;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    class Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class When_mapping_to_dynamic : NonValidatingSpecBase
    {
        dynamic _destination;

        protected override void Because_of()
        {
            _destination = Mapper.Map<ExpandoObject>(new Destination { Foo = "Foo", Bar = "Bar" });
        }

        [Fact]
        public void Should_map_source_properties()
        {
            Assert.Equal("Foo", _destination.Foo);
            Assert.Equal("Bar", _destination.Bar);
        }
    }

    public class When_mapping_from_dynamic : NonValidatingSpecBase
    {
        Destination _destination;

        protected override void Because_of()
        {
            dynamic source = new ExpandoObject();
            source.Foo = "Foo";
            source.Bar = "Bar";
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_destination_properties()
        {
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldEqual("Bar");
        }
    }

    public class When_mapping_from_dynamic_with_missing_property : NonValidatingSpecBase
    {
        Destination _destination;

        protected override void Because_of()
        {
            dynamic source = new ExpandoObject();
            source.Foo = "Foo";
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_existing_properties()
        {
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldBeNull();
        }
    }

    public class When_mapping_dynamic_from_expando_to_expando: NonValidatingSpecBase
    {
        dynamic _destination;

        protected override void Because_of()
        {
            dynamic source = new ExpandoObject();
            source.Foo = "Foo";
            source.Bar = "Bar";
            _destination = Mapper.Map<ExpandoObject>(source);
        }

        [Fact]
        public void Should_map_as_dictionary()
        {
            Assert.Equal("Foo", _destination.Foo);
            Assert.Equal("Bar", _destination.Bar);
        }
    }
}