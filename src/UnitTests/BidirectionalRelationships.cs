using System.Collections.Generic;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
    namespace BidirectionalRelationships
    {
        public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship : AutoMapperSpecBase
        {
            private ParentDto _dto;

            protected override void Establish_context()
            {
                Mapper.CreateMap<ParentModel, ParentDto>();
                Mapper.CreateMap<ChildModel, ChildDto>();
                Mapper.AssertConfigurationIsValid();
            }

            protected override void Because_of()
            {
                var parent = new ParentModel { ID = "PARENT_ONE" };

                parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

                _dto = Mapper.Map<ParentModel, ParentDto>(parent);
            }

            [Test]
            public void Should_preserve_the_parent_child_relationship_on_the_destination()
            {
                _dto.Children[0].Parent.ShouldBeTheSameAs(_dto);
            }

            private class ParentModel
            {
                public ParentModel()
                {
                    Children = new List<ChildModel>();
                }

                public string ID { get; set; }

                public IList<ChildModel> Children { get; private set; }

                public void AddChild(ChildModel child)
                {
                    child.Parent = this;
                    Children.Add(child);
                }
            }

            private class ChildModel
            {
                public string ID { get; set; }
                public ParentModel Parent { get; set; }
            }

            private class ParentDto
            {
                public string ID { get; set; }
                public IList<ChildDto> Children { get; protected set; }
            }

            private class ChildDto
            {
                public string ID { get; set; }
                public ParentDto Parent { get; protected set; }
            }
        }

        public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_one_child_relationship : AutoMapperSpecBase
        {
            private FooDto _dto;

            protected override void Establish_context()
            {
                Mapper.CreateMap<Foo, FooDto>();
                Mapper.CreateMap<Bar, BarDto>();
                Mapper.AssertConfigurationIsValid();
            }

            protected override void Because_of()
            {
                var foo = new Foo
                {
                    Bar = new Bar
                    {
                        Value = "something"
                    }
                };
                foo.Bar.Foo = foo;
                _dto = Mapper.Map<Foo, FooDto>(foo);
            }

            [Test]
            public void Should_preserve_the_parent_child_relationship_on_the_destination()
            {
                _dto.Bar.Foo.ShouldBeTheSameAs(_dto);
            }

            private class Foo
            {
                public Bar Bar { get; set; }
            }

            private class Bar
            {
                public Foo Foo { get; set; }
                public string Value { get; set; }
            }

            private class FooDto
            {
                public BarDto Bar { get; set; }
            }

            private class BarDto
            {
                public FooDto Foo { get; set; }
                public string Value { get; set; }
            }
        }

        public class When_mapping_to_a_destination_containing_two_dtos_mapped_from_the_same_source : AutoMapperSpecBase
        {
            private FooContainerModel _dto;

            protected override void Establish_context()
            {
                Mapper.CreateMap<FooModel, FooScreenModel>();
                Mapper.CreateMap<FooModel, FooInputModel>();
                Mapper.CreateMap<FooModel, FooContainerModel>()
                    .ForMember(dest => dest.Input, opt => opt.MapFrom(src => src))
                    .ForMember(dest => dest.Screen, opt => opt.MapFrom(src => src));
                Mapper.AssertConfigurationIsValid();
            }

            protected override void Because_of()
            {
                var model = new FooModel { Id = 3 };
                _dto = Mapper.Map<FooModel, FooContainerModel>(model);
            }

            [Test]
            public void Should_not_preserve_identity_when_destinations_are_incompatible()
            {
                Assert.IsInstanceOfType(typeof(FooContainerModel), _dto);
                Assert.IsInstanceOfType(typeof(FooInputModel), _dto.Input);
                Assert.IsInstanceOfType(typeof(FooScreenModel), _dto.Screen);
                Assert.AreEqual(_dto.Input.Id, 3);
                Assert.AreEqual(_dto.Screen.Id, "3");
            }

            private class FooContainerModel
            {
                public FooInputModel Input { get; set; }
                public FooScreenModel Screen { get; set; }
            }

            private class FooScreenModel
            {
                public string Id { get; set; }
            }

            private class FooInputModel
            {
                public long Id { get; set; }
            }

            private class FooModel
            {
                public long Id { get; set; }
            }
        }
    }
}
