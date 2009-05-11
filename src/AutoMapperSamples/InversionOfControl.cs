using AutoMapper;
using AutoMapper.Mappers;
using NUnit.Framework;
using StructureMap;
using StructureMap.Attributes;
using NBehave.Spec.NUnit;
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
                configuration1.ShouldBeTheSameAs(configuration2);

                var configurationProvider = ObjectFactory.GetInstance<IConfigurationProvider>();
                configurationProvider.ShouldBeTheSameAs(configuration1);

                var configuration = ObjectFactory.GetInstance<Configuration>();
                configuration.ShouldBeTheSameAs(configuration1);
                
                configuration1.CreateMap<Source, Destination>();

                var engine = ObjectFactory.GetInstance<IMappingEngine>();

                var destination = engine.Map<Source, Destination>(new Source {Value = 15});

                destination.Value.ShouldEqual(15);
            }
        }

        public class ConfigurationRegistry : Registry
        {
            public ConfigurationRegistry()
            {
                ForRequestedType<Configuration>()
                    .CacheBy(InstanceScope.Singleton)
                    .TheDefault.Is.OfConcreteType<Configuration>()
                    .CtorDependency<IObjectMapper[]>().Is(expr => expr.ConstructedBy(MapperRegistry.AllMappers));

                ForRequestedType<IConfigurationProvider>()
                    .TheDefault.Is.ConstructedBy(ctx => ctx.GetInstance<Configuration>());

                ForRequestedType<IConfiguration>()
                    .TheDefault.Is.ConstructedBy(ctx => ctx.GetInstance<Configuration>());

                ForRequestedType<IMappingEngine>().TheDefaultIsConcreteType<MappingEngine>();
            }
        }
    }
}