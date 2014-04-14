using System;
using System.Collections.Generic;
using Should;
using Xunit;
using System.Linq;

namespace AutoMapper.UnitTests
{
	namespace General
	{
		public class When_mapping_dto_with_a_missing_match : NonValidatingSpecBase
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

			[Fact]
			public void Should_map_successfully()
			{
				ModelDto dto = Mapper.Map<ModelObject, ModelDto>(new ModelObject());

				dto.ShouldNotBeNull();
			}
		}

		public class When_mapping_a_null_model : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelDto
			{
			}

			public class ModelObject
			{
			}

			protected override void Establish_context()
			{
				Mapper.AllowNullDestinationValues = false;
				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(null);
			}

			[Fact]
			public void Should_always_provide_a_dto()
			{
				_result.ShouldNotBeNull();
			}
		}

		public class When_mapping_a_dto_with_a_private_parameterless_constructor : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public string SomeValue { get; set; }
			}

			public class ModelDto
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

			[Fact]
			public void Should_map_the_dto_value()
			{
				_result.SomeValue.ShouldEqual("Some value");
			}
		}

		public class When_mapping_to_a_dto_string_property_and_the_dto_type_is_not_a_string : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public int NotAString { get; set; }
			}

			public class ModelDto
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

			[Fact]
			public void Should_use_the_ToString_value_of_the_unmatched_type()
			{
				_result.NotAString.ShouldEqual("5");
			}
		}

		public class When_mapping_dto_with_an_array_property : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public IEnumerable<int> GetSomeCoolValues()
				{
					return new[] { 4, 5, 6 };
				}
			}

			public class ModelDto
			{
				public string[] SomeCoolValues { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject();

				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Fact]
			public void Should_map_the_collection_of_items_in_the_input_to_the_array()
			{
				_result.SomeCoolValues[0].ShouldEqual("4");
				_result.SomeCoolValues[1].ShouldEqual("5");
				_result.SomeCoolValues[2].ShouldEqual("6");
			}
		}

		public class When_mapping_a_dto_with_mismatched_property_types : NonValidatingSpecBase
		{
			public class ModelObject
			{
				public string NullableDate { get; set; }
			}

			public class ModelDto
			{
				public DateTime NullableDate { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();
			}

			[Fact]
			public void Should_throw_a_mapping_exception()
			{
				var model = new ModelObject();
				model.NullableDate = "Lorem Ipsum";
				
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

			[Fact]
			public void Should_create_an_array_of_ModelDto_objects()
			{
				_dto.Length.ShouldEqual(2);
			}

			[Fact]
			public void Should_map_properties()
			{
                _dto.Any(d => d.SomeValue.Contains("First")).ShouldBeTrue();
                _dto.Any(d => d.SomeValue.Contains("Second")).ShouldBeTrue();
            }
		}

		public class When_mapping_a_List_of_model_objects : AutoMapperSpecBase
		{
			private List<ModelObject> _model;
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

				_model = new List<ModelObject> { new ModelObject { SomeValue = "First" }, new ModelObject { SomeValue = "Second" } };
				_dto = (ModelDto[])Mapper.Map(_model, typeof(ModelObject[]), typeof(ModelDto[]));
			}

			[Fact]
			public void Should_create_an_array_of_ModelDto_objects()
			{
				_dto.Length.ShouldEqual(2);
			}

			[Fact]
			public void Should_map_properties()
			{
                _dto.Any(d => d.SomeValue.Contains("First")).ShouldBeTrue();
                _dto.Any(d => d.SomeValue.Contains("Second")).ShouldBeTrue();
			}
		}

		public class When_mapping_a_nullable_type_to_non_nullable_type : AutoMapperSpecBase
		{
			private ModelObject _model;
			private ModelDto _dto;

			public class ModelObject
			{
				public int? SomeValue { get; set; }
				public int? SomeNullableValue { get; set; }
			}

			public class ModelDto
			{
				public int SomeValue { get; set; }
				public int SomeNullableValue { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();
			}

            protected override void Because_of()
            {
                _model = new ModelObject { SomeValue = 2 };
                _dto = Mapper.Map<ModelObject, ModelDto>(_model);
            }

			[Fact]
			public void Should_map_value_if_has_value()
			{
				_dto.SomeValue.ShouldEqual(2);
			}

			[Fact]
			public void Should_not_set_value_if_null()
			{
				_dto.SomeNullableValue.ShouldEqual(0);
			}
		}

		public class When_mapping_a_non_nullable_type_to_a_nullable_type : AutoMapperSpecBase
		{
			private ModelObject _model;
			private ModelDto _dto;

			public class ModelObject
			{
				public int SomeValue { get; set; }
				public int SomeOtherValue { get; set; }
			}

			public class ModelDto
			{
				public int? SomeValue { get; set; }
				public int? SomeOtherValue { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();

				_model = new ModelObject { SomeValue = 2 };
			}

			protected override void Because_of()
			{
				_dto = Mapper.Map<ModelObject, ModelDto>(_model);
			}

			[Fact]
			public void Should_map_value_if_has_value()
			{
				_dto.SomeValue.ShouldEqual(2);
			}

			[Fact]
			public void Should_not_set_value_if_null()
			{
				_dto.SomeOtherValue.ShouldEqual(0);
			}

		}

        public class When_mapping_a_nullable_type_to_a_nullable_type : AutoMapperSpecBase
        {
            private ModelObject _model;
            private ModelDto _dto;

            public class ModelObject
            {
                public int? SomeValue { get; set; }
                public int? SomeOtherValue { get; set; }
            }

            public class ModelDto
            {
                public int? SomeValue { get; set; }
                public int? SomeOtherValue2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dest => dest.SomeOtherValue2, opt => opt.MapFrom(src => src.SomeOtherValue));

                _model = new ModelObject();
            }

            protected override void Because_of()
            {
                _dto = Mapper.Map<ModelObject, ModelDto>(_model);
            }

            [Fact]
            public void Should_map_value_if_has_value()
            {
                _dto.SomeValue.ShouldBeNull();
            }

            [Fact]
            public void Should_not_set_value_if_null()
            {
                _dto.SomeOtherValue2.ShouldBeNull();
            }

        }

	    public class When_mapping_tuples : AutoMapperSpecBase
	    {
	        private Dest _dest;

	        public class Source
	        {
	            public Tuple<int, int> Value { get; set; }
	        }
	        public class Dest
	        {
	            public Tuple<int, int> Value { get; set; }
	        }

	        protected override void Establish_context()
	        {
	            Mapper.CreateMap<Source, Dest>();
	        }

	        protected override void Because_of()
	        {
	            var source = new Source
	            {
	                Value = new Tuple<int, int>(10, 11)
	            };
	            _dest = Mapper.Map<Source, Dest>(source);
	        }

	        [Fact]
	        public void Should_map_tuple()
	        {
	            _dest.Value.ShouldNotBeNull();
                _dest.Value.Item1.ShouldEqual(10);
                _dest.Value.Item2.ShouldEqual(11);
	        }
	    }
    }
}
