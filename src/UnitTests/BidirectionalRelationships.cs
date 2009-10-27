using System.Collections.Generic;
using System.Linq;

using NBehave.Spec.NUnit;

using NUnit.Framework;

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
