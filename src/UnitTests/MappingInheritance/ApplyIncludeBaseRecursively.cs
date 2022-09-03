namespace AutoMapper.UnitTests.MappingInheritance;

public class ApplyIncludeBaseRecursively : AutoMapperSpecBase
{
    ViewModel _destination;

    public class BaseEntity
    {
        public string Property1 { get; set; }
    }
    public class SubBaseEntity : BaseEntity { }

    public class SpecificEntity : SubBaseEntity
    {
        public bool Map { get; set; }
    }

    public class ViewModel
    {
        public string Property2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<BaseEntity, ViewModel>()
            .ForMember(vm => vm.Property2, opt => opt.MapFrom(e => e.Property1));

        cfg.CreateMap<SubBaseEntity, ViewModel>()
            .IncludeBase<BaseEntity, ViewModel>();

        cfg.CreateMap<SpecificEntity, ViewModel>()
            .IncludeBase<SubBaseEntity, ViewModel>()
            .ForMember(vm => vm.Property2, opt => opt.Condition(e => e.Map));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<ViewModel>(new SpecificEntity{ Map = true, Property1 = "Test" });
    }

    [Fact]
    public void Should_apply_all_included_base_maps()
    {
        _destination.Property2.ShouldBe("Test");
    }
}
public class IncludeOrder : AutoMapperSpecBase
{
    public interface IDevice
    {
        int Id { get; set; }
    }
    public interface IDerivedDevice : IDevice
    {
        int AdditionalProperty { get; set; }
    }
    public class Device : IDevice
    {
        public int Id { get; set; }
    }
    public class DerivedDevice : Device, IDerivedDevice
    {
        public int AdditionalProperty { get; set; }
    }
    public class DeviceDto
    {
        public int Id { get; set; }

        public int AdditionalProperty { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<IDevice, DeviceDto>(MemberList.None).Include<IDerivedDevice, DeviceDto>();
        cfg.CreateMap<IDerivedDevice, DeviceDto>();
    });
    [Fact]
    public void BaseFirst()
    {
        var source = new IDevice[] { new Device { Id = 2 }, new DerivedDevice { Id = 1, AdditionalProperty = 7 } };
        var destination = Map<DeviceDto[]>(source);
        destination[0].Id.ShouldBe(2);
        destination[0].AdditionalProperty.ShouldBe(0);
        destination[1].Id.ShouldBe(1);
        destination[1].AdditionalProperty.ShouldBe(7);
    }
    [Fact]
    public void DerivedFirst()
    {
        var source = new IDevice[] { new DerivedDevice { Id = 1, AdditionalProperty = 7 }, new Device { Id = 2 } };
        var destination = Map<DeviceDto[]>(source);
        destination[0].Id.ShouldBe(1);
        destination[0].AdditionalProperty.ShouldBe(7);
        destination[1].Id.ShouldBe(2);
        destination[1].AdditionalProperty.ShouldBe(0);
    }
}
public class CircularAs : NonValidatingSpecBase
{
    public interface IDevice
    {
        int Id { get; set; }
    }
    public interface IDerivedDevice : IDevice
    {
        int AdditionalProperty { get; set; }
    }
    public class Device : IDevice
    {
        public int Id { get; set; }
    }
    public class DerivedDevice : Device, IDerivedDevice
    {
        public int AdditionalProperty { get; set; }
    }
    public class DeviceDto
    {
        public int Id { get; set; }
        public int AdditionalProperty { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<IDevice, DeviceDto>(MemberList.None).As<DeviceDto>();
        cfg.CreateMap<IDerivedDevice, DeviceDto>();
    });
    [Fact]
    public void Should_report_the_error() => new Action(AssertConfigurationIsValid).ShouldThrow<InvalidOperationException>().Message.ShouldBe(
        "As must specify a derived type, not " + typeof(DeviceDto));
}