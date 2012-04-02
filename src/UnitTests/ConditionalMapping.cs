using System;
using System.Linq;
using NUnit.Framework;
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

            [Test]
            public void Should_skip_the_mapping_when_the_condition_is_true()
            {
                var destination = Mapper.Map<Source, Destination>(new Source {Value = -1});

                destination.Value.ShouldEqual(0);
            }

            [Test]
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
                            opt.ResolveUsing(src =>
                            {
                                throw new Exception("Blarg");
                                return 5;
                            });
                        });
                });
            }

            [Test]
            public void Should_skip_the_mapping_when_the_condition_is_true()
            {
                var destination = Mapper.Map<Source, Destination>(new Source { Value = -1 });

                destination.Value.ShouldEqual(0);
            }

            [Test]
            public void Should_execute_the_mapping_when_the_condition_is_false()
            {
                typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Destination>(new Source { Value = 7 }));
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

            [Test]
            public void Should_include_the_normal_members_by_default()
            {
                _destination.Value.ShouldEqual(5);
            }

            [Test]
            public void Should_skip_any_members_based_on_the_skip_condition()
            {
                _destination.Value2.ShouldEqual(default(int));
            }

            public class SkipAttribute : Attribute { }
        }

    }
}