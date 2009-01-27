using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace CustomMapping
	{
		public class When_mapping_to_a_dto_member_with_custom_mapping : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public int Value { get; set; }
				public int Value2fff { get; set; }
				public int Value3 { get; set; }
				public int Value4 { get; set; }
			}

			private class ModelDto
			{
				public int Value { get; set; }
				public int Value2 { get; set; }
				public int Value3 { get; set; }
				public int Value4 { get; set; }
			}

			public class CustomResolver : IValueResolver
			{
				public object Resolve(object model)
				{
					return ((ModelObject)model).Value + 1;
				}
			}

			public class CustomResolver2 : IValueResolver
			{
				public object Resolve(object model)
				{
					return ((ModelObject)model).Value2fff + 2;
				}
			}

			public class CustomResolver3 : IValueResolver
			{
				public object Resolve(object model)
				{
					return ((ModelObject)model).Value4 + 4;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.Value, opt => opt.ResolveUsing<CustomResolver>())
					.ForMember(dto => dto.Value2, opt => opt.ResolveUsing(new CustomResolver2()))
					.ForMember(dto => dto.Value4, opt => opt.ResolveUsing(typeof(CustomResolver3)));
				
				var model = new ModelObject { Value = 42, Value2fff = 42, Value3 = 42, Value4 = 42};
				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_ignore_the_mapping_for_normal_members()
			{
				_result.Value3.ShouldEqual(42);
			}

			[Test]
			public void Should_use_the_custom_generic_mapping_for_custom_dto_members()
			{
				_result.Value.ShouldEqual(43);
			}

			[Test]
			public void Should_use_the_instance_based_mapping_for_custom_dto_members()
			{
				_result.Value2.ShouldEqual(44);
			}

			[Test]
			public void Should_use_the_type_object_based_mapping_for_custom_dto_members()
			{
				_result.Value4.ShouldEqual(46);
			}
		}

		public class When_using_a_custom_resolver_for_a_child_model_property_instead_of_the_model : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public ModelSubObject Sub { get; set; }
			}

			private class ModelSubObject
			{
				public int SomeValue { get; set; }
			}

			private class ModelDto
			{
				public int SomeValue { get; set; }
			}

			private class CustomResolver : IValueResolver
			{
				public object Resolve(object model)
				{
					return ((ModelSubObject)model).SomeValue + 1;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.SomeValue, opt => opt.ResolveUsing<CustomResolver>().FromMember(m => m.Sub));
				
				var model = new ModelObject
				{
					Sub = new ModelSubObject
					{
						SomeValue = 46
					}
				};

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_use_the_specified_model_member_to_resolve_from()
			{
				_result.SomeValue.ShouldEqual(47);
			}
		}

		public class When_reseting_a_mapping_to_use_a_resolver_to_a_different_member : AutoMapperSpecBase
		{
			private Dest _result;

			private class Source
			{
				public int SomeValue { get; set; }
				public int SomeOtherValue { get; set; }
			}

			private class Dest
			{
				public int SomeValue { get; set; }
			}

			private class CustomResolver : IValueResolver
			{
				public object Resolve(object model)
				{
					return ((int) model) + 5;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Dest>()
					.ForMember(dto => dto.SomeValue, opt => opt.ResolveUsing<CustomResolver>().FromMember(m => m.SomeOtherValue));

				var model = new Source
					{
						SomeValue = 36,
						SomeOtherValue = 53
					};

				_result = Mapper.Map<Source, Dest>(model);
			}

			[Test]
			public void Should_override_the_existing_match_to_the_new_custom_resolved_member()
			{
				_result.SomeValue.ShouldEqual(58);
			}
		}

	}

}