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
        protected override void OnLoad()
        {
            CreateMap<Source, Destination>();
        }

        public override string Name
        {
            get
            {
                return "TestModule";
            }
        }
    }

    class ConflictingTestModule : AutoMapperModule
    {
        protected override void OnLoad()
        {
            CreateMap<Destination, Source>();
        }

        public override string Name
        {
            get
            {
                return "TestModule";
            }
        }
    }


    [TestFixture]
    public class ModuleLoading
    {
        [SetUp]
        public void SetUp()
        {
            AutoMapperModule.Configuration = () => Mapper.Configuration;
            AutoMapperModule.ResetModules();
        }

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

        [Test]
        [ExpectedException(typeof(NotSupportedException), ExpectedMessage="A module with the same name is already loaded")]
        public void modules_with_the_same_name_should_not_be_loaded_twice()
        {
            new TestModule().Load();
            new ConflictingTestModule().Load();
        }

        [Test]
        public void loaded_modules_should_be_resettable()
        {
            new TestModule().Load();
            AutoMapperModule.ResetModules();
            new ConflictingTestModule().Load();
        }

        [Test]
        public void loaded_modules_should_affect_mapping()
        {
            new TestModule().Load();
            Source s = CreateSource();

            Destination d = Mapper.Map<Source, Destination>(s);
            Assert.AreEqual(s.SomeProperty, d.SomeProperty);
        }

        private static Source CreateSource()
        {
            Source s = new Source
            {
                SomeProperty = 42
            };
            return s;
        }

        [Test]
        public void mapper_should_be_able_to_load_modules()
        {
            Mapper.AddModules(new TestModule());
            Source s = CreateSource();
            Destination d = Mapper.Map<Source, Destination>(s);
            Assert.AreEqual(s.SomeProperty, d.SomeProperty);
        }
    }
}
