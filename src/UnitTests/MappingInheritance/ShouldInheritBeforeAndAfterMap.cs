namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;
    using Xunit;

    public class ShouldInheritBeforeAndAfterMap
    {
        public class BaseClass
        {
            public string Prop { get; set; }
        }

        public class Class : BaseClass
        {
        }

        public class BaseDto
        {
            public string DifferentProp { get; set; }
        }

        public class Dto : BaseDto
        {
        }

        [Fact]
        public void should_inherit_base_beforemap()
        {
            // arrange
            var mc = new MapperContext();
            var source = new Class {Prop = "test"};

            mc.CreateMap<BaseClass, BaseDto>()
                .BeforeMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            mc.CreateMap<Class, Dto>();

            // act
            var dest = mc.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }

        [Fact]
        public void should_inherit_base_aftermap()
        {
            // arrange
            var mc = new MapperContext();
            var source = new Class {Prop = "test"};

            mc.CreateMap<BaseClass, BaseDto>()
                .AfterMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            mc.CreateMap<Class, Dto>();

            // act
            var dest = mc.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }
    }
}