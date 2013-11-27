using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    namespace ConditionalMapping
    {
        public class When_configuring_a_member_to_skip_based_on_the_property_value : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.Value, opt => opt.Condition(src => src.Value > 0));
                });
            }

            [Fact]
            public void Should_skip_the_mapping_when_the_condition_is_true()
            {
                var destination = Mapper.Map<Source, Destination>(new Source {Value = -1});

                destination.Value.ShouldEqual(0);
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_is_false()
            {
                var destination = Mapper.Map<Source, Destination>(new Source { Value = 7 });

                destination.Value.ShouldEqual(7);
            }
        }

        public class When_configuring_a_member_to_skip_based_on_the_property_value_with_custom_mapping : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.Value, opt =>
                        {
                            opt.Condition(src => src.Value > 0);
                            opt.ResolveUsing((Source src) =>
                            {
                                return 10;
                            });
                        });
                });
            }

            [Fact]
            public void Should_skip_the_mapping_when_the_condition_is_true()
            {
                var destination = Mapper.Map<Source, Destination>(new Source { Value = -1 });

                destination.Value.ShouldEqual(0);
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_is_false()
            {
                Mapper.Map<Source, Destination>(new Source { Value = 7 }).Value.ShouldEqual(10);
            }
        }


        public class When_configuring_a_member_to_skip_based_on_the_property_metadata : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }

                [Skip]
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForAllMembers(opt => opt.Condition(CustomCondition));
                });
            }

            private static bool CustomCondition(ResolutionContext context)
            {
                return !context.PropertyMap.DestinationProperty.MemberInfo.GetCustomAttributes(true).Any(attr => attr is SkipAttribute);
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value = 5, Value2 = 10});
            }

            [Fact]
            public void Should_include_the_normal_members_by_default()
            {
                _destination.Value.ShouldEqual(5);
            }

            [Fact]
            public void Should_skip_any_members_based_on_the_skip_condition()
            {
                _destination.Value2.ShouldEqual(default(int));
            }

            public class SkipAttribute : System.Attribute { }
        }


        public class When_configuring_a_map_to_ignore_all_properties_with_an_inaccessible_setter : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Id { get; set; }
                public string Title { get; set; }
                public string CodeName { get; set; }
                public string Nickname { get; set; }
                public string ScreenName { get; set; }
            }

            public class Destination
            {
                private double _height;

                public int Id { get; set; }
                public virtual string Name { get; protected set; }
                public string Title { get; internal set; }
                public string CodeName { get; private set; }
                public string Nickname { get; private set; }
                public string ScreenName { get; private set; }
                public int Age { get; private set; }

                public double Height
                {
                    get { return _height; }
                }

                public Destination()
                {
                    _height = 60;
                }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.ScreenName, opt => opt.MapFrom(src => src.ScreenName))
                        .IgnoreAllPropertiesWithAnInaccessibleSetter()
                        .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Nickname));
                });
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source { Id = 5, CodeName = "007", Nickname = "Jimmy", ScreenName = "jbogard" });
            }

            [Fact]
            public void Should_consider_the_configuration_valid_even_if_some_properties_with_an_inaccessible_setter_are_unmapped()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }

            [Fact]
            public void Should_map_a_property_with_an_inaccessible_setter_if_a_specific_mapping_is_configured_after_the_ignore_method()
            {
                _destination.Nickname.ShouldEqual("Jimmy");
            }

            [Fact]
            public void Should_not_map_a_property_with_an_inaccessible_setter_if_no_specific_mapping_is_configured_even_though_name_and_type_match()
            {
                _destination.CodeName.ShouldBeNull();
            }

            [Fact]
            public void Should_not_map_a_property_with_no_public_setter_if_a_specific_mapping_is_configured_before_the_ignore_method()
            {
                _destination.ScreenName.ShouldBeNull();
            }
        }

    }
}