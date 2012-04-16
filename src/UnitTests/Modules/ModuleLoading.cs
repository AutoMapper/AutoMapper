using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using AutoMapper.Modules;

namespace AutoMapper.UnitTests.Modules
{
    class Source
    {
        public int SomeProperty { get; set; }
    }

    class Destination
    {
        public int SomeProperty { get; set; }
    }

    class TestModule : AutoMapperModule
    {
        public override void Load()
        {
            CreateMap<Source, Destination>();
        }
    }

    [TestFixture]
    public class ModuleLoading
    {
        [Test]
        public void modules_should_delegate_mapping_creation_to_an_IConfiguration_instance()
        {
            IConfiguration config = MockRepository.GenerateMock<IConfiguration>();
            AutoMapperModule.Configuration = () => config;

            config.Expect(c => c.CreateMap<Source, Destination>()).Return(null);

            var module = new TestModule();
            module.Load();

            config.VerifyAllExpectations();
        }
    }
}
