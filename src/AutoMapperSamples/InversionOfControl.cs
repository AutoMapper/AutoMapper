using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Mappers;
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
            public void Example()
            {
                ObjectFactory.Initialize(init =>
                {
                    init.AddRegistry<ConfigurationRegistry>();
                });

                var configuration1 = ObjectFactory.GetInstance<IConfiguration>();
                var configuration2 = ObjectFactory.GetInstance<IConfiguration>();
                configuration1.ShouldBeSameAs(configuration2);

                var configurationProvider = ObjectFactory.GetInstance<IConfigurationProvider>();
                configurationProvider.ShouldBeSameAs(configuration1);

                var configuration = ObjectFactory.GetInstance<ConfigurationStore>();
                configuration.ShouldBeSameAs(configuration1);
                
                configuration1.CreateMap<Source, Destination>();

                var engine = ObjectFactory.GetInstance<IMappingEngine>();

                var destination = engine.Map<Source, Destination>(new Source {Value = 15});

                destination.Value.ShouldEqual(15);
            }

            [Test]
            public void Example2()
            {
                ObjectFactory.Initialize(init =>
                {
                    init.AddRegistry<MappingEngineRegistry>();
                });

                Mapper.Reset();

                Mapper.CreateMap<Source, Destination>();

                var engine = ObjectFactory.GetInstance<IMappingEngine>();

                var destination = engine.Map<Source, Destination>(new Source {Value = 15});

                destination.Value.ShouldEqual(15);
            }
        }

        public class ConfigurationRegistry : Registry
        {
            public ConfigurationRegistry()
            {
				ForRequestedType<ConfigurationStore>()
					.CacheBy(InstanceScope.Singleton)
					.TheDefault.Is.OfConcreteType<ConfigurationStore>()
					.CtorDependency<IEnumerable<IObjectMapper>>().Is(expr => expr.ConstructedBy(() => MapperRegistry.Mappers));

                ForRequestedType<IConfigurationProvider>()
					.TheDefault.Is.ConstructedBy(ctx => ctx.GetInstance<ConfigurationStore>());

                ForRequestedType<IConfiguration>()
					.TheDefault.Is.ConstructedBy(ctx => ctx.GetInstance<ConfigurationStore>());

                ForRequestedType<IMappingEngine>().TheDefaultIsConcreteType<MappingEngine>();

            	ForRequestedType<ITypeMapFactory>().TheDefaultIsConcreteType<TypeMapFactory>();
            }
        }

        public class MappingEngineRegistry : Registry
        {
            public MappingEngineRegistry()
            {
                ForRequestedType<IMappingEngine>()
                    .TheDefault.Is.ConstructedBy(() => Mapper.Engine);
            }
        }

    }
}
