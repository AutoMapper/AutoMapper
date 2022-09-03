namespace AutoMapper.UnitTests.Bug;

public class Self_referencing_existing_destination_without_PreserveReferences : AutoMapperSpecBase
{
    public class BaseType
    {
        public BaseType()
        {
            SelfReference = this;
        }
        public BaseType SelfReference { get; set; }
    }

    public class BaseTypeDto
    {
        public BaseTypeDto()
        {
            SelfReference = this;
        }
        public BaseTypeDto SelfReference { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=> cfg.CreateMap<BaseType, BaseTypeDto>());
    [Fact]
    public void Should_work()
    {
        var baseType = new BaseType();
        var baseTypeDto = new BaseTypeDto();

        Mapper.Map(baseType, baseTypeDto);
    }
}

public class WithoutPreserveReferencesSameDestination : AutoMapperSpecBase
{
    public class DtoOne
    {
        public DtoTwo Two { get; set; }
    }

    public class DtoTwo
    {
        public virtual ICollection<DtoOne> Ones { get; set; }
    }

    public class DtoThree
    {
        public int Id { get; set; }
    }

    public class EntityOne
    {
        public int Id { get; set; }
        public EntityTwo Two { get; set; }
    }

    public class EntityTwo
    {
        public virtual ICollection<EntityOne> Ones { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<EntityTwo, DtoTwo>();
        cfg.CreateMap<EntityOne, DtoOne>();
        cfg.CreateMap<EntityOne, DtoThree>();
    });

    [Fact]
    public void Should_use_the_right_map()
    {
        var source =
                new EntityOne {
                    Two = new EntityTwo {
                        Ones = new List<EntityOne> {
                            new EntityOne {
                                Two = new EntityTwo { Ones = new List<EntityOne>() }
                            }
                        }
                    }
                };
        Mapper.Map<EntityOne, DtoOne>(source).ShouldBeOfType<DtoOne>();
        Mapper.Map<EntityOne, DtoThree>(source).ShouldBeOfType<DtoThree>();
    }
}