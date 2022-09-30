namespace AutoMapper.UnitTests.Bug;

public class ObjectTypeMapFailure : NonValidatingSpecBase
{
    [Fact]
    public void Should_map_the_object_type()
    {
        var displayModel = new DisplayModel
        {
            Radius = 300
        };
        object vm = new SomeViewModel();
        var config = new MapperConfiguration(cfg => cfg.CreateMap<DisplayModel, SomeViewModel>());

        var mapper = config.CreateMapper();
        mapper.Map(displayModel, vm);
        ((SomeViewModel)vm).Radius.ShouldBe(300); // fails

        var vm2 = new SomeViewModel();
        mapper.Map(displayModel, vm2);
        vm2.Radius.ShouldBe(300); // succeeds
    }

    public class SomeViewModel
    {
        public int Radius { get; set; }
    }

    public class DisplayModel
    {
        public int Radius { get; set; }
    }
}
