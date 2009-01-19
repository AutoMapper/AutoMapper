using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace Profiles
	{
		public class When_segregating_configuration_through_a_profile : AutoMapperSpecBase
		{
			private Dto _result;

			private class Model
			{
				public int Value { get; set; }
			}

			private class Dto
			{
				public string Value { get; set; }
			}

			private class Formatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return context.SourceValue + " Custom";
				}
			}

			protected override void Establish_context()
			{
				Mapper.AddFormatter<Formatter>();

				Mapper.CreateProfile("Custom");

				Mapper.CreateMap<Model, Dto>().WithProfile("Custom");
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Model, Dto>(new Model {Value = 5});
			}

			[Test]
			public void Should_not_include_default_profile_configuration_with_profiled_maps()
			{
				_result.Value.ShouldEqual("5");
			}
		}

		public class When_configuring_an_entire_profile_at_once : AutoMapperSpecBase
		{
			private TypeMap _typeMap1;
			private TypeMap _typeMap2;

			private class Model1
			{
				public int Value { get; set; }
			}

			private class Dto1
			{
				public string Value { get; set; }
			}

			private class Model2
			{
				public int Value { get; set; }
			}

			private class Dto2
			{
				public string Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateProfile("Custom1", x => x.CreateMap<Model1, Dto1>());
				Mapper.CreateProfile("Custom2").CreateMap<Model2, Dto2>();
			}

			protected override void Because_of()
			{
				_typeMap1 = Mapper.FindTypeMapFor<Model1, Dto1>();
				_typeMap2 = Mapper.FindTypeMapFor<Model2, Dto2>();
			}

			[Test]
			public void Should_configure_each_type_map_to_have_the_specified_profile()
			{
				_typeMap1.Profile.ShouldEqual("Custom1");
				_typeMap2.Profile.ShouldEqual("Custom2");
			}
		}

		public class When_configuring_a_profile_through_a_profile_subclass : AutoMapperSpecBase
		{
			private Dto _result;

			private class Model
			{
				public int Value { get; set; }
			}

			private class Dto
			{
				public string Value { get; set; }
			}

			private class Dto2
			{
				public string Value { get; set; }
			}

			private class Formatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return context.SourceValue + " Custom";
				}
			}

			private class CustomProfile1 : Profile
			{
				protected override void Configure()
				{
					AddFormatter<Formatter>();

					CreateMap<Model, Dto>();
				}

				protected override string ProfileName
				{
					get
					{
						return "Custom1";
					}
				}
			}

			private class CustomProfile2 : Profile
			{
				protected override void Configure()
				{
					AddFormatter<Formatter>();

					CreateMap<Model, Dto2>();
				}

				protected override string ProfileName
				{
					get
					{
						return "Custom2";
					}
				}
			}

			protected override void Establish_context()
			{
				Mapper.AddProfile(new CustomProfile1());
				Mapper.AddProfile<CustomProfile2>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Model, Dto>(new Model { Value = 5 });
			}

			[Test]
			public void Should_use_the_overridden_configuration_method_to_configure()
			{
				_result.Value.ShouldEqual("5 Custom");
			}

			[Test]
			public void Should_assign_the_correct_profile_to_each_configured_type_map()
			{
				Mapper.FindTypeMapFor<Model, Dto>().Profile.ShouldEqual("Custom1");
				Mapper.FindTypeMapFor<Model, Dto2>().Profile.ShouldEqual("Custom2");
			}
		}

	}
}