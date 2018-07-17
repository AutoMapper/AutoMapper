namespace AutoMapper.UnitTests
{
    using System.Linq;
    using Shouldly;
    using Xunit;
    using QueryableExtensions;
    using System;

    [Collection(nameof(StaticMapping))]
    public class StaticMapping
    {
        public class ModelObject
        {
            public string Foo { get; set; }
            public string Barr { get; set; }
        }

        public class ModelDto
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        public class ModelObject2
        {
            public string Foo { get; set; }
            public string Barr { get; set; }
        }

        public class ModelDto2
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Bar1 { get; set; }
            public string Bar2 { get; set; }
            public string Bar3 { get; set; }
            public string Bar4 { get; set; }
        }

        public class ModelObject3
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Bar1 { get; set; }
            public string Bar2 { get; set; }
            public string Bar3 { get; set; }
            public string Bar4 { get; set; }
        }

        public class ModelDto3
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class OtherSource
        {
            public int Value { get; set; }
        }

        public class OtherDest
        {
            public int Value { get; set; }
        }

        private void InitializeMapping()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.CreateMap<ModelObject, ModelDto>();
                cfg.CreateMap<ModelObject2, ModelDto2>();
                cfg.CreateMap<ModelObject3, ModelDto3>(MemberList.Source);
            });
        }

        [Fact]
        public void Can_map_statically()
        {
            InitializeMapping();

            var source = new Source {Value = 5};

            var dest = Mapper.Map<Source, Dest>(source);

            dest.Value.ShouldBe(source.Value);
        } 

        [Fact]
        public void Can_project_statically()
        {
            InitializeMapping();

            var source = new Source {Value = 5};
            var sources = new[] {source}.AsQueryable();

            var dests = sources.ProjectTo<Dest>().ToArray();

            dests.Length.ShouldBe(1);
            dests[0].Value.ShouldBe(source.Value);
        }

        [Fact]
        public void Should_fail_a_configuration_check()
        {
            InitializeMapping();

            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
        }

        [Fact]
        public void Should_combine_mappings()
        {
            InitializeMapping();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<OtherSource, OtherDest>();
            });

            Mapper.Configuration.GetAllTypeMaps().Length.ShouldBe(5);
        }

        [Fact]
        public void Should_not_throw_when_initializing_twice()
        {
            var init = new Action(() =>
            {
                Mapper.Initialize(_ => { });
                Mapper.Initialize(_ => { });
            });

            init.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_resetting()
        {
            var action = new Action(() =>
            {
                Mapper.Reset();
                Mapper.Initialize(cfg => { });
                Mapper.Reset();
                Mapper.Initialize(cfg => { });
            });
            action.ShouldNotThrow();
        }

    }
}