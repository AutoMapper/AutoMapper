namespace AutoMapper.UnitTests.Projection;
public class ParameterizedQueriesTests_with_anonymous_object_and_factory : AutoMapperSpecBase
{
    private Dest[] _dests;
    private IQueryable<Source> _sources;

    public class Source
    {
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        int value = 0;

        Expression<Func<Source, int>> sourceMember = src => value + 5;
        cfg.CreateProjection<Source, Dest>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
    });

    protected override void Because_of()
    {
        _sources = new[]
        {
            new Source()
        }.AsQueryable();

        _dests = _sources.ProjectTo<Dest>(Configuration, new { value = 10 }).ToArray();
    }

    [Fact]
    public void Should_substitute_parameter_value()
    {
        _dests[0].Value.ShouldBe(15);
    }

    [Fact]
    public void Should_not_cache_parameter_value()
    {
        var newDests = _sources.ProjectTo<Dest>(Configuration, new { value = 15 }).ToArray();

        newDests[0].Value.ShouldBe(20);
    }
}

public class ParameterizedQueriesTests_with_anonymous_object : AutoMapperSpecBase
{
    private Dest[] _dests;
    private IQueryable<Source> _sources;

    public class Source
    {
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        int value = 0;

        Expression<Func<Source, int>> sourceMember = src => value + 5;
        cfg.CreateProjection<Source, Dest>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
    });

    protected override void Because_of()
    {
        _sources = new[]
        {
            new Source()
        }.AsQueryable();

        _dests = _sources.ProjectTo<Dest>(Configuration, new { value = 10 }).ToArray();
    }

    [Fact]
    public void Should_substitute_parameter_value()
    {
        _dests[0].Value.ShouldBe(15);
    }

    [Fact]
    public void Should_not_cache_parameter_value()
    {
        var newDests = _sources.ProjectTo<Dest>(Configuration, new {value = 15}).ToArray();

        newDests[0].Value.ShouldBe(20);
    }
}

public class ParameterizedQueriesTests_with_dictionary_object : AutoMapperSpecBase
{
    private Dest[] _dests;
    private IQueryable<Source> _sources;

    public class Source
    {
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        int value = 0;

        Expression<Func<Source, int>> sourceMember = src => value + 5;
        cfg.CreateProjection<Source, Dest>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
    });

    protected override void Because_of()
    {
        _sources = new[]
        {
            new Source()
        }.AsQueryable();

        _dests = _sources.ProjectTo<Dest>(Configuration, new Dictionary<string, object>{{"value", 10}}).ToArray();
    }

    [Fact]
    public void Should_substitute_parameter_value()
    {
        _dests[0].Value.ShouldBe(15);
    }

    [Fact]
    public void Should_not_cache_parameter_value()
    {
        var newDests = _sources.ProjectTo<Dest>(Configuration, new Dictionary<string, object> { { "value", 15 } }).ToArray();

        newDests[0].Value.ShouldBe(20);
    }  
}

public class ParameterizedQueriesTests_with_filter : AutoMapperSpecBase
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DateActivated { get; set; }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DateActivated { get; set; }
        public int position { get; set; }
    }

    public class DB
    {
        public DB()
        {
            Users = new List<User>()
            {
                new User {DateActivated = new DateTime(2000, 1, 1), Id = 1, Name = "Joe Schmoe"},
                new User {DateActivated = new DateTime(2000, 2, 1), Id = 2, Name = "John Schmoe"},
                new User {DateActivated = new DateTime(2000, 3, 1), Id = 3, Name = "Jim Schmoe"},
            }.AsQueryable();
        }
        public IQueryable<User> Users { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        DB db = null;

        cfg.CreateProjection<User, UserViewModel>()
            .ForMember(a => a.position,
                opt => opt.MapFrom(src => db.Users.Count(u => u.DateActivated < src.DateActivated)));
    });

    [Fact]
    public void Should_only_replace_outer_parameters()
    {
        var db = new DB();

        var user = db.Users.ProjectTo<UserViewModel>(Configuration, new { db }).FirstOrDefault(a => a.Id == 2);

        user.position.ShouldBe(1);
    }
}