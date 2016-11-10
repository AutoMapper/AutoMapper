using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples
{
    namespace CustomValueResolvers
    {
        [TestFixture]
        public class CustomResolutionClass
        {
            public class Source
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
            }

            public class Destination
            {
                public int Total { get; set; }
            }

            public class CustomResolver : IValueResolver<Source, Destination, int>
            {
                public int Resolve(Source source, Destination d, int dest, ResolutionContext context)
                {
                    return source.Value1 + source.Value2;
                }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Total, opt => opt.ResolveUsing<CustomResolver>()));
                config.AssertConfigurationIsValid();

                var source = new Source
                    {
                        Value1 = 5,
                        Value2 = 7
                    };

                var result = config.CreateMapper().Map<Source, Destination>(source);

                result.Total.ShouldEqual(12);
            }

            [Test]
            public void ConstructedExample()
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Total, opt => opt.ResolveUsing(new CustomResolver())
                    ));
                config.AssertConfigurationIsValid();

                var source = new Source
                    {
                        Value1 = 5,
                        Value2 = 7
                    };

                var result = config.CreateMapper().Map<Source, Destination>(source);

                result.Total.ShouldEqual(12);
            }
        }
    }
}