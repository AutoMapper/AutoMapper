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

                var configuration1 = ObjectFactory.GetInstance<Configuration>();
                var configuration2 = ObjectFactory.GetInstance<Configuration>();
                configuration1.ShouldBeTheSameAs(configuration2);
            }
        }

        public class ConfigurationRegistry : Registry
        {
            public ConfigurationRegistry()
            {
                Scan(scanner => scanner.AssemblyContainingType<IMappingEngine>());

                //ForRequestedType<IConfigurationProvider>()
                //    .CacheBy(InstanceScope.Singleton)
                //    .TheDefault.Is.OfConcreteType<Configuration>()
                //    .CtorDependency<IObjectMapper[]>().Is(expr => expr.ConstructedBy(MapperRegistry.AllMappers));
            }
        }
    }
}