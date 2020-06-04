using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal;
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
    public class IncludeMembersWrapperFirstOrDefault : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<InnerSourceWrapper> InnerSources { get; set; } = new List<InnerSourceWrapper>();
            public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
        }
        class InnerSourceWrapper
        {
            public InnerSource InnerSource { get; set; }
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Publisher { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Publisher { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault().InnerSource, s => s.OtherInnerSources.FirstOrDefault()).ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public static void Should_null_check()
        {
            Expression<Func<Source, InnerSource>> expression = s => s.InnerSources.FirstOrDefault().InnerSource;
            var result= expression.Body.NullCheck();
        }
        [Fact]
        public void Should_flatten()
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSourceWrapper { InnerSource = new InnerSource { Description = "description", Publisher = "publisher" } } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            var destination = Mapper.Map<Destination>(source);
            var plan = Configuration.BuildExecutionPlan(typeof(Source), typeof(Destination));
            FirstOrDefaultCounter.Assert(plan, 2);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
            destination.Author.ShouldBe("author");
            destination.Publisher.ShouldBe("publisher");
        }
    }
    public class IncludeMembersFirstOrDefault : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
            public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Publisher { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Publisher { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault()).ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            var destination = Mapper.Map<Destination>(source);
            var plan = Configuration.BuildExecutionPlan(typeof(Source), typeof(Destination));
            FirstOrDefaultCounter.Assert(plan, 2);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
            destination.Author.ShouldBe("author");
            destination.Publisher.ShouldBe("publisher");
        }
    }
    public class IncludeMembersFirstOrDefaultReverseMap : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
            public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Publisher { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Publisher { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault()).ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ReverseMap();
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ReverseMap();
        });
        [Fact]
        public void Should_unflatten()
        {
            var source = Mapper.Map<Source>(new Destination { Description = "description", Name = "name", Title = "title" });
            source.Name.ShouldBe("name");
        }
    }
    public class IncludeMembersNested : AutoMapperSpecBase
    {
        class Source
        {
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public NestedInnerSource NestedInnerSource { get; set; }
        }
        class OtherInnerSource
        {
            public NestedOtherInnerSource NestedOtherInnerSource { get; set; }
        }
        class NestedInnerSource
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        class NestedOtherInnerSource
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
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource.NestedInnerSource, s => s.OtherInnerSource.NestedOtherInnerSource);
            cfg.CreateMap<NestedInnerSource, Destination>(MemberList.None);
            cfg.CreateMap<NestedOtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source
            {
                Name = "name",
                InnerSource = new InnerSource { NestedInnerSource = new NestedInnerSource { Description = "description" } },
                OtherInnerSource = new OtherInnerSource { NestedOtherInnerSource = new NestedOtherInnerSource { Title = "title" } }
            };
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ReverseMap();
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ReverseMap();
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
    public class IncludeMembersReverseMapOverride : AutoMapperSpecBase
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
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource).ReverseMap()
                .ForMember(d=>d.InnerSource, o=>o.Ignore())
                .ForMember(d=>d.OtherInnerSource, o=>o.Ignore());
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_unflatten()
        {
            var source = Mapper.Map<Source>(new Destination { Description = "description", Name = "name", Title = "title" });
            source.Name.ShouldBe("name");
            source.InnerSource.ShouldBeNull();
            source.OtherInnerSource.ShouldBeNull();
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
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ReverseMap();
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ReverseMap();
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
    public class ReverseMapToIncludeMembersOverride : AutoMapperSpecBase
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
            cfg.CreateMap<Destination, Source>(MemberList.None)
                .ForMember(d => d.InnerSource, o => o.MapFrom(s => s))
                .ForMember(d => d.OtherInnerSource, o => o.MapFrom(s => s))
                .ReverseMap()
                .IncludeMembers();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ReverseMap();
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ReverseMap();
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBeNull();
            destination.Title.ShouldBeNull();
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
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource).AddTransform<string>(s => s + "Main");
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.AddTransform(s=>s+"Extra")).AddTransform<string>(s => s + "Ex");
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.AddTransform(s => s + "Extra")).AddTransform<string>(s => s + "Ex");
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("nameMain");
            destination.Description.ShouldBe("descriptionExtraExMain");
            destination.Title.ShouldBe("titleExtraExMain");
        }
    }
    public class IncludeMembersTransformersPerMember : AutoMapperSpecBase
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
    public class IncludeMembersWithGenerics : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Source<,>), typeof(Destination), MemberList.None).IncludeMembers("InnerSource", "OtherInnerSource");
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source<InnerSource, OtherInnerSource> { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }

    public class IncludeMembersWithGenericsInvalidStrings
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
                Assert.Throws<ArgumentOutOfRangeException>(() => cfg.CreateMap(typeof(Source<,>), typeof(Destination), MemberList.None).IncludeMembers("dInnerSource", "fOtherInnerSource"));
            });
        }
    }

    public class IncludeMembersReverseMapGenerics : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Source<,>), typeof(Destination), MemberList.None).IncludeMembers("InnerSource", "OtherInnerSource").ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ReverseMap();
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ReverseMap();
        });
        [Fact]
        public void Should_unflatten()
        {
            var source = Mapper.Map<Source<InnerSource, OtherInnerSource>>(new Destination { Description = "description", Name = "name", Title = "title" });
            source.Name.ShouldBe("name");
            source.InnerSource.Name.ShouldBe("name");
            source.OtherInnerSource.Name.ShouldBe("name");
            source.InnerSource.Description.ShouldBe("description");
            source.OtherInnerSource.Description.ShouldBe("description");
            source.OtherInnerSource.Title.ShouldBe("title");
        }
    }
    public class IncludeMembersReverseMapGenericsOverride : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Source<,>), typeof(Destination), MemberList.None).IncludeMembers("InnerSource", "OtherInnerSource").ReverseMap()
                .ForMember("InnerSource", o=>o.Ignore())
                .ForMember("OtherInnerSource", o=>o.Ignore());
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_unflatten()
        {
            var source = Mapper.Map<Source<InnerSource, OtherInnerSource>>(new Destination { Description = "description", Name = "name", Title = "title" });
            source.Name.ShouldBe("name");
            source.InnerSource.ShouldBeNull();
            source.OtherInnerSource.ShouldBeNull();
        }
    }
    public class ReverseMapToIncludeMembersGenerics : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Destination), typeof(Source<,>))
                .ForMember("InnerSource", o => o.MapFrom(s => s))
                .ForMember("OtherInnerSource", o => o.MapFrom(s => s))
                .ReverseMap();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source<InnerSource, OtherInnerSource> { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBe("description");
            destination.Title.ShouldBe("title");
        }
    }
    public class ReverseMapToIncludeMembersGenericsOverride : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Destination), typeof(Source<,>))
                .ForMember("InnerSource", o => o.MapFrom(s => s))
                .ForMember("OtherInnerSource", o => o.MapFrom(s => s))
                .ReverseMap()
                .IncludeMembers();
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            var source = new Source<InnerSource, OtherInnerSource> { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            var destination = Mapper.Map<Destination>(source);
            destination.Name.ShouldBe("name");
            destination.Description.ShouldBeNull();
            destination.Title.ShouldBeNull();
        }
    }
    public class IncludeMembersSourceValidation : AutoMapperSpecBase
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
            cfg.CreateMap<Source, Destination>(MemberList.Source).IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
    }
    public class IncludeMembersWithGenericsSourceValidation : AutoMapperSpecBase
    {
        class Source<TInnerSource, TOtherInnerSource>
        {
            public string Name { get; set; }
            public TInnerSource InnerSource { get; set; }
            public TOtherInnerSource OtherInnerSource { get; set; }
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
            cfg.CreateMap(typeof(Source<,>), typeof(Destination), MemberList.Source).IncludeMembers("InnerSource", "OtherInnerSource");
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
    }
    public class IncludeMembersWithInclude : AutoMapperSpecBase
    {
        public class ParentOfSource
        {
            public Source InnerSource { get; set; }
        }
        public class Source : SourceBase
        {
        }
        public class SourceBase
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        public class Destination
        {
            public string FullName { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceBase, Destination>().ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName)).IncludeAllDerived();
            cfg.CreateMap<ParentOfSource, Destination>().IncludeMembers(src => src.InnerSource);
            cfg.CreateMap<Source, Destination>();
        });
        [Fact]
        public void Should_inherit_configuration() => Mapper.Map<Destination>(new ParentOfSource { InnerSource = new Source { FirstName = "first", LastName = "last" } }).FullName.ShouldBe("first last");
    }
    public class IncludeMembersWithIncludeDifferentOrder : AutoMapperSpecBase
    {
        public class ParentOfSource
        {
            public Source InnerSource { get; set; }
        }
        public class Source : SourceBase
        {
        }
        public class SourceBase
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        public class Destination
        {
            public string FullName { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceBase, Destination>().ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName)).IncludeAllDerived();
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<ParentOfSource, Destination>().IncludeMembers(src => src.InnerSource);
        });
        [Fact]
        public void Should_inherit_configuration() => Mapper.Map<Destination>(new ParentOfSource { InnerSource = new Source { FirstName = "first", LastName = "last" } }).FullName.ShouldBe("first last");
    }
    public class IncludeMembersWithIncludeBase : AutoMapperSpecBase
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
        }
        public class Address
        {
            public string Line1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CustomerDtoBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AddressLine1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CreateCustomerDto : CustomerDtoBase
        {
            public string CreatedBy { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<Customer, CustomerDtoBase>().IncludeMembers(x => x.Address) .ForMember(m => m.Id, o => o.Ignore());
            cfg.CreateMap<Address, CustomerDtoBase>(MemberList.None).ForMember(m => m.AddressLine1, o => o.MapFrom(x => x.Line1));
            cfg.CreateMap<Customer, CreateCustomerDto>().IncludeBase<Customer, CustomerDtoBase>().ForMember(m => m.CreatedBy, o => o.Ignore());
        });
        [Fact]
        public void Should_inherit_IncludeMembers() => Mapper.Map<CreateCustomerDto>(new Customer { Address = new Address { Postcode = "Postcode" } }).Postcode.ShouldBe("Postcode");
    }
    public class IncludeMembersWithIncludeBaseOverride : AutoMapperSpecBase
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
            public Address NewAddress { get; set; }
        }
        public class Address
        {
            public string Line1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CustomerDtoBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AddressLine1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CreateCustomerDto : CustomerDtoBase
        {
            public string CreatedBy { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDtoBase>().IncludeMembers(x => x.Address).ForMember(m => m.Id, o => o.Ignore());
            cfg.CreateMap<Address, CustomerDtoBase>(MemberList.None).ForMember(m => m.AddressLine1, o => o.MapFrom(x => x.Line1));
            cfg.CreateMap<Address, CreateCustomerDto>(MemberList.None).IncludeBase<Address, CustomerDtoBase>();
            cfg.CreateMap<Customer, CreateCustomerDto>().IncludeMembers(s => s.NewAddress).IncludeBase<Customer, CustomerDtoBase>().ForMember(m => m.CreatedBy, o => o.Ignore());
        });
        [Fact]
        public void Should_override_IncludeMembers() => Mapper.Map<CreateCustomerDto>(new Customer { NewAddress = new Address { Postcode = "Postcode" } }).Postcode.ShouldBe("Postcode");
    }
    public class IncludeMembersWithIncludeBaseOverrideMapFrom : AutoMapperSpecBase
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
        }
        public class Address
        {
            public string Line1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CustomerDtoBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AddressLine1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CreateCustomerDto : CustomerDtoBase
        {
            public string CreatedBy { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDtoBase>().IncludeMembers(x => x.Address).ForMember(m => m.Id, o => o.Ignore());
            cfg.CreateMap<Address, CustomerDtoBase>(MemberList.None).ForMember(m => m.AddressLine1, o => o.MapFrom(x => x.Line1));
            cfg.CreateMap<Customer, CreateCustomerDto>()
                .IncludeBase<Customer, CustomerDtoBase>()
                .ForMember(d=>d.Postcode, o=>o.MapFrom((s, d)=>s.Name))
                .ForMember(m => m.CreatedBy, o => o.Ignore());
        });
        [Fact]
        public void Should_override_IncludeMembers() => Mapper.Map<CreateCustomerDto>(new Customer { Name = "Postcode", Address = new Address() }).Postcode.ShouldBe("Postcode");
    }
    public class IncludeMembersWithIncludeBaseOverrideConvention : AutoMapperSpecBase
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
        }
        public class NewCustomer : Customer
        {
            public string Postcode { get; set; }
        }
        public class Address
        {
            public string Line1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CustomerDtoBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AddressLine1 { get; set; }
            public string Postcode { get; set; }
        }
        public class CreateCustomerDto : CustomerDtoBase
        {
            public string CreatedBy { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDtoBase>().IncludeMembers(x => x.Address).ForMember(m => m.Id, o => o.Ignore());
            cfg.CreateMap<Address, CustomerDtoBase>(MemberList.None).ForMember(m => m.AddressLine1, o => o.MapFrom(x => x.Line1));
            cfg.CreateMap<NewCustomer, CreateCustomerDto>().IncludeBase<Customer, CustomerDtoBase>().ForMember(m => m.CreatedBy, o => o.Ignore());
        });
        [Fact]
        public void Should_override_IncludeMembers() => Mapper.Map<CreateCustomerDto>(new NewCustomer { Postcode = "Postcode", Address = new Address() }).Postcode.ShouldBe("Postcode");
    }
    public class IncludeMembersWithValueTypeValidation : AutoMapperSpecBase
    {
        class Source
        {
            public InnerSource InnerSource { get; set; }
        }
        struct InnerSource
        {
            public string Name { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
        });
    }
}