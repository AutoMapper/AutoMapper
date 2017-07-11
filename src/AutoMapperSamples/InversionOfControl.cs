using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using StructureMap.Attributes;
using Shouldly;
using StructureMap.Configuration.DSL;

namespace AutoMapperSamples
{
    namespace InversionOfControl
    {
        [TestFixture]
        public class InversionOfControl
        {
            private class Source
            {
                public int Value { get; set; }
            }

            private class Destination
            {
                public int Value { get; set; }
            }

            [Test]
            public void Example2()
            {
                var container = new Container(init =>
                {
                    init.AddRegistry<ConfigurationRegistry>();
                });

                var engine = container.GetInstance<IMapper>();

                var destination = engine.Map<Source, Destination>(new Source {Value = 15});

                destination.Value.ShouldBe(15);
            }

            public class ConfigurationRegistry : Registry
            {
                public ConfigurationRegistry()
                {
                    var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

                    For<IMapper>().Use(ctx => new Mapper(configuration, ctx.GetInstance));
                }
            }
        }


    }
}
