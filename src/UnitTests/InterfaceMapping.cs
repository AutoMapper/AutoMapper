using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.InterfaceMapping
{
    public class GenericsAndInterfaces : AutoMapperSpecBase
    {
        MyClass<ContainerClass> source = new MyClass<ContainerClass> { Container = new ContainerClass { MyProperty = 3 } };

        public interface IMyInterface<T>
        {
            T Container { get; set; }
        }

        public class ContainerClass
        {
            public int MyProperty { get; set; }
        }

        public class ImplementedClass : IMyInterface<ContainerClass>
        {
            public ContainerClass Container
            {
                get;
                set;
            }
        }

        public class MyClass<T>
        {
            public T Container { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap(typeof(MyClass<>), typeof(IMyInterface<>)));

        [Fact]
        public void ShouldMapToExistingObject()
        {
            var destination = new ImplementedClass();
            Mapper.Map(source, destination, typeof(MyClass<ContainerClass>), typeof(IMyInterface<ContainerClass>));
            destination.Container.MyProperty.ShouldBe(3);
        }

        [Fact]
        public void ShouldMapToNewObject()
        {
            var destination = (IMyInterface<ContainerClass>) Mapper.Map(source, typeof(MyClass<ContainerClass>), typeof(IMyInterface<ContainerClass>));
            destination.Container.MyProperty.ShouldBe(3);
        }
    }

    public class When_mapping_generic_interface : AutoMapperSpecBase
    {
        public class Source<T> : List<T>
        {
            public String PropertyToMap { get; set; }
            public String PropertyToIgnore { get; set; } = "I am not ignored";
        }

        public class Destination : DestinationBase<String>
        {
        }

        public abstract class DestinationBase<T> : DestinationBaseBase, IDestinationBase<T>
        {
            private String m_PropertyToIgnore;

            public virtual String PropertyToMap { get; set; }

            [IgnoreMap]
            public override String PropertyToIgnore
            {
                get
                {
                    return m_PropertyToIgnore ?? (m_PropertyToIgnore = "Ignore me");
                }
                set
                {
                    m_PropertyToIgnore = value;
                }
            }

            public virtual List<T> Items { get; set; }
        }

        public abstract class DestinationBaseBase
        {
            [IgnoreMap]
            public virtual String PropertyToIgnore { get; set; }
        }

        public interface IDestinationBase<T>
        {
            String PropertyToMap { get; set; }
            List<T> Items { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
            cfg.CreateMap(typeof(IList<>), typeof(IDestinationBase<>))
                    .ForMember(nameof(IDestinationBase<Object>.Items), p_Expression => p_Expression.MapFrom(p_Source => p_Source)));

        [Fact]
        public void Should_work()
        {
            var source = new Source<String>{"Cat", "Dog"};
            source.PropertyToMap = "Hello World";
            var destination = Mapper.Map<IDestinationBase<string>>(source);
            destination.PropertyToMap.ShouldBeNull();
            destination.Items.ShouldBe(source);
        }
    }

    public class When_mapping_an_interface_with_getter_only_member : AutoMapperSpecBase
    {
        interface ISource
        {
            int Id { get; set; }
        }

        public interface IDestination
        {
            int Id { get; set; }
            int ReadOnly { get; }
        }

        class Source : ISource
        {
            public int Id { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c=>c.CreateMap<ISource, IDestination>());

        [Fact]
        public void ShouldMapOk()
        {
            Mapper.Map<IDestination>(new Source { Id = 5 }).Id.ShouldBe(5);
        }
    }

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
            _result.prop2.ShouldBe("PROP2");
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
            _result.Child.ShouldBeOfType(typeof (SubDtoChildObject));
        }

        [Fact]
        public void Should_map_ChildProperty_to_child_property_value()
        {
            _result.Child.ChildProperty.ShouldBe("child property value");
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
            _result.Value.ShouldBe(5);
        }

        [Fact]
        public void Should_not_derive_from_INotifyPropertyChanged()
        {
            _result.ShouldNotBeOfType<INotifyPropertyChanged>();    
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
            _result.Value.ShouldBe(5);
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
                e.PropertyName.ShouldBe("Value");
            };

            _result.Value = 42;
            count.ShouldBe(1);
            _result.Value.ShouldBe(42);
        }

        [Fact]
        public void Should_detach_event_handler()
        {
            _result.PropertyChanged += MyHandler;
            _count.ShouldBe(0);

            _result.Value = 56;
            _count.ShouldBe(1);

            _result.PropertyChanged -= MyHandler;
            _count.ShouldBe(1);

            _result.Value = 75;
            _count.ShouldBe(1);
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
            _result.Id.ShouldBe(7);
        }

        [Fact]
        public void Should_map_derived_interface_property()
        {
            _result.SecondId.ShouldBe(42);
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
            _result.Id.ShouldBe(7);
        }

        [Fact]
        public void Should_map_derived_interface_property()
        {
            _result.SecondId.ShouldBe(42);
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
            _destination.Value.ShouldBe(10);
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
            _baseDtos.First().ShouldBeOfType<DerivedDto>();
        }

    }
}