using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace InterfaceMapping
    {
        public class When_mapping_base_interface_members
        {
            public interface ISource
            {
                int Id { get; set; }
            }

            public interface ITarget : ITargetBase
            {
                int Id { get; set; }
            }

            public interface ITargetBase
            {
                int BaseId { get; set; }
            }

            [Fact]
            public void Should_find_inherited_members_by_name()
            {
                new MapperConfiguration(c=>c.CreateMap<ISource, ITarget>().ForMember("BaseId", opt => opt.Ignore()));
            }
        }

        public class When_mapping_to_existing_object_through_interfaces : AutoMapperSpecBase
        {
            private class2DTO _result;

            public class class1 : iclass1
            {
                public string prop1 { get; set; }
            }

            public class class2 : class1, iclass2
            {
                public string prop2 { get; set; }
            }

            public class class1DTO : iclass1DTO
            {
                public string prop1 { get; set; }
            }

            public class class2DTO : class1DTO, iclass2DTO
            {
                public string prop2 { get; set; }
            }

            public interface iclass1
            {
                string prop1 { get; set; }
            }

            public interface iclass2
            {
                string prop2 { get; set; }
            }

            public interface iclass1DTO
            {
                string prop1 { get; set; }
            }

            public interface iclass2DTO
            {
                string prop2 { get; set; }
            }

            protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<iclass1, iclass1DTO>();
                cfg.CreateMap<iclass2, iclass2DTO>();
            });

            protected override void Because_of()
            {
                var bo = new class2 { prop1 = "PROP1", prop2 = "PROP2" };
                _result = Mapper.Map(bo, new class2DTO());
            }

            [Fact]
            public void Should_use_the_most_derived_interface()
            {
                _result.prop2.ShouldEqual("PROP2");
            }
        }

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

            protected override void Because_of()
            {
                var model = new ModelObject
                {
                    Child = new SubChildModelObject {ChildProperty = "child property value"}
                };
                _result = Mapper.Map<ModelObject, DtoObject>(model);
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<ModelObject, DtoObject>();

                cfg.CreateMap<IChildModelObject, DtoChildObject>()
                    .Include<SubChildModelObject, SubDtoChildObject>();

                cfg.CreateMap<SubChildModelObject, SubDtoChildObject>();
            });

            [Fact]
            public void Should_map_Child_to_SubDtoChildObject_type()
            {
                _result.Child.ShouldBeType(typeof (SubDtoChildObject));
            }

            [Fact]
            public void Should_map_ChildProperty_to_child_property_value()
            {
                _result.Child.ChildProperty.ShouldEqual("child property value");
            }
        }

        public class When_mapping_a_concrete_type_to_an_interface_type : AutoMapperSpecBase
        {
            private IDestination _result;

            public class Source
            {
                public int Value { get; set; }
            }

            public interface IDestination
            {
                int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, IDestination>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, IDestination>(new Source {Value = 5});
            }

            [Fact]
            public void Should_create_an_implementation_of_the_interface()
            {
                _result.Value.ShouldEqual(5);
            }

            [Fact]
            public void Should_not_derive_from_INotifyPropertyChanged()
            {
                _result.ShouldNotBeInstanceOf<INotifyPropertyChanged>();    
            }
        }

        public class When_mapping_a_concrete_type_to_an_interface_type_that_derives_from_INotifyPropertyChanged : AutoMapperSpecBase
        {
            private IDestination _result;

            private int _count;

            public class Source
            {
                public int Value { get; set; }
            }

            public interface IDestination : INotifyPropertyChanged
            {
                int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, IDestination>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, IDestination>(new Source {Value = 5});
            }

            [Fact]
            public void Should_create_an_implementation_of_the_interface()
            {
                _result.Value.ShouldEqual(5);
            }

            [Fact]
            public void Should_derive_from_INotifyPropertyChanged()
            {
                var q = _result as INotifyPropertyChanged;
                q.ShouldNotBeNull();
            }

            [Fact]
            public void Should_notify_property_changes()
            {
                var count = 0;
                _result.PropertyChanged += (o, e) => {
                    count++;
                    o.ShouldBeSameAs(_result); 
                    e.PropertyName.ShouldEqual("Value");
                };

                _result.Value = 42;
                count.ShouldEqual(1);
                _result.Value.ShouldEqual(42);
            }

            [Fact]
            public void Should_detach_event_handler()
            {
                _result.PropertyChanged += MyHandler;
                _count.ShouldEqual(0);

                _result.Value = 56;
                _count.ShouldEqual(1);

                _result.PropertyChanged -= MyHandler;
                _count.ShouldEqual(1);

                _result.Value = 75;
                _count.ShouldEqual(1);
            }

            private void MyHandler(object sender, PropertyChangedEventArgs e) {
                _count++;
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

            public class Source : ISource
            {
                public int Id { get; set; }
                public int SecondId { get; set; }
            }

            public abstract class DestinationBase
            {
                public int Id { get; set; }
            }

            public class Destination : DestinationBase
            {
                public int SecondId { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ISource, Destination>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<ISource, Destination>(new Source {Id = 7, SecondId = 42});
            }

            [Fact]
            public void Should_map_base_interface_property()
            {
                _result.Id.ShouldEqual(7);
            }

            [Fact]
            public void Should_map_derived_interface_property()
            {
                _result.SecondId.ShouldEqual(42);
            }

            [Fact]
            public void Should_pass_configuration_testing()
            {
                Configuration.AssertConfigurationIsValid();
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

            public class Source : ISource
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

            public abstract class DestinationBase : IDestinationBase
            {
                public int Id { get; set; }
            }

            public class Destination : DestinationBase, IDestination
            {
                public int SecondId { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ISource, Destination>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<ISource, Destination>(new Source {Id = 7, SecondId = 42});
            }

            [Fact]
            public void Should_map_base_interface_property()
            {
                _result.Id.ShouldEqual(7);
            }

            [Fact]
            public void Should_map_derived_interface_property()
            {
                _result.SecondId.ShouldEqual(42);
            }

            [Fact]
            public void Should_pass_configuration_testing()
            {
                Configuration.AssertConfigurationIsValid();
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

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value = 10});
            }

            [Fact]
            public void Should_ignore_interface_members_for_mapping()
            {
                _destination.Value.ShouldEqual(10);
            }

            [Fact]
            public void Should_ignore_interface_members_for_validation()
            {
                Configuration.AssertConfigurationIsValid();
            }
        }

        public class MappingToInterfacesWithPolymorphism : AutoMapperSpecBase
        {
            private BaseDto[] _baseDtos;

            public interface IBase { }
            public interface IDerived : IBase { }
            public class Base : IBase { }
            public class Derived : Base, IDerived { }
            public class BaseDto { }
            public class DerivedDto : BaseDto { }

            //and following mappings:
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base, BaseDto>().Include<Derived, DerivedDto>();
                cfg.CreateMap<Derived, DerivedDto>();
                cfg.CreateMap<IBase, BaseDto>().Include<IDerived, DerivedDto>();
                cfg.CreateMap<IDerived, DerivedDto>();
            });

            protected override void Because_of()
            {
                List<Base> list = new List<Base>() { new Derived() };
                _baseDtos = Mapper.Map<IEnumerable<Base>, BaseDto[]>(list);
            }

            [Fact]
            public void Should_use_the_derived_type_map()
            {
                _baseDtos.First().ShouldBeType<DerivedDto>();
            }

        }

        //[TestFixture, Explicit]
        //public class MappingToInterfacesPolymorphic
        //{
        //    [SetUp]
        //    public void SetUp()
        //    {
        //        
        //    }

        //    public interface DomainInterface
        //    {
        //        Guid Id { get; set; }
        //        NestedType Nested { get; set; }
        //    }

        //    public class NestedType
        //    {
        //        public virtual string Name { get; set; }
        //        public virtual decimal DecimalValue { get; set; }
        //    }

        //    public class DomainImplA : DomainInterface
        //    {
        //        public virtual Guid Id { get; set; }
        //        private NestedType nested;

        //        public virtual NestedType Nested
        //        {
        //            get
        //            {
        //                if (nested == null) nested = new NestedType();
        //                return nested;
        //            }
        //            set { nested = value; }
        //        }
        //    }

        //    public class DomainImplB : DomainInterface
        //    {
        //        public virtual Guid Id { get; set; }
        //        private NestedType nested;

        //        public virtual NestedType Nested
        //        {
        //            get
        //            {
        //                if (nested == null) nested = new NestedType();
        //                return nested;
        //            }
        //            set { nested = value; }
        //        }
        //    }

        //    public class Dto
        //    {
        //        public Guid Id { get; set; }
        //        public string Name { get; set; }
        //        public decimal DecimalValue { get; set; }
        //    }

        //    [Fact]
        //    public void CanMapToDomainInterface()
        //    {
        //        Mapper.CreateMap<DomainInterface, Dto>()
        //            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nested.Name))
        //            .ForMember(dest => dest.DecimalValue, opt => opt.MapFrom(src => src.Nested.DecimalValue));
        //        Mapper.CreateMap<Dto, DomainInterface>()
        //            .ForMember(dest => dest.Nested.Name, opt => opt.MapFrom(src => src.Name))
        //            .ForMember(dest => dest.Nested.DecimalValue, opt => opt.MapFrom(src => src.DecimalValue));

        //        var domainInstance1 = new DomainImplA();
        //        var domainInstance2 = new DomainImplB();
        //        var domainInstance3 = new DomainImplA();

        //        var dtoCollection = new List<Dto>
        //        {
        //            Mapper.Map<DomainInterface, Dto>(domainInstance1),
        //            Mapper.Map<DomainInterface, Dto>(domainInstance2),
        //            Mapper.Map<DomainInterface, Dto>(domainInstance3)
        //        };

        //        dtoCollection[0].Id = Guid.NewGuid();
        //        dtoCollection[0].DecimalValue = 1M;
        //        dtoCollection[0].Name = "Bob";
        //        dtoCollection[1].Id = Guid.NewGuid();
        //        dtoCollection[1].DecimalValue = 0.1M;
        //        dtoCollection[1].Name = "Frank";
        //        dtoCollection[2].Id = Guid.NewGuid();
        //        dtoCollection[2].DecimalValue = 2.1M;
        //        dtoCollection[2].Name = "Sam";

        //        Mapper.Map<Dto, DomainInterface>(dtoCollection[0], domainInstance1);
        //        Mapper.Map<Dto, DomainInterface>(dtoCollection[1], domainInstance2);
        //        Mapper.Map<Dto, DomainInterface>(dtoCollection[2], domainInstance3);

        //        dtoCollection[0].Id.ShouldEqual(domainInstance1.Id);
        //        dtoCollection[1].Id.ShouldEqual(domainInstance2.Id);
        //        dtoCollection[2].Id.ShouldEqual(domainInstance3.Id);

        //        dtoCollection[0].DecimalValue.ShouldEqual(domainInstance1.Nested.DecimalValue);
        //        dtoCollection[1].DecimalValue.ShouldEqual(domainInstance2.Nested.DecimalValue);
        //        dtoCollection[2].DecimalValue.ShouldEqual(domainInstance3.Nested.DecimalValue);

        //        dtoCollection[0].DecimalValue.ShouldEqual(domainInstance1.Nested.Name);
        //        dtoCollection[1].DecimalValue.ShouldEqual(domainInstance2.Nested.Name);
        //        dtoCollection[2].DecimalValue.ShouldEqual(domainInstance3.Nested.Name);
        //    }
        //}
    }
}