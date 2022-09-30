namespace AutoMapper.UnitTests.MappingInheritance;

public class AsWithMissingMap : NonValidatingSpecBase
{
    interface TInterface
    {
        string Value { get; set; }
    }
    class TConcrete : TInterface
    {
        public string Value { get; set; }
    }
    class TModel
    {
        public string Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<TModel, TInterface>().As<TConcrete>());
    [Fact]
    public void Should_report_missing_map() => new Action(AssertConfigurationIsValid).ShouldThrow<InvalidOperationException>().Message.ShouldBe(
        "Missing map from AutoMapper.UnitTests.MappingInheritance.AsWithMissingMap+TModel to AutoMapper.UnitTests.MappingInheritance.AsWithMissingMap+TConcrete. Create using CreateMap<TModel, TConcrete>.");
}
public class AsShouldWorkOnlyWithDerivedTypesWithGenerics : AutoMapperSpecBase
{
    class Source<T>
    {
    }

    class Destination<T>
    {
    }

    class Override<T> : Destination<T>
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap(typeof(Source<>), typeof(Override<>));
        c.CreateMap(typeof(Source<>), typeof(Destination<>)).As(typeof(Override<>));
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class AsShouldWorkOnlyWithDerivedTypes
{
    class Source
    {
    }

    class Destination
    {
    }

    [Fact]
    public void Should_detect_unrelated_override()
    {
        new Action(() => new MapperConfiguration(c => c.CreateMap(typeof(Source), typeof(Destination)).As(typeof(Source)))).ShouldThrowException<ArgumentOutOfRangeException>(ex =>
        {
            ex.Message.ShouldStartWith($"{typeof(Source)} is not derived from {typeof(Destination)}.");
        });
    }
}

public class DestinationTypePolymorphismTest
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerStubDTO
    {
        public int Id { get; set; }
    }

    public class CustomerDTO : CustomerStubDTO
    {
        public string Name { get; set; }
    }

    public class Order
    {
        public Customer Customer { get; set; }
    }

    public class OrderDTO
    {
        public CustomerStubDTO Customer { get; set; }
    }


    [Fact]
    public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
    {
        var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDTO>();
            cfg.CreateMap<Customer, CustomerDTO>();
            cfg.CreateMap<Customer, CustomerStubDTO>().As<CustomerDTO>();
        });
        var orderDto = config.CreateMapper().Map<Order, OrderDTO>(order);

        var customerDto = (CustomerDTO)orderDto.Customer;
        "A".ShouldBe(customerDto.Name);
        1.ShouldBe(customerDto.Id);

    }

}
public class DestinationTypePolymorphismTestNonGeneric
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerStubDTO
    {
        public int Id { get; set; }
    }

    public class CustomerDTO : CustomerStubDTO
    {
        public string Name { get; set; }
    }

    public class Order
    {
        public Customer Customer { get; set; }
    }

    public class OrderDTO
    {
        public CustomerStubDTO Customer { get; set; }
    }


    [Fact]
    public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
    {
        var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Order), typeof(OrderDTO));
            cfg.CreateMap(typeof(Customer), typeof(CustomerDTO));
            cfg.CreateMap(typeof(Customer), typeof(CustomerStubDTO)).As(typeof(CustomerDTO));
        });
        var orderDto = config.CreateMapper().Map<Order, OrderDTO>(order);

        var customerDto = (CustomerDTO)orderDto.Customer;
        "A".ShouldBe(customerDto.Name);
        1.ShouldBe(customerDto.Id);

    }

}

public class AsWithGenerics : AutoMapperSpecBase
{
    INodeModel<int> _destination;

    public interface INodeModel<T> : INodeModel where T : struct
    {
        new T? ID { get; set; }
    }

    public interface INodeModel
    {
        object ID { get; set; }
        string Name { get; set; }
    }

    public class NodeModel<T> : INodeModel<T> where T : struct
    {
        public T? ID { get; set; }
        public string Name { get; set; }

        object INodeModel.ID
        {
            get
            {
                return ID;
            }

            set
            {
                ID = value as T?;
            }
        }
    }

    public class NodeDto<T> where T : struct
    {
        public T? ID { get; set; }
        public string Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
     {
         cfg.CreateMap(typeof(NodeDto<>), typeof(NodeModel<>));
         cfg.CreateMap(typeof(NodeDto<>), typeof(INodeModel<>)).As(typeof(NodeModel<>));
         cfg.CreateMap(typeof(INodeModel<>), typeof(NodeModel<>));
     });

    protected override void Because_of()
    {
        var dto = new NodeDto<int> { ID = 1, Name = "Hi" };
        _destination = Mapper.Map<INodeModel<int>>(dto);
    }

    [Fact]
    public void Should_override_the_map()
    {
        _destination.ShouldBeOfType<NodeModel<int>>();
        _destination.ID.ShouldBe(1);
        _destination.Name.ShouldBe("Hi");
    }
}
