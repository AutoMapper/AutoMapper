using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using StructureMap.Attributes;
using Should;
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
                ObjectFactory.Initialize(init =>
                {
                    init.AddRegistry<ConfigurationRegistry>();
                });

                var engine = ObjectFactory.GetInstance<IMapper>();

                var destination = engine.Map<Source, Destination>(new Source {Value = 15});

                destination.Value.ShouldEqual(15);
            }

            public class ConfigurationRegistry : Registry
            {
                public ConfigurationRegistry()
                {
                    var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

                    ForRequestedType<IMapper>().TheDefault.Is.ConstructedBy(() => configuration.CreateMapper());
                }
            }
        }


    }
}
