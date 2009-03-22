using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace InterfaceMapping
	{
		public class When_mapping_an_interface_to_an_abstract_type : AutoMapperSpecBase
		{
			private DtoObject _result;

			public class ModelObject
			{
				public IChildModelObject Child { get; set; }
			}

			public interface IChildModelObject
			{
				string ChildProperty { get; set; }
			}

			public class SubChildModelObject : IChildModelObject
			{
				public string ChildProperty { get; set; }
			}
            
			public class DtoObject
			{
				public DtoChildObject Child { get; set; }
			}

			public abstract class DtoChildObject
			{
				public virtual string ChildProperty { get; set; }
			}

			public class SubDtoChildObject : DtoChildObject
			{
			}

			protected override void Establish_context()
			{
				Mapper.Reset();

				var model = new ModelObject
					{
						Child = new SubChildModelObject {ChildProperty = "child property value" }
					};

				Mapper.CreateMap<ModelObject, DtoObject>();

				Mapper.CreateMap<IChildModelObject, DtoChildObject>()
					.Include<SubChildModelObject, SubDtoChildObject>();

				Mapper.CreateMap<SubChildModelObject, SubDtoChildObject>();

				Mapper.AssertConfigurationIsValid();

				_result = Mapper.Map<ModelObject, DtoObject>(model);
			}

			[Test]
			public void Should_map_Child_to_SubDtoChildObject_type()
			{
				_result.Child.ShouldBeInstanceOfType(typeof(SubDtoChildObject));
			}

			[Test]
			public void Should_map_ChildProperty_to_child_property_value()
			{
				_result.Child.ChildProperty.ShouldEqual("child property value");
			}
		}
	
		public class When_mapping_a_concrete_type_to_an_interface_type : AutoMapperSpecBase
		{
			private IDestination _result;

			private class Source
			{
				public int Value { get; set; }
			}

			public interface IDestination
			{
				int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, IDestination>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, IDestination>(new Source {Value = 5});
			}

			[Test]
			public void Should_create_an_implementation_of_the_interface()
			{
				_result.Value.ShouldEqual(5);
			}

			[Test]
			public void Should_pass_configuration_testing()
			{
				Mapper.AssertConfigurationIsValid();
			}
		}

	}
}