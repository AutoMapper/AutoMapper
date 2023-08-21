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
    public class Dto
    {
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
        cfg.CreateMap<Source, Dto>()
            .ForMember(dto => dto.Code, conf => conf.ExplicitExpansion(false))
            .ForAllMembers(conf => conf.ExplicitExpansion())
            ;});

    [Fact]
    public void Should_CodeBeExpanded() {
        using (var ctx = new Context()) {
            var dto = ProjectTo<Dto>(ctx.Sources).ToList().First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");

            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name)); dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Code)); dto.Code.ShouldBe(_iqf.Code);
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc)); dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void Should_NameAndCodeBeExpanded() {
        using (var ctx = new Context()) {
            var dto = ProjectTo<Dto>(ctx.Sources, null, _ => _.Name).First();
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

public class ConstructorWithInheritanceNoExplicitExpansion : IntegrationTest<ConstructorWithInheritanceNoExplicitExpansion.DatabaseInitializer> {
    public class Entity {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SubEntity : Entity {
        public string Caption { get; set; }
    }

    record Dto(string Name) { }
    record SubDto(string Name, string Caption) : Dto(Name) { }

    public class Context : LocalDbContext {
        public DbSet<SubEntity> Entities { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context> {
        protected override void Seed(Context context) {
            context.Entities.Add(new() { Name = "Name", Caption = "Caption" });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => {
        c.CreateMap<Entity, Dto>().ForCtorParam("Name", o => o.ExplicitExpansion());
        c.CreateMap<SubEntity, SubDto>()
            .IncludeBase<Entity, Dto>()
            .ForCtorParam("Name", o => o.ExplicitExpansion(false))
            .ForCtorParam("Caption", o => o.ExplicitExpansion(false));
    });
    [Fact]
    public void Should_work() {
        using var context = new Context();
        var dto = ProjectTo<SubDto>(context.Entities).Single();
        dto.Name.ShouldBe("Name");
        dto.Caption.ShouldBe("Caption");
        dto = ProjectTo<SubDto>(context.Entities, null, d => d.Name, d => d.Caption).Single();
        dto.Name.ShouldBe("Name");
        dto.Caption.ShouldBe("Caption");
    }
}