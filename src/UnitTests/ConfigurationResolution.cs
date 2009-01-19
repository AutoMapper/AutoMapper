using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace ConfigurationResolution
	{
		public class When_attempting_to_resolve_configuration_for_model_subtype : SpecBase
		{
			private class ModelObject
			{
				public string Blarg { get; set; }
			}

			private class ModelSubtypeObject : ModelObject
			{
			}

			private class ModelDto
			{
				public string Blarg { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper.CreateMap<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_resolve_to_the_configuration_for_the_base_type()
			{
				AutoMapper.FindTypeMapFor<ModelSubtypeObject, ModelDto>().ShouldNotBeNull();
			}
		}
	}
}