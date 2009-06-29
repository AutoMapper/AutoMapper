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

        public class When_mapping_a_derived_interface_to_an_derived_concrete_type : AutoMapperSpecBase
        {
            private Destination _result = null;

            public interface ISourceBase
            {
                int Id { get; }
            }

            public interface ISource : ISourceBase
            {
                int SecondId { get; }
            }

            private class Source : ISource
            {
                public int Id { get; set; }
                public int SecondId { get; set; }
            }

            private abstract class DestinationBase
            {
                public int Id { get; set; }
            }

            private class Destination : DestinationBase
            {
                public int SecondId { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<ISource, Destination>();
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<ISource, Destination>(new Source { Id = 7, SecondId = 42 });
            }

            [Test]
            public void Should_map_base_interface_property()
            {
                _result.Id.ShouldEqual(7);
            }

            [Test]
            public void Should_map_derived_interface_property()
            {
                _result.SecondId.ShouldEqual(42);
            }

            [Test]
            public void Should_pass_configuration_testing()
            {
                Mapper.AssertConfigurationIsValid();
            }
        }

        public class When_mapping_a_derived_interface_to_an_derived_concrete_type_with_readonly_interface_members : AutoMapperSpecBase
        {
            private Destination _result = null;

            public interface ISourceBase
            {
                int Id { get; }
            }

            public interface ISource : ISourceBase
            {
                int SecondId { get; }
            }

            private class Source : ISource
            {
                public int Id { get; set; }
                public int SecondId { get; set; }
            }

            public interface IDestinationBase
            {
                int Id { get; }
            }

            public interface IDestination : IDestinationBase
            {
                int SecondId { get; }
            }

            private abstract class DestinationBase : IDestinationBase
            {
                public int Id { get; set; }
            }

            private class Destination : DestinationBase, IDestination
            {
                public int SecondId { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<ISource, Destination>();
            }

            protected override void Because_of()
            {
                _result = Mapper.Map<ISource, Destination>(new Source { Id = 7, SecondId = 42 });
            }

            [Test]
            public void Should_map_base_interface_property()
            {
                _result.Id.ShouldEqual(7);
            }

            [Test]
            public void Should_map_derived_interface_property()
            {
                _result.SecondId.ShouldEqual(42);
            }

            [Test]
            public void Should_pass_configuration_testing()
            {
                Mapper.AssertConfigurationIsValid();
            }
        }

	    public class When_mapping_to_a_type_with_explicitly_implemented_interface_members : AutoMapperSpecBase
	    {
	        private Destination _destination;

	        public class Source
            {
                public int Value { get; set; }
            }

            public interface IOtherDestination
            {
                int OtherValue { get; set; }
            }

            public class Destination : IOtherDestination
            {
                public int Value { get; set; }
                int IOtherDestination.OtherValue { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Destination>();
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value = 10});
            }

	        [Test]
	        public void Should_ignore_interface_members_for_mapping()
	        {
	            _destination.Value.ShouldEqual(10);
	        }

	        [Test]
	        public void Should_ignore_interface_members_for_validation()
	        {
	            Mapper.AssertConfigurationIsValid();
	        }
	    }

	}
}