namespace AutoMapper.UnitTests.MappingInheritance
{
    using System;
    using Shouldly;
    using Xunit;
    public class MapToBaseClass : AutoMapperSpecBase
    {
        A _destination;

        public class Input { }
        public class A { }
        public class B : A { }

        protected override MapperConfiguration CreateConfiguration() => new(c =>
        {
            c.CreateMap<Input, A>().Include<Input, B>();
            c.CreateMap<Input, B>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<A>(new Input());
        }

        [Fact]
        public void ShouldReturnBaseClass()
        {
            _destination.ShouldBeOfType<A>();
        }
    }
    public class OverrideInclude : AutoMapperSpecBase
    {
        class Source
        {
        }
        class Destination
        {
        }
        class SourceDerived : Source
        {
        }
        class DestinationDerived : Destination
        {
        }
        protected override MapperConfiguration CreateConfiguration() => new(c =>
        {
            c.CreateMap<Source, Destination>().Include<SourceDerived, DestinationDerived>();
            c.CreateMap<SourceDerived, DestinationDerived>();
            c.CreateMap<SourceDerived, Destination>();
        });
        [Fact]
        public void ExplicitMapShouldApply() => Map<Destination>(new SourceDerived()).ShouldBeOfType<Destination>();
    }
    public class IncludeAs : AutoMapperSpecBase
    {
        class Source
        {
        }
        abstract class Destination
        {
        }
        class SourceDerived : Source
        {
        }
        class DestinationDerived : Destination
        {
        }
        class DestinationConcrete : Destination { }
        protected override MapperConfiguration CreateConfiguration() => new(c =>
        {
            c.CreateMap<Source, Destination>().Include<SourceDerived, DestinationDerived>();
            c.CreateMap<SourceDerived, Destination>().As<DestinationConcrete>();
            c.CreateMap<SourceDerived, DestinationDerived>();
            c.CreateMap<SourceDerived, DestinationConcrete>();
        });
        [Fact]
        public void RedirectedMapShouldApply() => Map<Destination>(new SourceDerived()).ShouldBeOfType<DestinationConcrete>();
    }
}