using System;
using System.Collections.Generic;
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
                    Child = new SubChildModelObject {ChildProperty = "child property value"}
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
                _result.Child.ShouldBeInstanceOfType(typeof (SubDtoChildObject));
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
                _result = Mapper.Map<ISource, Destination>(new Source {Id = 7, SecondId = 42});
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

        public class When_mapping_a_derived_interface_to_an_derived_concrete_type_with_readonly_interface_members :
            AutoMapperSpecBase
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
                _result = Mapper.Map<ISource, Destination>(new Source {Id = 7, SecondId = 42});
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

        [TestFixture, Explicit]
        public class MappingToInterfacesPolymorphic
        {
            [SetUp]
            public void SetUp()
            {
                Mapper.Reset();
            }

            public interface DomainInterface
            {
                Guid Id { get; set; }
                NestedType Nested { get; set; }
            }

            public class NestedType
            {
                public virtual string Name { get; set; }
                public virtual decimal DecimalValue { get; set; }
            }

            public class DomainImplA : DomainInterface
            {
                public virtual Guid Id { get; set; }
                private NestedType nested;

                public virtual NestedType Nested
                {
                    get
                    {
                        if (nested == null) nested = new NestedType();
                        return nested;
                    }
                    set { nested = value; }
                }
            }

            public class DomainImplB : DomainInterface
            {
                public virtual Guid Id { get; set; }
                private NestedType nested;

                public virtual NestedType Nested
                {
                    get
                    {
                        if (nested == null) nested = new NestedType();
                        return nested;
                    }
                    set { nested = value; }
                }
            }

            public class Dto
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public decimal DecimalValue { get; set; }
            }

            [Test]
            public void CanMapToDomainInterface()
            {
                Mapper.CreateMap<DomainInterface, Dto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nested.Name))
                    .ForMember(dest => dest.DecimalValue, opt => opt.MapFrom(src => src.Nested.DecimalValue));
                Mapper.CreateMap<Dto, DomainInterface>()
                    .ForMember(dest => dest.Nested.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Nested.DecimalValue, opt => opt.MapFrom(src => src.DecimalValue));

                var domainInstance1 = new DomainImplA();
                var domainInstance2 = new DomainImplB();
                var domainInstance3 = new DomainImplA();

                var dtoCollection = new List<Dto>
                {
                    Mapper.Map<DomainInterface, Dto>(domainInstance1),
                    Mapper.Map<DomainInterface, Dto>(domainInstance2),
                    Mapper.Map<DomainInterface, Dto>(domainInstance3)
                };

                dtoCollection[0].Id = Guid.NewGuid();
                dtoCollection[0].DecimalValue = 1M;
                dtoCollection[0].Name = "Bob";
                dtoCollection[1].Id = Guid.NewGuid();
                dtoCollection[1].DecimalValue = 0.1M;
                dtoCollection[1].Name = "Frank";
                dtoCollection[2].Id = Guid.NewGuid();
                dtoCollection[2].DecimalValue = 2.1M;
                dtoCollection[2].Name = "Sam";

                Mapper.Map<Dto, DomainInterface>(dtoCollection[0], domainInstance1);
                Mapper.Map<Dto, DomainInterface>(dtoCollection[1], domainInstance2);
                Mapper.Map<Dto, DomainInterface>(dtoCollection[2], domainInstance3);

                Assert.AreEqual(dtoCollection[0].Id, domainInstance1.Id);
                Assert.AreEqual(dtoCollection[1].Id, domainInstance2.Id);
                Assert.AreEqual(dtoCollection[2].Id, domainInstance3.Id);

                Assert.AreEqual(dtoCollection[0].DecimalValue, domainInstance1.Nested.DecimalValue);
                Assert.AreEqual(dtoCollection[1].DecimalValue, domainInstance2.Nested.DecimalValue);
                Assert.AreEqual(dtoCollection[2].DecimalValue, domainInstance3.Nested.DecimalValue);

                Assert.AreEqual(dtoCollection[0].DecimalValue, domainInstance1.Nested.Name);
                Assert.AreEqual(dtoCollection[1].DecimalValue, domainInstance2.Nested.Name);
                Assert.AreEqual(dtoCollection[2].DecimalValue, domainInstance3.Nested.Name);
            }
        }
    }
}