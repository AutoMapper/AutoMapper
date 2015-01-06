using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Query
{
    public class StringPropertyMethodsCall : AutoMapperSpecBase
    {
        private IQueryable<Dest> _dests;

        class Source
        {
            public string Name { get; set; }
        }

        class Dest
        {
            public Dest(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>();
        }

        protected override void Because_of()
        {
            var destList = new[]
            {
                new Dest("Luke Skywalker"), 
                new Dest("Princess Leia"),
                new Dest("Han Solo"), 
                new Dest("Chewbacca"),
                new Dest("Padmé Amidala"),
                new Dest("Darth Vader"), 
                new Dest("Yoda"),
            };
            _dests = new Source[0].AsQueryable()
                .Where(s => s.Name.Contains(" "))
                .OrderBy(s => s.Name.Substring(s.Name.IndexOf(" ")))
                .Map<Source, Dest>(destList.AsQueryable());
        }

        [Fact]
        public void Should_contain_only_split_names_and_ordered_by_second_name()
        {
            _dests.Count().ShouldEqual(5);
            _dests.First().Name.ShouldEqual("Padmé Amidala");
        }
    }

    public class StringPropertyMethodsCall_ForNestedProperties : AutoMapperSpecBase
    {

        private IQueryable<Dest> _dests;

        class Source
        {
            public SourcePerson Person { get; set; }
        }

        class SourcePerson
        {
            public string Name { get; set; }
        }

        class Dest
        {
            public Dest(string name)
            {
                Person = new DestPerson { Name = name };
            }

            public DestPerson Person { get; set; }
        }
        class DestPerson
        {
            public string Name { get; set; }
        }
        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>();
            Mapper.CreateMap<SourcePerson, DestPerson>();
        }

        protected override void Because_of()
        {
            var destList = new[]
            {
                new Dest("Luke Skywalker"), 
                new Dest("Princess Leia"),
                new Dest("Han Solo"), 
                new Dest("Chewbacca"),
                new Dest("Padmé Amidala"),
                new Dest("Darth Vader"), 
                new Dest("Yoda"),
            };
            _dests = new Source[0].AsQueryable()
                .Where(s => s.Person.Name.Contains(" "))
                .OrderBy(s => s.Person.Name.Substring(s.Person.Name.IndexOf(" ")))
                .Map<Source, Dest>(destList.AsQueryable());
        }

        [Fact]
        public void Should_contain_only_split_names_and_ordered_by_second_name()
        {
            _dests.Count().ShouldEqual(5);
            _dests.First().Person.Name.ShouldEqual("Padmé Amidala");
        }
    }
}
