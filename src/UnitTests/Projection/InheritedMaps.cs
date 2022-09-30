namespace AutoMapper.UnitTests.Projection
{
    namespace InheritedMaps
    {
        public class SourceBase
        {
            public int OtherValue { get; set; }
        }

        public class Source : SourceBase
        {
            
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class When_projecting_and_inheriting_maps : AutoMapperSpecBase
        {
            private Dest[] _dest;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<SourceBase, Dest>()
                    .Include<Source, Dest>()
                    .ForMember(d => d.Value, opt => opt.MapFrom(src => src.OtherValue));

                cfg.CreateProjection<Source, Dest>();
            });

            protected override void Because_of()
            {
                IQueryable<Source> sources = new[]
                {
                    new Source()
                    {
                        OtherValue = 10
                    }
                }.AsQueryable();

                _dest = sources.ProjectTo<Dest>(Configuration).ToArray();
            }

            [Fact]
            public void Should_inherit_base_mapping()
            {
                _dest[0].Value.ShouldBe(10);
            }
        }
    }
}