namespace AutoMapper.UnitTests.Bug
{
    namespace ParentChildResolversBug
    {
        public enum DestEnum
        {
            a,
            b,
            c,
        }

        public enum ParentDestEnum
        {
            d,
            e,
            f
        }

        public class ParentDest
        {
            public ParentDestEnum? field
            {
                get;
                set;
            }
        }

        public class Dest : ParentDest
        {
            public new DestEnum? field
            {
                get;
                set;
            }
        }

        public class Source
        {
            public string fieldCode
            {
                get;
                set;
            }
        }


        public class ParentResolver : IValueResolver<Source, ParentDest, ParentDestEnum?>
        {
            public ParentDestEnum? Resolve(Source source, ParentDest dest, ParentDestEnum? destMember, ResolutionContext context)
            {
                switch (source.fieldCode)
                {
                    case "testa": return ParentDestEnum.d;
                    case "testb": return ParentDestEnum.e;
                    case "testc": return ParentDestEnum.f;
                    default: return null;
                }
            }
        }

        public class Resolver : IValueResolver<Source, ParentDest, DestEnum?>
        {
            public DestEnum? Resolve(Source source, ParentDest dest, DestEnum? destMember, ResolutionContext context)
            {
                switch (source.fieldCode)
                {
                    case "testa": return DestEnum.a;
                    case "testb": return DestEnum.b;
                    case "testc": return DestEnum.c;
                    default: return null;
                }
            }
        }

        public class ParentChildResolverTests : AutoMapperSpecBase
        {
            private Dest _dest;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, ParentDest>()
                    .ForMember(dest => dest.field, opt => opt.MapFrom<ParentResolver>())
                    .Include<Source, Dest>();

                cfg.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.field, opt => opt.MapFrom<Resolver>());
            });

            protected override void Because_of()
            {
                var source = new Source()
                {
                    fieldCode = "testa"
                };

                _dest = Mapper.Map<Dest>(source);
            }

            [Fact]
            public void Should_use_correct_resolver()
            {
                _dest.field.ShouldBe(DestEnum.a);
            }
        }
    }
}