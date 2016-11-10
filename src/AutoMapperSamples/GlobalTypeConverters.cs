using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples
{
    namespace GlobalTypeConverters
    {
        [TestFixture]
        public class GlobalTypeConverters
        {
            public class Source
            {
                public string Value1 { get; set; }
                public string Value2 { get; set; }
                public string Value3 { get; set; }
            }

            public class Destination
            {
                public int Value1 { get; set; }
                public DateTime Value2 { get; set; }
                public Type Value3 { get; set; }
            }

            public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
            {
                public DateTime Convert(string source, DateTime destination, ResolutionContext context)
                {
                    return System.Convert.ToDateTime(source);
                }
            }

            public class TypeTypeConverter : ITypeConverter<string, Type>
            {
                public Type Convert(string source, Type destination, ResolutionContext context)
                {
                    Type type = Assembly.GetExecutingAssembly().GetType(source);
                    return type;
                }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<string, int>().ConvertUsing((string s) => Convert.ToInt32(s));
                    cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
                    cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
                    cfg.CreateMap<Source, Destination>();
                });
                config.AssertConfigurationIsValid();

                var source = new Source
                {
                    Value1 = "5",
                    Value2 = "01/01/2000",
                    Value3 = "AutoMapperSamples.GlobalTypeConverters.GlobalTypeConverters+Destination"
                };

                var mapper = config.CreateMapper();
                Destination result = mapper.Map<Source, Destination>(source);
                result.Value3.ShouldEqual(typeof(Destination));

                Expression<Func<Source, object>> func = x => x.Value1;

            }
        }
    }
}