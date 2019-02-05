using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.IMappingExpression
{
    public class IncludeMembers : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s=>s.InnerSource, s=>s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource{ Description = "description" }, OtherInnerSource = new OtherInnerSource{ Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersWithMapFromExpression : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public string Name { get; set; }
            public string Description1 { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title1 { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.MapFrom(s=>s.Description1));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.MapFrom(s=>s.Title1));
        });
        [Fact]
        public void Should_flatten_with_MapFrom()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersWithNullSubstitute : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.NullSubstitute("description"));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o => o.NullSubstitute("title"));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name" };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersWithMapFromFunc : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public string Name { get; set; }
            public string Description1 { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title1 { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom((s, d) => s.Description1));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom((s, d) => s.Title1));
        });
        [Fact]
        public void Should_flatten_with_MapFrom()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersWithResolver : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public string Name { get; set; }
            public string Description1 { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title1 { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom<DescriptionResolver>());
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom<TitleResolver>());
        });
        [Fact]
        public void Should_flatten_with_MapFrom()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }

        private class DescriptionResolver : IValueResolver<InnerSource, Destination, string>
        {
            public string Resolve(InnerSource source, Destination destination, string destMember, ResolutionContext context) => source.Description1;
        }

        private class TitleResolver : IValueResolver<OtherInnerSource, Destination, string>
        {
            public string Resolve(OtherInnerSource source, Destination destination, string destMember, ResolutionContext context) => source.Title1;
        }
    }
}