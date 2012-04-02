using System;
using System.Collections.Generic;
using System.Linq;

using Should;

using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace BidirectionalRelationships
	{
        public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship : AutoMapperSpecBase
        {
            private ParentDto _dto;
            private int _beforeMapCount = 0;
            private int _afterMapCount = 0;

            protected override void Establish_context()
            {
                Mapper.CreateMap<ParentModel, ParentDto>()
                    .BeforeMap((src, dest) => _beforeMapCount++)
                    .AfterMap((src, dest) => _afterMapCount++);
                Mapper.CreateMap<ChildModel, ChildDto>();
                Mapper.AssertConfigurationIsValid();
            }

            protected override void Because_of()
            {
                var parent = new ParentModel { ID = "PARENT_ONE" };

                parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

                parent.AddChild(new ChildModel { ID = "CHILD_TWO" });

                _dto = Mapper.Map<ParentModel, ParentDto>(parent);
            }

            [Test]
            public void Should_preserve_the_parent_child_relationship_on_the_destination()
            {
                _dto.Children[0].Parent.ShouldBeSameAs(_dto);
                _dto.Children[1].Parent.ShouldBeSameAs(_dto);
            }

            [Test]
            public void Before_and_After_for_the_parent_should_be_called_once()
            {
                _beforeMapCount.ShouldEqual(1);
                _afterMapCount.ShouldEqual(1);
            }

            public class ParentModel
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

            public class ChildModel
            {
                public string ID { get; set; }
                public ParentModel Parent { get; set; }
            }

            public class ParentDto
            {
                public string ID { get; set; }
                public IList<ChildDto> Children { get; set; }
            }

            public class ChildDto
            {
                public string ID { get; set; }
                public ParentDto Parent { get; set; }
            }
        }


		[Ignore("This test breaks the Test Runner")]
		public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_StackOverflow : AutoMapperSpecBase
		{
			private ParentDto _dto;
			private ParentModel _parent;

			protected override void Establish_context()
			{
				_parent = new ParentModel
					{
						ID = 2
					};

				List<ChildModel> childModels = new List<ChildModel>
					{
						new ChildModel
							{
								ID = 1,
								Parent = _parent
							}
					};

				Dictionary<int, ParentModel> parents = childModels.ToDictionary(x => x.ID, x => x.Parent);

				Mapper.CreateMap<int, ParentDto>().ConvertUsing(new ChildIdToParentDtoConverter(parents));
				Mapper.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));

				Mapper.CreateMap<ParentModel, ParentDto>()
					.ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ID));
				Mapper.CreateMap<ChildModel, ChildDto>();

				Mapper.AssertConfigurationIsValid();
			}

			protected override void Because_of()
			{
				_dto = Mapper.Map<ParentModel, ParentDto>(_parent);
			}

			[Test]
			public void Should_preserve_the_parent_child_relationship_on_the_destination()
			{
				_dto.Children[0].Parent.ID.ShouldEqual(_dto.ID);
			}

			public class ChildIdToParentDtoConverter : TypeConverter<int, ParentDto>
			{
				private readonly Dictionary<int, ParentModel> _parentModels;

				public ChildIdToParentDtoConverter(Dictionary<int, ParentModel> parentModels)
				{
					_parentModels = parentModels;
				}

				protected override ParentDto ConvertCore(int childId)
				{
					ParentModel parentModel = _parentModels[childId];
					MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
					return mappingEngine.Map<ParentModel, ParentDto>(parentModel);
				}
			}

			public class ParentIdToChildDtoListConverter : TypeConverter<int, List<ChildDto>>
			{
				private readonly IList<ChildModel> _childModels;

				public ParentIdToChildDtoListConverter(IList<ChildModel> childModels)
				{
					_childModels = childModels;
				}

				protected override List<ChildDto> ConvertCore(int childId)
				{
					List<ChildModel> childModels = _childModels.Where(x => x.Parent.ID == childId).ToList();
					MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
					return mappingEngine.Map<List<ChildModel>, List<ChildDto>>(childModels);
				}
			}

			public class ParentModel
			{
				public int ID { get; set; }
			}

			public class ChildModel
			{
				public int ID { get; set; }
				public ParentModel Parent { get; set; }
			}

			public class ParentDto
			{
				public int ID { get; set; }
				public List<ChildDto> Children { get; set; }
			}

			public class ChildDto
			{
				public int ID { get; set; }
				public ParentDto Parent { get; set; }
			}
		}

		public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_with_context : AutoMapperSpecBase
		{
			private ParentDto _dto;
			private ParentModel _parent;

			protected override void Establish_context()
			{
				_parent = new ParentModel
					{
						ID = 2
					};

				List<ChildModel> childModels = new List<ChildModel>
					{
						new ChildModel
							{
								ID = 1,
								Parent = _parent
							}
					};

				Dictionary<int, ParentModel> parents = childModels.ToDictionary(x => x.ID, x => x.Parent);

				Mapper.CreateMap<int, ParentDto>().ConvertUsing(new ChildIdToParentDtoConverter(parents));
				Mapper.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));

				Mapper.CreateMap<ParentModel, ParentDto>()
					.ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ID));
				Mapper.CreateMap<ChildModel, ChildDto>();

				Mapper.AssertConfigurationIsValid();
			}

			protected override void Because_of()
			{
				_dto = Mapper.Map<ParentModel, ParentDto>(_parent);
			}

			[Test]
			public void Should_preserve_the_parent_child_relationship_on_the_destination()
			{
				_dto.Children[0].Parent.ID.ShouldEqual(_dto.ID);
			}

			public class ChildIdToParentDtoConverter : ITypeConverter<int, ParentDto>
			{
				private readonly Dictionary<int, ParentModel> _parentModels;

				public ChildIdToParentDtoConverter(Dictionary<int, ParentModel> parentModels)
				{
					_parentModels = parentModels;
				}

				public ParentDto Convert(ResolutionContext resolutionContext)
				{
					int childId = (int) resolutionContext.SourceValue;
					ParentModel parentModel = _parentModels[childId];
					MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
					return mappingEngine.Map<ParentModel, ParentDto>(resolutionContext, parentModel);
				}
			}

			public class ParentIdToChildDtoListConverter : ITypeConverter<int, List<ChildDto>>
			{
				private readonly IList<ChildModel> _childModels;

				public ParentIdToChildDtoListConverter(IList<ChildModel> childModels)
				{
					_childModels = childModels;
				}

				public List<ChildDto> Convert(ResolutionContext resolutionContext)
				{
					int childId = (int)resolutionContext.SourceValue;
					List<ChildModel> childModels = _childModels.Where(x => x.Parent.ID == childId).ToList();
					MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
					return mappingEngine.Map<List<ChildModel>, List<ChildDto>>(resolutionContext, childModels);
				}
			}

			public class ParentModel
			{
				public int ID { get; set; }
			}

			public class ChildModel
			{
				public int ID { get; set; }
				public ParentModel Parent { get; set; }
			}

			public class ParentDto
			{
				public int ID { get; set; }
				public List<ChildDto> Children { get; set; }
			}

			public class ChildDto
			{
				public int ID { get; set; }
				public ParentDto Parent { get; set; }
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
				_dto.Bar.Foo.ShouldBeSameAs(_dto);
			}

			public class Foo
			{
				public Bar Bar { get; set; }
			}

			public class Bar
			{
				public Foo Foo { get; set; }
				public string Value { get; set; }
			}

			public class FooDto
			{
				public BarDto Bar { get; set; }
			}

			public class BarDto
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

			public class FooContainerModel
			{
				public FooInputModel Input { get; set; }
				public FooScreenModel Screen { get; set; }
			}

			public class FooScreenModel
			{
				public string Id { get; set; }
			}

			public class FooInputModel
			{
				public long Id { get; set; }
			}

			public class FooModel
			{
				public long Id { get; set; }
			}
		}

	    public class When_mapping_with_a_bidirectional_relationship_that_includes_arrays : AutoMapperSpecBase

	    {
	        private ParentDto _dtoParent;

	        protected override void Establish_context()
            {
                var parent1 = new Parent { Name = "Parent 1" };
                var child1 = new Child { Name = "Child 1" };

                parent1.Children.Add(child1);
                child1.Parents.Add(parent1);

                Mapper.CreateMap<Parent, ParentDto>();
                Mapper.CreateMap<Child, ChildDto>();

	            _dtoParent = Mapper.Map<Parent, ParentDto>(parent1);
            }

	        [Test]
	        public void Should_map_successfully()
	        {
                object.ReferenceEquals(_dtoParent.Children[0].Parents[0], _dtoParent).ShouldBeTrue();
	        }

            public class Parent
            {
                public Guid Id { get; private set; }

                public string Name { get; set; }

                public List<Child> Children { get; set; }

                public Parent()
                {
                    Id = Guid.NewGuid();
                    Children = new List<Child>();
                }

                public bool Equals(Parent other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return other.Id.Equals(Id);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != typeof (Parent)) return false;
                    return Equals((Parent) obj);
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }
            }

            public class Child
            {
                public Guid Id { get; private set; }

                public string Name { get; set; }

                public List<Parent> Parents { get; set; }

                public Child()
                {
                    Id = Guid.NewGuid();
                    Parents = new List<Parent>();
                }

                public bool Equals(Child other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return other.Id.Equals(Id);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != typeof (Child)) return false;
                    return Equals((Child) obj);
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }
            }

            public class ParentDto
            {
                public Guid Id { get; set; }

                public string Name { get; set; }

                public List<ChildDto> Children { get; set; }

                public ParentDto()
                {
                    Children = new List<ChildDto>();
                }
            }

            public class ChildDto
            {
                public Guid Id { get; set; }

                public string Name { get; set; }

                public List<ParentDto> Parents { get; set; }

                public ChildDto()
                {
                    Parents = new List<ParentDto>();
                }
            }
	    }

    }
}
