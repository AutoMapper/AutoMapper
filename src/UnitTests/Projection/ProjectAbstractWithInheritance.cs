namespace AutoMapper.UnitTests.Projection;

public class ProjectAbstractWithInheritance : AutoMapperSpecBase
{
    class StepGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Step> Steps { get; set; } = new HashSet<Step>();
    }
    abstract class Step
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    class CheckingStep : Step { }
    class InstructionStep : Step { }

    class StepGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<StepModel> Steps { get; set; } = new HashSet<StepModel>();
    }
    abstract class StepModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    class CheckingStepModel : StepModel { }
    class InstructionStepModel : StepModel { }

    protected override MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<StepGroup, StepGroupModel>();
            cfg.CreateMap<Step, StepModel>();
            cfg.CreateMap<CheckingStep, CheckingStepModel>()
                .IncludeBase<Step, StepModel>();
            cfg.CreateMap<InstructionStep, InstructionStepModel>()
                .IncludeBase<Step, StepModel>();
        });
    }

    [Fact]
    public void ProjectCollectionWithElementInheritingAbstractClassWithoutException()
    {
        var stepGroup = new StepGroup
        {
            Id = 1,
            Name = "StepGroup",
            Steps = new List<Step>
            {
                new InstructionStep
                {
                    Id = 1,
                    Name = "InstructionStep"
                },
                new CheckingStep
                {
                    Id = 2,
                    Name = "CheckingStep"
                }
            }
        };

        var query = new[] { stepGroup }.AsQueryable();

        Should.NotThrow(() => query.ProjectTo<StepGroupModel>(Configuration).SingleOrDefault());
    }
}