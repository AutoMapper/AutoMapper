namespace AutoMapper.IntegrationTests.Inheritance;

public class ProjectToAbstractTypeWithInheritance : IntegrationTest<ProjectToAbstractTypeWithInheritance.DatabaseInitializer>
{
    public class StepGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Step> Steps { get; set; } = new();
    }
    public abstract class Step
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StepGroupId { get; set; }
        public virtual StepGroup StepGroup { get; set; }
    }
    public class CheckingStep : Step { }
    public class InstructionStep : Step { }
    public abstract class AbstractStep : Step { }

    public class StepGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<StepModel> Steps { get; set; } = new();
    }
    public abstract class StepModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CheckingStepModel : StepModel { }
    public class InstructionStepModel : StepModel { }
    public abstract class AbstractStepModel : StepModel { }

    public class Context : LocalDbContext
    {
        public DbSet<StepGroup> StepGroups { get; set; }

        public DbSet<Step> Steps { get; set; }

        public DbSet<CheckingStep> CheckingSteps { get; set; }

        public DbSet<InstructionStep> InstructionSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Step>(entity =>
            {
                entity.HasOne(d => d.StepGroup).WithMany(p => p.Steps)
                    .HasForeignKey(d => d.StepGroupId);
            });
        }
    }

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
            cfg.CreateMap<AbstractStep, AbstractStepModel>()
                .IncludeBase<Step, StepModel>();
        });
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.StepGroups.Add(new StepGroup
            {
                Name = "StepGroup",
                Steps = new List<Step>
                {
                    new InstructionStep
                    {
                        Name = "InstructionStep"
                    },
                    new CheckingStep
                    {
                        Name = "CheckingStep"
                    }
                }
            });

            base.Seed(context);
        }
    }

    [Fact]
    public void ProjectCollectionWithElementInheritingAbstractClass()
    {
        using var context = new Context();
        var steps = ProjectTo<StepGroupModel>(context.StepGroups).Single().Steps;
        steps[0].ShouldBeOfType<CheckingStepModel>().Name.ShouldBe("CheckingStep");
        steps[1].ShouldBeOfType<InstructionStepModel>().Name.ShouldBe("InstructionStep");
    }
}