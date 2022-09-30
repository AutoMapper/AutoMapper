namespace AutoMapper.UnitTests.Bug
{
    namespace ProjectCollectionsBug
    {
        public class A
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
        }

        public class B
        {
            public B()
            {
                BP2 = new HashSet<A>();
            }
            public int BP1 { get; set; }
            public ICollection<A> BP2 { get; set; }
        }

        public class AEntity
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
        }

        public class BEntity
        {
            public BEntity()
            {
                BP2 = new HashSet<AEntity>();
            }
            public int BP1 { get; set; }
            public ICollection<AEntity> BP2 { get; set; }
        }

        public class Bug
        {
            [Fact]
            public void Should_not_throw_exception()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<BEntity, B>();
                    cfg.CreateMap<AEntity, A>();
                });
                //config.AssertConfigurationIsValid();

                var be = new BEntity();
                be.BP1 = 3;
                be.BP2.Add(new AEntity() { AP1 = 1, AP2 = "hello" });
                be.BP2.Add(new AEntity() { AP1 = 2, AP2 = "two" });

                var b = config.CreateMapper().Map<BEntity, B>(be);

                var belist = new List<BEntity>();
                belist.Add(be);
                IQueryable<BEntity> bei = belist.AsQueryable();
                typeof(Exception).ShouldNotBeThrownBy(() => bei.ProjectTo<B>(config));
            }
        }
    }
}