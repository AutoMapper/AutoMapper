﻿namespace AutoMapper.UnitTests.Bug
{
    namespace ParentChildResolversBug
    {
        using Should;
        using Xunit;

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


        public class ParentResolver : IValueResolver<Source, ParentDestEnum?>
        {
            public ParentDestEnum? Resolve(Source source, ParentDestEnum? dest, ResolutionContext context)
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

        public class Resolver : IValueResolver<Source, DestEnum?>
        {
            public DestEnum? Resolve(Source source, DestEnum? dest, ResolutionContext context)
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

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, ParentDest>()
                    .ForMember(dest => dest.field, opt => opt.ResolveUsing<ParentResolver>())
                    .Include<Source, Dest>();

                cfg.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.field, opt => opt.ResolveUsing<Resolver>());
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
                _dest.field.ShouldEqual(DestEnum.a);
            }
        }
    }
}