using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    public class InterfaceSelfMappingBug
    {
        public interface IFoo
        {
            int Value { get; set; } 
        }

        public class Bar : IFoo
        {
            public int Value { get; set; }
        }

        public class Baz : IFoo
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Example()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.CreateMap<IFoo, IFoo>();
            });
            Mapper.AssertConfigurationIsValid();

            IFoo bar = new Bar
            {
                Value = 5
            };
            IFoo baz = new Baz
            {
                Value = 10
            };

            Mapper.Map(bar, baz);

            baz.Value.ShouldEqual(5);
        }
    }
}