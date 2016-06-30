﻿using Should;
using Xunit;
using StringDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace AutoMapper.UnitTests.Mappers
{
    class Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }

        internal string Jack { get; set; }
    }

    public class When_mapping_to_StringDictionary : NonValidatingSpecBase
    {
        StringDictionary _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            _destination = Mapper.Map<StringDictionary>(new Destination { Foo = "Foo", Bar = "Bar" });
        }

        [Fact]
        public void Should_map_source_properties()
        {
            _destination.Count.ShouldEqual(2);
            _destination["Foo"].ShouldEqual("Foo");
            _destination["Bar"].ShouldEqual("Bar");
        }
    }

    public class When_mapping_from_StringDictionary : NonValidatingSpecBase
    {
        Destination _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" }, { "Jack", "Jack" } };
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_destination_properties()
        {
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldEqual("Bar");
            _destination.Jack.ShouldBeNull();
        }
    }

    public class When_mapping_from_StringDictionary_with_missing_property : NonValidatingSpecBase
    {
        Destination _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var source = new StringDictionary() { { "Foo", "Foo" } };
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_existing_properties()
        {
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldBeNull();
        }
    }

    public class When_mapping_from_StringDictionary_to_StringDictionary: NonValidatingSpecBase
    {
        StringDictionary _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
            _destination = Mapper.Map<StringDictionary>(source);
        }

        [Fact]
        public void Should_map()
        {
            _destination["Foo"].ShouldEqual("Foo");
            _destination["Bar"].ShouldEqual("Bar");
        }
    }
}