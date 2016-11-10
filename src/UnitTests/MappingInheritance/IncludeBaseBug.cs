namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;

    public class Test : AutoMapperSpecBase
    {
        public class From
        {
            public int Value { get; set; }
            public int ChildValue { get; set; }
        }


        public class Concrete
        {
            public int ConcreteValue { get; set; }
        }

        public abstract class AbstractChild : Concrete
        {
            public int AbstractValue { get; set; }
        }

        public class Derivation : AbstractChild
        {
            public int DerivedValue { get; set; }
        }


        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<From, Concrete>()
                .ForMember(d => d.ConcreteValue, o => o.MapFrom(s => s == null ? default(int) : s.ChildValue))
                .Include<From, AbstractChild>();
            cfg.CreateMap<From, AbstractChild>()
                .ForMember(d => d.AbstractValue, o => o.Ignore())
                .Include<From, Derivation>();
            cfg.CreateMap<From, Derivation>()
                .ForMember(d => d.DerivedValue, o => o.Ignore());
            cfg.AllowNullDestinationValues = false;
        });

        public void TestMethod1()
        {
            var dest = Mapper.Map(null, typeof(From), typeof(Concrete));
            dest.ShouldBeType<Concrete>();
            ReferenceEquals(dest.GetType(), typeof(Concrete)).ShouldBeTrue();
        }
    }
}