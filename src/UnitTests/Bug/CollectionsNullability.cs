using Xunit;
using Should;
using System;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class CollectionsNullability : AutoMapperSpecBase
    {
        Holder _destination;

        public class Container
        {
            public List<string> Items { get; set; }
        }

        class Holder
        {
            public Container[] Containers { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Holder, Holder>();
            Mapper.CreateMap<Container, Container>()
                //.ForMember(d =>d.Items, o=>o.NullSubstitute(new List<string>()))
                //.ForMember(d => d.Items, o => o.MapFrom(s => s.Items ?? new List<string>()));
                ;
        }

        protected override void Because_of()
        {
            var from = new Holder { Containers = new[] { new Container() } };
            _destination = Mapper.Map<Holder>(from);
        }

        [Fact]
        public void Should_map_null_collection_to_not_null()
        {
            _destination.Containers[0].Items.ShouldNotBeNull();
        }
    }
}