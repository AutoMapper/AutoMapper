using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
	namespace Profiles
	{
		public class When_segregating_configuration_through_a_profile : AutoMapperSpecBase
		{
			private Dto _result;

			public class Model
			{
				public int Value { get; set; }
			}

			public class Dto
			{
				public string Value { get; set; }
			}

			public class Formatter : IValueFormatter
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

			[Fact]
			public void Should_not_include_default_profile_configuration_with_profiled_maps()
			{
				_result.Value.ShouldEqual("5");
			}
		}

		public class When_configuring_a_profile_through_a_profile_subclass : AutoMapperSpecBase
		{
			private Dto _result;
		    private CustomProfile1 _customProfile;

		    public class Model
			{
				public int Value { get; set; }
			}

			public class Dto
			{
				public string Value { get; set; }
			}

			public class Dto2
			{
				public string Value { get; set; }
			}

			public class Formatter : IValueFormatter
			{
				public string FormatValue(ResolutionContext context)
				{
					return context.SourceValue + " Custom";
				}
			}

			public class CustomProfile1 : Profile
			{
				protected override void Configure()
				{
					AddFormatter<Formatter>();

					CreateMap<Model, Dto>();
				}
			}

			public class CustomProfile2 : Profile
			{
				protected override void Configure()
				{
					AddFormatter<Formatter>();

					CreateMap<Model, Dto2>();
				}
			}

			protected override void Establish_context()
			{
			    _customProfile = new CustomProfile1();
			    Mapper.AddProfile(_customProfile);
				Mapper.AddProfile<CustomProfile2>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Model, Dto>(new Model { Value = 5 });
			}

		    [Fact]
		    public void Should_default_the_custom_profile_name_to_the_type_name()
		    {
                _customProfile.ProfileName.ShouldEqual(typeof(CustomProfile1).FullName);
		    }

			[Fact]
			public void Should_use_the_overridden_configuration_method_to_configure()
			{
				_result.Value.ShouldEqual("5 Custom");
			}
		}


        public class When_disabling_constructor_mapping_with_profiles : AutoMapperSpecBase
        {
            private B _b;

            public class AProfile : Profile
            {
                protected override void Configure()
                {
                    DisableConstructorMapping();
                    CreateMap<A, B>();
                }
            }

            public class A
            {
                public string Value { get; set; }
            }

            public class B
            {

                public B()
                {
                }

                public B(string value)
                {
                    throw new Exception();
                }

                public string Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.AddProfile<AProfile>();
            }

            protected override void Because_of()
            {
                _b = Mapper.Map<B>(new A { Value = "BLUEZ" });
            }

            [Fact]
            public void When_using_profile_and_no_constructor_mapping()
            {
                Assert.Equal("BLUEZ", _b.Value);
            }
        }


	}
}