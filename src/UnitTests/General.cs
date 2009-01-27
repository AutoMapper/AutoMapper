using System;
using System.Collections.Generic;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace General
	{
		public class When_mapping_dto_with_a_missing_match : AutoMapperSpecBase
		{
			public class ModelObject
			{
			}

			public class ModelDto
			{
				public string SomePropertyThatDoesNotExistOnModel { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_map_successfully()
			{
				ModelDto dto = Mapper.Map<ModelObject, ModelDto>(new ModelObject());

				dto.ShouldNotBeNull();
			}
		}

		public class When_mapping_a_null_model : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelDto
			{
			}

			private class ModelObject
			{
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(null);
			}

			[Test]
			public void Should_always_provide_a_dto()
			{
				_result.ShouldNotBeNull();
			}
		}

		public class When_mapping_a_model_with_null_items : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelDto
			{
				public ModelSubDto Sub { get; set; }
				public int SubSomething { get; set; }
				public string NullString { get; set; }
			}

			private class ModelSubDto
			{
				public int[] Items { get; set; }
			}

			private class ModelObject
			{
				public ModelSubObject Sub { get; set; }
				public string NullString { get; set; }
			}

			private class ModelSubObject
			{
				public int[] GetItems()
				{
					return new[] { 0, 1, 2, 3 };
				}

				public int Something { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject();
				model.Sub = null;

				Mapper.CreateMap<ModelObject, ModelDto>();
				Mapper.CreateMap<ModelSubObject, ModelSubDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_populate_dto_items_with_a_value()
			{
				_result.Sub.ShouldNotBeNull();
			}

			[Test]
			public void Should_provide_empty_array_for_array_type_values()
			{
				_result.Sub.Items.ShouldNotBeNull();
			}

			[Test]
			public void Should_return_default_value_of_property_in_the_chain()
			{
				_result.SubSomething.ShouldEqual(0);
			}

			[Test]
			public void Default_value_for_string_should_be_empty()
			{
				_result.NullString.ShouldEqual(string.Empty);
			}
		}

		public class When_mapping_a_dto_with_a_private_parameterless_constructor : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public string SomeValue { get; set; }
			}

			private class ModelDto
			{
				public string SomeValue { get; set; }

				private ModelDto()
				{
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject
				{
					SomeValue = "Some value"
				};

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_the_dto_value()
			{
				_result.SomeValue.ShouldEqual("Some value");
			}
		}

		public class When_mapping_to_a_dto_string_property_and_the_dto_type_is_not_a_string : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public int NotAString { get; set; }
			}

			private class ModelDto
			{
				public string NotAString { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject
				{
					NotAString = 5
				};

				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_use_the_ToString_value_of_the_unmatched_type()
			{
				_result.NotAString.ShouldEqual("5");
			}
		}

		public class When_mapping_dto_with_an_array_property : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public IEnumerable<int> GetSomeCoolValues()
				{
					return new[] { 4, 5, 6 };
				}
			}

			private class ModelDto
			{
				public string[] SomeCoolValues { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject();

				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_the_collection_of_items_in_the_input_to_the_array()
			{
				_result.SomeCoolValues[0].ShouldEqual("4");
				_result.SomeCoolValues[1].ShouldEqual("5");
				_result.SomeCoolValues[2].ShouldEqual("6");
			}
		}

		public class When_mapping_a_dto_with_mismatched_property_types : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public DateTime? NullableDate { get; set; }
			}

			private class ModelDto
			{
				public DateTime NullableDate { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_throw_a_mapping_exception()
			{
				var model = new ModelObject();
				model.NullableDate = new DateTime(2007, 8, 4);
				
				typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<ModelObject, ModelDto>(model));
			}
		}

		public class When_mapping_an_array_of_model_objects : AutoMapperSpecBase
		{
			private ModelObject[] _model;
			private ModelDto[] _dto;

			public class ModelObject
			{
				public string SomeValue { get; set; }
			}

			public class ModelDto
			{
				public string SomeValue { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();

				_model = new[] { new ModelObject { SomeValue = "First" }, new ModelObject { SomeValue = "Second" } };
				_dto = (ModelDto[])Mapper.Map(_model, typeof(ModelObject[]), typeof(ModelDto[]));
			}

			[Test]
			public void Should_create_an_array_of_ModelDto_objects()
			{
				_dto.Length.ShouldEqual(2);
			}

			[Test]
			public void Should_map_properties()
			{
				Array.Find(_dto, d => d.SomeValue.Contains("First")).ShouldNotBeNull();
				Array.Find(_dto, d => d.SomeValue.Contains("Second")).ShouldNotBeNull();
			}
		}

	}
}