using System;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace MemberResolution
	{
		public class When_mapping_derived_classes : AutoMapperSpecBase
		{
			private DtoObject[] _result;

			public class ModelObject
			{
				public string BaseString { get; set; }
			}

			public class ModelSubObject : ModelObject
			{
				public string SubString { get; set; }
			}

			public class DtoObject
			{
				public string BaseString { get; set; }
			}

			public class DtoSubObject : DtoObject
			{
				public string SubString { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.Reset();

				var model = new[]
            	{
            		new ModelObject { BaseString = "Base1" }, 
					new ModelSubObject { BaseString = "Base2", SubString = "Sub2"}
            	};

				Mapper
					.CreateMap<ModelObject, DtoObject>()
					.Include<ModelSubObject, DtoSubObject>();

				Mapper.CreateMap<ModelSubObject, DtoSubObject>();

				_result = (DtoObject[])Mapper.Map(model, typeof(ModelObject[]), typeof(DtoObject[]));
			}

			[Test]
			public void Should_map_both_the_base_and_sub_objects()
			{
				_result.Length.ShouldEqual(2);
				_result[0].BaseString.ShouldEqual("Base1");
				_result[1].BaseString.ShouldEqual("Base2");
			}

			[Test]
			public void Should_map_to_the_correct_respective_dto_types()
			{
				_result[0].ShouldBeInstanceOfType(typeof(DtoObject));
				_result[1].ShouldBeInstanceOfType(typeof(DtoSubObject));
			}
		}

		public class When_mapping_dto_with_only_properties : AutoMapperSpecBase
		{
			private ModelDto _result;

			public class ModelObject
			{
				public DateTime BaseDate { get; set; }
				public ModelSubObject Sub { get; set; }
				public ModelSubObject Sub2 { get; set; }
				public ModelSubObject SubWithExtraName { get; set; }
				public ModelSubObject SubMissing { get; set; }
			}

			public class ModelSubObject
			{
				public string ProperName { get; set; }
				public ModelSubSubObject SubSub { get; set; }
			}

			public class ModelSubSubObject
			{
				public string IAmACoolProperty { get; set; }
			}

			public class ModelDto
			{
				public DateTime BaseDate { get; set; }
				public DateTime BaseDate2 { get; set; }
				public string SubProperName { get; set; }
				public string Sub2ProperName { get; set; }
				public string SubWithExtraNameProperName { get; set; }
				public string SubSubSubIAmACoolProperty { get; set; }
				public string SubMissingSubSubIAmACoolProperty { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.Reset();

				var model = new ModelObject
				{
					BaseDate = new DateTime(2007, 4, 5),
					Sub = new ModelSubObject
					{
						ProperName = "Some name",
						SubSub = new ModelSubSubObject
						{
							IAmACoolProperty = "Cool daddy-o"
						}
					},
					Sub2 = new ModelSubObject
					{
						ProperName = "Sub 2 name"
					},
					SubWithExtraName = new ModelSubObject
					{
						ProperName = "Some other name"
					},
					SubMissing = new ModelSubObject
					{
						ProperName = "I have a missing sub sub object"
					}
				};

				Mapper.CreateMap<ModelObject, ModelDto>();
				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_item_in_first_level_of_hierarchy()
			{
				_result.BaseDate.ShouldEqual(new DateTime(2007, 4, 5));
			}

			[Test]
			public void Should_map_a_member_with_a_number()
			{
				_result.Sub2ProperName.ShouldEqual("Sub 2 name");
			}

			[Test]
			public void Should_map_item_in_second_level_of_hierarchy()
			{
				_result.SubProperName.ShouldEqual("Some name");
			}

			[Test]
			public void Should_map_item_with_more_items_in_property_name()
			{
				_result.SubWithExtraNameProperName.ShouldEqual("Some other name");
			}

			[Test]
			public void Should_map_item_in_any_level_of_depth_in_the_hierarchy()
			{
				_result.SubSubSubIAmACoolProperty.ShouldEqual("Cool daddy-o");
			}
		}

		public class When_ignoring_a_dto_property_during_configuration : AutoMapperSpecBase
		{
			private TypeMap[] _allTypeMaps;
			private Source _source;

			private class Source
			{
				public string Value { get; set; }
			}

			private class Destination
			{
				public string Ignored { get; set; }
				public string Value { get; set; }
			}

			[Test]
			public void Should_not_report_it_as_unmapped()
			{
				Array.ForEach(_allTypeMaps, t => t.GetUnmappedPropertyNames().ShouldBeOfLength(0));
			}

			[Test]
			public void Should_map_successfully()
			{
				Mapper.Map<Source, Destination>(_source).Value.ShouldEqual("foo");
			}

			protected override void Establish_context()
			{
				_source = new Source {Value = "foo"};
				Mapper.CreateMap<Source, Destination>()
					.ForMember(x => x.Ignored, opt => opt.Ignore());
				_allTypeMaps = Mapper.GetAllTypeMaps();
			}
		}

		public class When_mapping_dto_with_get_methods : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public string GetSomeCoolValue()
				{
					return "Cool value";
				}

				public ModelSubObject Sub { get; set; }
			}

			private class ModelSubObject
			{
				public string GetSomeOtherCoolValue()
				{
					return "Even cooler";
				}
			}

			private class ModelDto
			{
				public string SomeCoolValue { get; set; }
				public string SubSomeOtherCoolValue { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject
				{
					Sub = new ModelSubObject()
				};

				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_base_method_value()
			{
				_result.SomeCoolValue.ShouldEqual("Cool value");
			}

			[Test]
			public void Should_map_second_level_method_value_off_of_property()
			{
				_result.SubSomeOtherCoolValue.ShouldEqual("Even cooler");
			}
		}

		public class When_mapping_a_dto_with_names_matching_properties : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public string SomeCoolValue()
				{
					return "Cool value";
				}

				public ModelSubObject Sub { get; set; }
			}

			private class ModelSubObject
			{
				public string SomeOtherCoolValue()
				{
					return "Even cooler";
				}
			}

			private class ModelDto
			{
				public string SomeCoolValue { get; set; }
				public string SubSomeOtherCoolValue { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject
				{
					Sub = new ModelSubObject()
				};

				Mapper.CreateMap<ModelObject, ModelDto>();

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_base_method_value()
			{
				_result.SomeCoolValue.ShouldEqual("Cool value");
			}

			[Test]
			public void Should_map_second_level_method_value_off_of_property()
			{
				_result.SubSomeOtherCoolValue.ShouldEqual("Even cooler");
			}
		}

		public class When_mapping_with_a_dto_subtype : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public ModelSubObject Sub { get; set; }
			}

			private class ModelSubObject
			{
				public string SomeValue { get; set; }
			}

			private class ModelDto
			{
				public ModelSubDto Sub { get; set; }
			}

			private class ModelSubDto
			{
				public string SomeValue { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();
				Mapper.CreateMap<ModelSubObject, ModelSubDto>();

				var model = new ModelObject
				{
					Sub = new ModelSubObject
					{
						SomeValue = "Some value"
					}
				};

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_the_model_sub_type_to_the_dto_sub_type()
			{
				_result.Sub.ShouldNotBeNull();
				_result.Sub.SomeValue.ShouldEqual("Some value");
			}
		}

		public class When_mapping_a_dto_with_a_set_only_property_and_a_get_method : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelDto
			{
				public int SomeValue { get; set; }
			}

			private class ModelObject
			{
				private int _someValue;

				public int SomeValue
				{
					set { _someValue = value; }
				}

				public int GetSomeValue()
				{
					return _someValue;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>();

				var model = new ModelObject();
				model.SomeValue = 46;

				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_map_the_get_method_to_the_dto()
			{
				_result.SomeValue.ShouldEqual(46);
			}
		}

		public class When_mapping_using_a_custom_member_mappings : AutoMapperSpecBase
		{
			private ModelDto _result;

			private class ModelObject
			{
				public int Blarg { get; set; }
				public string MoreBlarg { get; set; }

				public int SomeMethodToGetMoreBlarg()
				{
					return 45;
				}

				public string SomeValue { get; set; }
				public ModelSubObject SomeWeirdSubObject { get; set; }

				public string IAmSomeMethod()
				{
					return "I am some method";
				}
			}

			private class ModelSubObject
			{
				public int Narf { get; set; }
				public ModelSubSubObject SubSub { get; set; }

				public string SomeSubValue()
				{
					return "I am some sub value";
				}
			}

			private class ModelSubSubObject
			{
				public int Norf { get; set; }

				public string SomeSubSubValue()
				{
					return "I am some sub sub value";
				}
			}

			private class ModelDto
			{
				public int Splorg { get; set; }
				public string SomeValue { get; set; }
				public string SomeMethod { get; set; }
				public int SubNarf { get; set; }
				public string SubValue { get; set; }
				public int GrandChildInt { get; set; }
				public string GrandChildString { get; set; }
				public string BlargBucks { get; set; }
				public int MoreBlarg { get; set; }
			}

			protected override void Establish_context()
			{
				var model = new ModelObject
				{
					Blarg = 10,
					SomeValue = "Some value",
					SomeWeirdSubObject = new ModelSubObject
					{
						Narf = 5,
						SubSub = new ModelSubSubObject
						{
							Norf = 15
						}
					},
					MoreBlarg = "adsfdsaf"
				};
				Mapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.Splorg, opt => opt.MapFrom(m => m.Blarg))
					.ForMember(dto => dto.SomeMethod, opt => opt.MapFrom(m => m.IAmSomeMethod()))
					.ForMember(dto => dto.SubNarf, opt => opt.MapFrom(m => m.SomeWeirdSubObject.Narf))
					.ForMember(dto => dto.SubValue, opt => opt.MapFrom(m => m.SomeWeirdSubObject.SomeSubValue()))
					.ForMember(dto => dto.GrandChildInt, opt => opt.MapFrom(m => m.SomeWeirdSubObject.SubSub.Norf))
					.ForMember(dto => dto.GrandChildString, opt => opt.MapFrom(m => m.SomeWeirdSubObject.SubSub.SomeSubSubValue()))
					.ForMember(dto => dto.MoreBlarg, opt => opt.MapFrom(m => m.SomeMethodToGetMoreBlarg()));


				_result = Mapper.Map<ModelObject, ModelDto>(model);
			}

			[Test]
			public void Should_preserve_the_existing_mapping()
			{
				_result.SomeValue.ShouldEqual("Some value");
			}

			[Test]
			public void Should_map_top_level_properties()
			{
				_result.Splorg.ShouldEqual(10);
			}

			[Test]
			public void Should_map_methods_results()
			{
				_result.SomeMethod.ShouldEqual("I am some method");
			}

			[Test]
			public void Should_map_children_properties()
			{
				_result.SubNarf.ShouldEqual(5);
			}

			[Test]
			public void Should_map_children_methods()
			{
				_result.SubValue.ShouldEqual("I am some sub value");
			}

			[Test]
			public void Should_map_grandchildren_properties()
			{
				_result.GrandChildInt.ShouldEqual(15);
			}

			[Test]
			public void Should_map_grandchildren_methods()
			{
				_result.GrandChildString.ShouldEqual("I am some sub sub value");
			}

			[Test]
			public void Should_override_existing_matches_for_new_mappings()
			{
				_result.MoreBlarg.ShouldEqual(45);
			}
		}
	}
}