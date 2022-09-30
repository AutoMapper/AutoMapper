namespace AutoMapper.IntegrationTests;
public class ConstructorDefaultValue : IntegrationTest<ConstructorDefaultValue.DatabaseInitializer>
{
    public class Customer
    {
        public int Id { get; set; }
    }
    public class CustomerViewModel
    {
        public CustomerViewModel(int value = 5) => Value = value;
        public int Value { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer());
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using var context = new Context();
        ProjectTo<CustomerViewModel>(context.Customers).Single().Value.ShouldBe(5);
    }
}