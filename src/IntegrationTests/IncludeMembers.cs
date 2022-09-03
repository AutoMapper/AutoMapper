namespace AutoMapper.IntegrationTests;

public class IncludeMembers : IntegrationTest<IncludeMembers.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s=>s.InnerSource, s=>s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using(var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}
public class IncludeMembersExplicitExpansion : IntegrationTest<IncludeMembersExplicitExpansion.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.ExplicitExpansion());
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.ExplicitExpansion());
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources, null, d=>d.Title);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBeNull();
            result.Title.ShouldBe("title");
        }
    }
}

public class IncludeMembersFirstOrDefault : IntegrationTest<IncludeMembersFirstOrDefault.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}

public class IncludeMembersFirstOrDefaultWithMapFromExpression : IntegrationTest<IncludeMembersFirstOrDefaultWithMapFromExpression.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { Description1 = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title1 = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom(s => s.Description1));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom(s => s.Title1));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultWithSubqueryMapFrom : IntegrationTest<IncludeMembersFirstOrDefaultWithSubqueryMapFrom.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetails.FirstOrDefault()));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.FirstOrDefault()));
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 4);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersSelectFirstOrDefaultWithSubqueryMapFrom : IntegrationTest<IncludeMembersSelectFirstOrDefaultWithSubqueryMapFrom.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } }
                },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault(), s => s.OtherInnerSources.Select(s=>s).FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault()));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 4);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersFirstOrDefault : IntegrationTest<SubqueryMapFromWithIncludeMembersFirstOrDefault.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectFirstOrDefault : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectFirstOrDefault.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.Select(s => s).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefault : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefault.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } }
                },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersWithMapFromExpression : IntegrationTest<IncludeMembersWithMapFromExpression.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.MapFrom(s=>s.Description1));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.MapFrom(s=>s.Title1));
    });
    [Fact]
    public void Should_flatten_with_MapFrom()
    {
        using(var context = new Context())
        {
            var result = ProjectTo<Destination>(context.Sources).Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}

public class IncludeMembersWithNullSubstitute : IntegrationTest<IncludeMembersWithNullSubstitute.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
    });
    [Fact]
    public void Should_flatten()
    {
        using(var context = new Context())
        {
            var result = ProjectTo<Destination>(context.Sources).Single();
            result.Name.ShouldBe("name");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class IncludeMembersMembersFirstOrDefaultWithNullSubstitute : IntegrationTest<IncludeMembersMembersFirstOrDefaultWithNullSubstitute.DatabaseInitializer>
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class CascadedIncludeMembers : IntegrationTest<CascadedIncludeMembers.DatabaseInitializer>
{
    public class Source
    {
        public int Id{ get; set; }
        public Level1 FieldLevel1{ get; set; }
    }
    public class Level1
    {
        public int Id{ get; set; }
        public Level2 FieldLevel2{ get; set; }
    }
    public class Level2
    {
        public int Id{ get; set; }
        public long TheField{ get; set; }
    }
    public class Destination
    {
        public int Id{ get; set; }
        public long TheField{ get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.FieldLevel1);
        cfg.CreateProjection<Level1, Destination>(MemberList.None).IncludeMembers(s => s.FieldLevel2);
        cfg.CreateProjection<Level2, Destination>(MemberList.None);
    });
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { FieldLevel1 = new Level1 { FieldLevel2 = new Level2 { TheField = 2 } } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    [Fact]
    public void Should_flatten()
    {
        using (var context = new Context())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            var result = projectTo.Single();
            result.Id.ShouldBe(1);
            result.TheField.ShouldBe(2);
        }
    }
}