namespace AutoMapper.UnitTests.Projection;

public class ExplicitExpansionWithInheritance : AutoMapperSpecBase
{
    abstract class EntityBase
    {
        public EntityBase() => Id = Guid.NewGuid();
        public Guid Id { get; set; }
        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
    }

    class User { }
    class Computer : EntityBase { }
    class Script : EntityBase
    {
        public Computer Computer { get; set; }
        public Computer OtherComputer { get; set; }
    }

    class EntityBaseModel
    {
        public Guid Id { get; set; }
        public UserModel CreatedBy { get; set; }
        public UserModel ModifiedBy { get; set; }
    }

    class UserModel { }
    class ComputerModel : EntityBaseModel { }
    class ScriptModel : EntityBaseModel
    {
        public ComputerModel Computer { get; set; }
        public ComputerModel OtherComputer { get; set; }
    }

    private Script _source;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<User, UserModel>();
        cfg.CreateProjection<EntityBase, EntityBaseModel>()
            .ForMember(d => d.ModifiedBy, o => o.ExplicitExpansion())
            .ForMember(d => d.CreatedBy, o => o.ExplicitExpansion());
        cfg.CreateProjection<Computer, ComputerModel>()
            .ForMember(d => d.ModifiedBy, o => o.ExplicitExpansion())
            .ForMember(d => d.CreatedBy, o => o.ExplicitExpansion());
        cfg.CreateProjection<Script, ScriptModel>()
            .ForMember(d => d.Computer, o => o.ExplicitExpansion())
            .ForMember(d => d.ModifiedBy, o => o.ExplicitExpansion())
            .ForMember(d => d.CreatedBy, o => o.ExplicitExpansion());
    });

    protected override void Because_of()
    {
        _source = new Script
        {
            CreatedBy = new User(),
            ModifiedBy = new User(),
            Computer = new Computer()
            {
                CreatedBy = new User(),
                ModifiedBy = new User(),
            },
            OtherComputer = new Computer()
            {
                CreatedBy = new User(),
                ModifiedBy = new User(),
            }
        };
    }

    [Fact]
    public void ComputerCreatedBy_should_be_null_but_ScriptCreatedBy_should_not_be_null_using_lambda_expression_expansions()
    {
        // act
        var scriptModel = new[] { _source }.AsQueryable()
            .ProjectTo<ScriptModel>(Configuration, c => c.Computer, c => c.CreatedBy).Single();

        // assert
        Assert.Null(scriptModel.Computer.CreatedBy);
        Assert.NotNull(scriptModel.CreatedBy);
    }

    [Fact]
    public void ComputerCreatedBy_should_not_be_null_but_ScriptCreatedBy_should_be_null_using_lambda_expression_expansions()
    {
        // act
        var scriptModel = new[] { _source }.AsQueryable()
            .ProjectTo<ScriptModel>(Configuration, c => c.Computer.CreatedBy).Single();

        // assert
        Assert.NotNull(scriptModel.Computer.CreatedBy);
        Assert.Null(scriptModel.OtherComputer.CreatedBy);
        Assert.Null(scriptModel.CreatedBy);
    }

    [Fact]
    public void ComputerCreatedBy_should_be_null_but_ScriptCreatedBy_should_not_be_null_using_string_expansions()
    {
        // act
        var scriptModel = new[] { _source }.AsQueryable()
            .ProjectTo<ScriptModel>(Configuration, null, "Computer", "CreatedBy").Single();

        // assert
        Assert.Null(scriptModel.Computer.CreatedBy);
        Assert.NotNull(scriptModel.CreatedBy);
    }

    [Fact]
    public void ComputerCreatedBy_shouldnt_be_null_but_ScriptCreatedBy_should_be_null_using_string_expansions()
    {
        // act
        var scriptModel = new[] { _source }.AsQueryable()
            .ProjectTo<ScriptModel>(Configuration, null, "Computer.CreatedBy").Single();

        // assert
        Assert.NotNull(scriptModel.Computer.CreatedBy);
        Assert.Null(scriptModel.CreatedBy);
    }
}
