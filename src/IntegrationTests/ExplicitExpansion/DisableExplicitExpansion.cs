using System.Text.RegularExpressions;

namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class DisableExplicitExpansion : IntegrationTest<DisableExplicitExpansion.DatabaseInitializer>
{
    public class Source
    {   [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 Desc { get; set; }
        public String Name { get; set; }
        public String Code { get; set; }
    }
    public class DtoWithoutOverride
    {
        public String Name { get; set; }
        public String Code { get; set; }
        public Nullable<int> Desc { get; set; }
    }

    public class DtoWithOverride {
        public String Name { get; set; }
        public String Code { get; set; }
        public Nullable<int> Desc { get; set; }
    }

    public class Context : LocalDbContext
    {
        public List<string> Log = new List<string>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(s => Log.Add(s));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Source> Sources { get; set; }

        public string GetLastSelectSqlLogEntry() => Log.LastOrDefault(_ => _.Contains("SELECT"));
    }

    private static readonly IQueryable<Source> _iq = new List<Source> {
        new Source() { Name = "Name1", Code = "Code1", Desc = -12 },
    } .AsQueryable();

    private static readonly Source _iqf = _iq.First();

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Sources.Add(_iqf);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, DtoWithOverride>()
            .ForMember(dto => dto.Desc, conf => conf.ExplicitExpansion())
            .ForMember(dto => dto.Name, conf => conf.MapFrom(src => src.Name))
            .ForMember(dto => dto.Code, conf => conf.ExplicitExpansion(false))
            .ForAllMembers(conf => conf.ExplicitExpansion())
            ;

        cfg.CreateMap<Source, DtoWithoutOverride>()
            .ForMember(dto => dto.Desc, conf => conf.ExplicitExpansion())
            .ForMember(dto => dto.Name, conf => conf.MapFrom(src => src.Name))
            .ForMember(dto => dto.Code, conf => conf.ExplicitExpansion(false))
            .ForAllMembers(conf => conf.ExplicitExpansion(overrideExpansion: false))
            ;});

    [Fact]
    public void NoExplicitExpansion() {
        using (var ctx = new Context()) {
            var dto = ProjectTo<DtoWithOverride>(ctx.Sources).ToList().First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");

            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name)); dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Code)); dto.Code.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc)); dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void OnlyExplicitExpansionForCode()
    {
        using (var ctx = new Context())
        {
            var dto = ProjectTo<DtoWithoutOverride>(ctx.Sources).ToList().First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Code));      dto.Code.ShouldNotBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void ProjectNoExplicit() {
        using (var ctx = new Context()) {
            var dto = ProjectTo<DtoWithOverride>(ctx.Sources, null, _ => _.Name).First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            
            dto.Name.ShouldBe(_iqf.Name); sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Name));
            dto.Code.ShouldBeNull(_iqf.Code); sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Code));
            dto.Desc.ShouldBeNull(); sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));
        }
    }

    [Fact]
    public void ProjectWithCodeImplicit() {
        using (var ctx = new Context()) {
            var dto = ProjectTo<DtoWithoutOverride>(ctx.Sources, null, _ => _.Name).First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");

            dto.Name.ShouldBe(_iqf.Name); sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Name));
            dto.Code.ShouldBe(_iqf.Code); sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Code));
            dto.Desc.ShouldBeNull(); sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));
        }
    }

}

public class ConstructorNoExplicitExpansion : IntegrationTest<ConstructorNoExplicitExpansion.DatabaseInitializer> {
    public class Entity {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    record Dto(string Name) { }
    public class Context : LocalDbContext {
        public DbSet<Entity> Entities { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context> {
        protected override void Seed(Context context) {
            context.Entities.Add(new() { Name = "Name" });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateProjection<Entity, Dto>().ForCtorParam("Name", o => o.ExplicitExpansion(false)));
    [Fact]
    public void Should_work() {
        using var context = new Context();
        var dto = ProjectTo<Dto>(context.Entities).Single();
        dto.Name.ShouldBe("Name");
        dto = ProjectTo<Dto>(context.Entities, null, d => d.Name).Single();
        dto.Name.ShouldBe("Name");
    }
}