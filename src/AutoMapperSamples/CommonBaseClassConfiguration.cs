using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples
{
    namespace CommonBaseClassConfiguration
    {
        public class RootDest
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
            public int Value4 { get; set; }
            public int Value5 { get; set; }
        }

        [TestFixture]
        public class ProvidingCommonBaseClassConfiguration
        {
            public class SubDest1 : RootDest
            {
                public string SomeValue { get; set; }
            }

            public class SubDest2 : RootDest
            {
                public string SomeOtherValue { get; set; }
            }

            public class Source1
            {
                public string SomeValue { get; set; }
            }

            public class Source2
            {
                public string SomeOtherValue { get; set; }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Source1, SubDest1>().FixRootDest();
                    cfg.CreateMap<Source2, SubDest2>().FixRootDest();
                });

                config.AssertConfigurationIsValid();

                var mapper = config.CreateMapper();

                var subDest1 = mapper.Map<Source1, SubDest1>(new Source1 {SomeValue = "Value1"});
                var subDest2 = mapper.Map<Source2, SubDest2>(new Source2 {SomeOtherValue = "Value2"});

                subDest1.SomeValue.ShouldEqual("Value1");
                subDest2.SomeOtherValue.ShouldEqual("Value2");
            }
        }

        public static class MappingExpressionExtensions
        {
            public static IMappingExpression<TSource, TDestination> FixRootDest<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression)
                where TDestination : RootDest
            {
                mappingExpression.ForMember(dest => dest.Value1, opt => opt.Ignore());
                mappingExpression.ForMember(dest => dest.Value2, opt => opt.Ignore());
                mappingExpression.ForMember(dest => dest.Value3, opt => opt.Ignore());
                mappingExpression.ForMember(dest => dest.Value4, opt => opt.Ignore());
                mappingExpression.ForMember(dest => dest.Value5, opt => opt.Ignore());

                return mappingExpression;
            }
        }
    }
}