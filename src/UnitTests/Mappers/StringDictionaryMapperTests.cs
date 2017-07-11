using System;
using Shouldly;
using Xunit;
using StringDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace AutoMapper.UnitTests.Mappers
{
    class Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
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
            _destination["Foo"].ShouldBe("Foo");
            _destination["Bar"].ShouldBe("Bar");
        }
    }

    public class When_mapping_from_StringDictionary : NonValidatingSpecBase
    {
        Destination _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_destination_properties()
        {
            _destination.Foo.ShouldBe("Foo");
            _destination.Bar.ShouldBe("Bar");
        }
    }

    public class When_mapping_struct_from_StringDictionary : NonValidatingSpecBase
    {
        Destination _destination;

        struct Destination
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_destination_properties()
        {
            _destination.Foo.ShouldBe("Foo");
            _destination.Bar.ShouldBe("Bar");
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
            _destination.Foo.ShouldBe("Foo");
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
            _destination["Foo"].ShouldBe("Foo");
            _destination["Bar"].ShouldBe("Bar");
        }
    }

    public class When_mapping_from_StringDictionary_to_existing_destination : AutoMapperSpecBase
    {
        public abstract class SomeBase
        {
            protected int _x = 100;
            public abstract int X { get; }
            protected int _y = 200;
            public abstract int Y { get; }
        }

        public class SomeBody : SomeBase
        {
            public override int X { get { return _x + 10; } }

            public override int Y { get { return _y + 20; } }
            private int _z = 300;
            public int Z { get { return _z + 30; } }
        }

        public class SomeOne : SomeBase
        {
            public override int X { get { return _x - 10; } }

            public override int Y { get { return _y - 20; } }
            private int _a = 300;
            public int A { get { return _a - 30; } }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c => c.CreateMap<SomeBase, SomeBase>());

        [Fact]
        public void Should_map_ok()
        {
            SomeBase someBase = new SomeBody();
            var someOne = new StringDictionary();

            Mapper.Map(someOne, someBase);
        }
    }

    public class When_mapping_from_StringDictionary_to_abstract_type : AutoMapperSpecBase
    {
        public abstract class SomeBase
        {
            protected int _x = 100;
            public abstract int X { get; }
            protected int _y = 200;
            public abstract int Y { get; }
        }

        public class SomeBody : SomeBase
        {
            public override int X { get { return _x + 10; } }

            public override int Y { get { return _y + 20; } }
            private int _z = 300;
            public int Z { get { return _z + 30; } }
        }

        public class SomeOne : SomeBase
        {
            public override int X { get { return _x - 10; } }

            public override int Y { get { return _y - 20; } }
            private int _a = 300;
            public int A { get { return _a - 30; } }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c => c.CreateMap<SomeBase, SomeBase>());

        [Fact]
        public void Should_throw()
        {
            new Action(()=>Mapper.Map<SomeBase>(new StringDictionary()))
                .ShouldThrowException<AutoMapperMappingException>(ex=>
                    ex.InnerException.Message.ShouldStartWith($"Cannot create an instance of abstract type {typeof(SomeBase)}."));

        }
    }
}