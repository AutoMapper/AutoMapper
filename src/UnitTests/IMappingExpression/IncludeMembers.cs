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
        public void Should_flatten()
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
        public void Should_flatten()
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
        public void Should_flatten()
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

    public class IncludeMembersWithMemberResolver : AutoMapperSpecBase
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom<DescriptionResolver,string>(s=>s.Description1));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom<TitleResolver,string>("Title1"));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }

        private class DescriptionResolver : IMemberValueResolver<InnerSource, Destination, string, string>
        {
            public string Resolve(InnerSource source, Destination destination, string sourceMember, string destMember, ResolutionContext context) => sourceMember;
        }

        private class TitleResolver : IMemberValueResolver<OtherInnerSource, Destination, string, string>
        {
            public string Resolve(OtherInnerSource source, Destination destination, string sourceMember, string destMember, ResolutionContext context) => sourceMember;
        }
    }
    public class IncludeMembersWithValueConverter : AutoMapperSpecBase
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.ConvertUsing<ValueConverter, string>(s => s.Description1));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.ConvertUsing<ValueConverter, string>("Title1"));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }

        private class ValueConverter : IValueConverter<string, string>
        {
            public string Convert(string sourceMember, ResolutionContext context) => sourceMember;
        }
    }

    public class IncludeMembersWithConditions : AutoMapperSpecBase
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.Condition((s, d, sm, dm, c) => false));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.Condition((s, d, sm, dm, c) => true));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBeNull();
            destination.Title.ShouldBe("title");
        }
    }
    public class IncludeMembersWithPreConditions : AutoMapperSpecBase
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.PreCondition((s, d, c) => false));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.PreCondition((s, d, c) => true));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBeNull();
            destination.Title.ShouldBe("title");
        }
    }
    public class IncludeMembersCycle : AutoMapperSpecBase
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
            public Source Parent { get; set; }
        }
        class OtherInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public Source Parent { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public Destination Parent { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).IncludeMembers(s=>s.Parent);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).IncludeMembers(s=>s.Parent);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            source.InnerSource.Parent = source;
            source.OtherInnerSource.Parent = source;
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
            destination.Parent.ShouldBe(destination);
        }
    }
    public class IncludeMembersReverseMap : AutoMapperSpecBase
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
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource).ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_unflatten()
        {
            var source = Mapper.Map<Source>(new Destination { Description = "description", Name = "name", Title = "title" });
            source.Name.ShouldBe("name");
            source.InnerSource.Name.ShouldBe("name");
            source.OtherInnerSource.Name.ShouldBe("name");
            source.InnerSource.Description.ShouldBe("description");
            source.OtherInnerSource.Description.ShouldBe("description");
            source.OtherInnerSource.Title.ShouldBe("title");
        }
    }

    public class ReverseMapToIncludeMembers : AutoMapperSpecBase
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
            cfg.CreateMap<Destination, Source>()
                .ForMember(d => d.InnerSource, o => o.MapFrom(s => s))
                .ForMember(d => d.OtherInnerSource, o => o.MapFrom(s => s))
                .ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }
    public class IncludeMembersWithAfterMap : AutoMapperSpecBase
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
        bool afterMap, beforeMap;
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).AfterMap((s,d)=>afterMap=true);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).BeforeMap((s, d, c) => beforeMap = true);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
            afterMap.ShouldBeTrue();
            beforeMap.ShouldBeTrue();
        }
    }

    public class IncludeMembersWithForPath : AutoMapperSpecBase
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
            public InnerDestination InnerDestination { get; set; }
        }
        class InnerDestination
        {
            public string Description { get; set; }
            public string Title { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForPath(d=>d.InnerDestination.Description, o=>
            {
                o.MapFrom(s => s.Description);
                o.Condition(c => true);
            });
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForPath(d=>d.InnerDestination.Title, o=>
            {
                o.MapFrom(s => s.Title);
                o.Condition(c => true);
            });
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.InnerDestination.Description.ShouldBe("description");
            destination.InnerDestination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersTransformers : AutoMapperSpecBase
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.AddTransform(s=>s+"Ex"));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.AddTransform(s => s + "Ex"));
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("descriptionEx");
            destination.Title.ShouldBe("titleEx");
        }
    }
    public class IncludeMembersInvalidExpression
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

        [Fact]
        public void Should_throw()
        {
            new MapperConfiguration(cfg =>
            {
                Assert.Throws<ArgumentOutOfRangeException>("memberExpressions", () => cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource.ToString(), s => s.OtherInnerSource));
            });
        }
    }
}