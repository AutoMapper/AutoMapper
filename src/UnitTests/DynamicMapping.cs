using System;
using System.Linq;
using Xunit;
using Should;
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
            _result.Value.ShouldEqual(ConsoleColor.DarkGreen);
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

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
            result.Patient.Id.ShouldEqual(10);
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

        [Fact]
        public void Should_dynamically_map_the_two_types()
        {
            _resultWithGenerics = Mapper.Map<Source, Destination>(new Source {Value = 5});
            _resultWithoutGenerics = (Destination) Mapper.Map(new Source {Value = 5}, typeof(Source), typeof(Destination));
            _resultWithGenerics.Value.ShouldEqual(5);
            _resultWithoutGenerics.Value.ShouldEqual(5);
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

            cfg.CreateMissingTypeMaps = true;
        });

        [Fact]
        public void Should_use_resolver()
        {
            var tm = Configuration.FindTypeMapFor<Original, Target>();
            var original = new Original { Text = "Hello world from original!" };
            var mapped = Mapper.Map<Target>(original);

            mapped.Text.ShouldEqual(original.Text);
            mapped.Child.ShouldNotBeNull();
            mapped.Child.Content.ShouldEqual("Hello world from inner!");
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

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
            _resultWithGenerics.Value.ShouldEqual(5);
        }

        [Fact]
        public void Should_dynamically_map_the_children()
        {
            _resultWithGenerics.Child.Value2.ShouldEqual("foo");
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

        [Fact]
        public void Should_ignore_any_members_that_do_not_match()
        {
            var destination = Mapper.Map<Source, Destination>(new Source {Value = 5});

            destination.Valuefff.ShouldEqual(0);
        }

        [Fact]
        public void Should_not_throw_any_configuration_errors()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.Map<Source, Destination>(new Source { Value = 5 }));
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

        public When_mapping_to_an_existing_destination_object()
        {
            _destination = new Destination { Valuefff = 7};
            Mapper.Map(new Source { Value = 5, Value2 = 3}, _destination);
        }

        [Fact]
        public void Should_preserve_existing_values()
        {
            _destination.Valuefff.ShouldEqual(7);
        }

        [Fact]
        public void Should_map_new_values()
        {
            _destination.Value2.ShouldEqual(3);
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
            _result.Value.ShouldEqual(5);
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
    }
}