using Should;
using Xunit;

namespace AutoMapper.UnitTests.MappingInheritance
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
            destination.Container.MyProperty.ShouldEqual(3);
        }

        [Fact]
        public void ShouldMapToNewObject()
        {
            var destination = (IMyInterface<ContainerClass>) Mapper.Map(source, typeof(MyClass<ContainerClass>), typeof(ImplementedClass));
            destination.Container.MyProperty.ShouldEqual(3);
        }
    }
}