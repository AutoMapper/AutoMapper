using System;
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
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((ModelObject)source.Value).Value + 1);
				}
			}

			public class CustomResolver2 : IValueResolver
			{
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((ModelObject)source.Value).Value2fff + 2);
				}
			}

			public class CustomResolver3 : IValueResolver
			{
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((ModelObject)source.Value).Value4 + 4);
				}

				public Type GetResolvedValueType()
				{
					return typeof (int);
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
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((ModelSubObject)source.Value).SomeValue + 1);
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
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((int)source.Value) + 5);
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

		public class When_reseting_a_mapping_from_a_property_to_a_method : AutoMapperSpecBase
		{
			private Dest _result;

			private class Source
			{
				public int Type { get; set; }
			}

			private class Dest
			{
				public int Type { get; set; }
			}

			private class CustomResolver : IValueResolver
			{
				public ResolutionResult Resolve(ResolutionResult source)
				{
					return new ResolutionResult(((int)source.Value) + 5);
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Dest>()
					.ForMember(dto => dto.Type, opt => opt.MapFrom(m => m.Type));

				var model = new Source
				{
					Type = 5 
				};

				_result = Mapper.Map<Source, Dest>(model);
			}

			[Test]
			public void Should_override_the_existing_match_to_the_new_custom_resolved_member()
			{
				_result.Type.ShouldEqual(5);
			}
		}

		public class When_specifying_a_custom_constructor_and_member_resolver : AutoMapperSpecBase
		{
			private Source _source;
			private Destination _dest;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

			public class CustomResolver : ValueResolver<int, int>
			{
				private readonly int _toAdd;

				public CustomResolver(int toAdd)
				{
					_toAdd = toAdd;
				}

				public CustomResolver()
				{
					_toAdd = 10;
				}

				protected override int ResolveCore(int model)
				{
					return model + _toAdd;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(s => s.Value, 
						opt => opt.ResolveUsing<CustomResolver>()
							.FromMember(s => s.Value)
							.ConstructedBy(() => new CustomResolver(15)));

				_source = new Source
					{
						Value = 10
					};
			}

			protected override void Because_of()
			{
				_dest = Mapper.Map<Source, Destination>(_source);
			}

			[Test]
			public void Should_use_the_custom_constructor()
			{
				_dest.Value.ShouldEqual(25);
			}
		}

		public class When_specifying_a_member_resolver_and_custom_constructor : AutoMapperSpecBase
		{
			private Source _source;
			private Destination _dest;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

			public class CustomResolver : ValueResolver<int, int>
			{
				private readonly int _toAdd;

				public CustomResolver(int toAdd)
				{
					_toAdd = toAdd;
				}

				public CustomResolver()
				{
					_toAdd = 10;
				}

				protected override int ResolveCore(int model)
				{
					return model + _toAdd;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(s => s.Value,
						opt => opt.ResolveUsing<CustomResolver>()
							.ConstructedBy(() => new CustomResolver(15))
							.FromMember(s => s.Value)
						);

				_source = new Source
				{
					Value = 10
				};
			}

			protected override void Because_of()
			{
				_dest = Mapper.Map<Source, Destination>(_source);
			}

			[Test]
			public void Should_use_the_custom_constructor()
			{
				_dest.Value.ShouldEqual(25);
			}
		}

	}

}