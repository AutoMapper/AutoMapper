using System;
using System.Linq;
using Xunit;
using Shouldly;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.DynamicMapping
{
    public class When_mapping_from_untyped_enum_to_typed_enum : NonValidatingSpecBase
    {
        private Destination _result;

        public class Destination
        {
            public ConsoleColor Value { get; set; }
        }

        protected override void Because_of()
        {
            _result = Mapper.Map<Destination>(new { Value = (Enum) ConsoleColor.DarkGreen });
        }

        [Fact]
        public void Should_map_ok()
        {
            _result.Value.ShouldBe(ConsoleColor.DarkGreen);
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => {});
    }

    public class When_mapping_nested_types : NonValidatingSpecBase
    {
        List<ParentTestDto> _destination;

        public class Test
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TestDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ParentTestDto
        {
            public int Code { get; set; }
            public string FullName { get; set; }
            public TestDto Patient { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        protected override void Because_of()
        {
            var dynamicObject = new
            {
                Code = 5,
                FullName = "John",
                Patient = new Test
                {
                    Id = 10,
                    Name = "Roberts"
                }
            };
            var dynamicList = new List<object> { dynamicObject };
            _destination = Mapper.Map<List<ParentTestDto>>(dynamicList);
        }

        [Fact]
        public void Should_map_ok()
        {
            var result = _destination.First();
            result.Patient.Id.ShouldBe(10);
        }
    }

    public class When_mapping_two_non_configured_types : NonValidatingSpecBase
    {
        private Destination _resultWithGenerics;
        private Destination _resultWithoutGenerics;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        [Fact]
        public void Should_dynamically_map_the_two_types()
        {
            _resultWithGenerics = Mapper.Map<Source, Destination>(new Source {Value = 5});
            _resultWithoutGenerics = (Destination) Mapper.Map(new Source {Value = 5}, typeof(Source), typeof(Destination));
            _resultWithGenerics.Value.ShouldBe(5);
            _resultWithoutGenerics.Value.ShouldBe(5);
        }
    }

    public class When_mapping_two_non_configured_types_with_resolvers : NonValidatingSpecBase
    {
        public class Inner
        {
            public string Content { get; set; }
        }

        public class Original
        {
            public string Text { get; set; }
        }

        public class Target
        {
            public string Text { get; set; }

            public Inner Child { get; set; }
        }

        public class TargetResolver : IValueResolver<Original, Target, Inner>
        {
            public Inner Resolve(Original source, Target dest, Inner destination, ResolutionContext context)
            {
                return new Inner { Content = "Hello world from inner!" };
            }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Original, Target>()
                .ForMember(t => t.Child, o => o.ResolveUsing<TargetResolver>());
        });

        [Fact]
        public void Should_use_resolver()
        {
            var tm = Configuration.FindTypeMapFor<Original, Target>();
            var original = new Original { Text = "Hello world from original!" };
            var mapped = Mapper.Map<Target>(original);

            mapped.Text.ShouldBe(original.Text);
            mapped.Child.ShouldNotBeNull();
            mapped.Child.Content.ShouldBe("Hello world from inner!");
        }
    }

    public class When_mapping_two_non_configured_types_with_nesting : NonValidatingSpecBase
    {
        private Destination _resultWithGenerics;

        public class Source
        {
            public int Value { get; set; }
            public ChildSource Child { get; set; }
        }

        public class ChildSource
        {
            public string Value2 { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
            public ChildDestination Child { get; set; }
        }

        public class ChildDestination
        {
            public string Value2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => {});

        public When_mapping_two_non_configured_types_with_nesting()
        {
            var source = new Source
            {
                Value = 5,
                Child = new ChildSource
                {
                    Value2 = "foo"
                }
            };
            _resultWithGenerics = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_dynamically_map_the_two_types()
        {
            _resultWithGenerics.Value.ShouldBe(5);
        }

        [Fact]
        public void Should_dynamically_map_the_children()
        {
            _resultWithGenerics.Child.Value2.ShouldBe("foo");
        }
    }

    public class When_mapping_two_non_configured_types_that_do_not_match : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int Valuefff { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        [Fact]
        public void Should_ignore_any_members_that_do_not_match()
        {
            var destination = Mapper.Map<Source, Destination>(new Source {Value = 5}, opt => opt.ConfigureMap(MemberList.None));

            destination.Valuefff.ShouldBe(0);
        }

        [Fact]
        public void Should_not_throw_any_configuration_errors()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.Map<Source, Destination>(new Source { Value = 5 }, opt => opt.ConfigureMap(MemberList.None)));
        }
    }

    public class When_mapping_to_an_existing_destination_object : NonValidatingSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }

        public class Destination
        {
            public int Valuefff { get; set; }
            public int Value2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => {});

        public When_mapping_to_an_existing_destination_object()
        {
            _destination = new Destination { Valuefff = 7};
            Mapper.Map(new Source { Value = 5, Value2 = 3}, _destination, opt => opt.ConfigureMap(MemberList.None));
        }

        [Fact]
        public void Should_preserve_existing_values()
        {
            _destination.Valuefff.ShouldBe(7);
        }

        [Fact]
        public void Should_map_new_values()
        {
            _destination.Value2.ShouldBe(3);
        }
    }

    public class When_mapping_from_an_anonymous_type_to_an_interface : NonValidatingSpecBase
    {
        private IDestination _result;

        public interface IDestination
        {
            int Value { get; set; }
        }

        protected override void Because_of()
        {
            _result = Mapper.Map<IDestination>(new {Value = 5});
        }

        [Fact]
        public void Should_allow_dynamic_mapping()
        {
            _result.Value.ShouldBe(5);
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });
    }

    public class When_dynamically_mapping_a_badly_configured_map : NonValidatingSpecBase
    {
        public class Source
        {
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => { });

        [Fact]
        public void Should_throw()
        {
            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(new Source()));
        }
    }

    public class When_automatically_dynamically_mapping : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => {});

        [Fact]
        public void Should_map()
        {
            var source = new Source {Value = 5};
            var dest = Mapper.Map<Dest>(source);
            dest.Value.ShouldBe(5);
        }
    }

    public class When_mixing_auto_and_manual_map : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
            public Inner Value2 { get; set; }

            public class Inner
            {
                public string Value { get; set; }
            }
        }

        public class Dest
        {
            public int Value { get; set; }
            public Inner Value2 { get; set; }

            public class Inner
            {
                public string Value { get; set; }
            }

        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>().ForMember(d => d.Value, opt => opt.MapFrom(src => src.Value + 5)));

        [Fact]
        public void Should_map()
        {
            var source = new Source
            {
                Value = 5,
                Value2 = new Source.Inner
                {
                    Value = "asdf"
                }
            };

            var dest = Mapper.Map<Dest>(source);

            dest.Value.ShouldBe(source.Value + 5);
            dest.Value2.Value.ShouldBe(source.Value2.Value);
        }
    }

    public class When_mixing_auto_and_manual_map_with_mismatched_properties : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
            public Inner Value2 { get; set; }

            public class Inner
            {
                public string Value { get; set; }
            }
        }

        public class Dest
        {
            public int Value { get; set; }
            public Inner Value2 { get; set; }

            public class Inner
            {
                public string Valuefff { get; set; }
            }

        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>().ForMember(d => d.Value, opt => opt.MapFrom(src => src.Value + 5)));

        [Fact]
        public void Should_pass_validation()
        {
            Action assert = () => Configuration.AssertConfigurationIsValid();

            assert.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_pass_runtime_validation()
        {
            Action assert = () => Mapper.Map<Dest>(new Source { Value = 5, Value2 = new Source.Inner { Value = "asdf"}});

            var exception = assert.ShouldThrow<AutoMapperMappingException>();
            var inner = exception.InnerException as AutoMapperConfigurationException;

            inner.ShouldNotBeNull();

            inner.Errors.Select(e => e.TypeMap.Types).ShouldContain(tp => tp == new TypePair(typeof(Source.Inner), typeof(Dest.Inner)));
        }
    }

}