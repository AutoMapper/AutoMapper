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
	}
}