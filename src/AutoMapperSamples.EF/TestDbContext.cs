using System;
using System.Data.Common;
using System.Data.Entity;
using System.Runtime.Remoting;
using AutoMapperSamples.EF.Model;
using AutoMapperSamples.EF.Model.Configuration;

namespace AutoMapperSamples.EF
{
    public class TestDbContext : TestContextBase<TestDbContext>, ITestContext
    {
        public static bool DynamicProxiesEnabled { get; set; }

        public DbSet<Order> OrderSet { get; set; }

        public DbSet<Customer> CustomerSet { get; set; }

        public TestDbContext(DbConnection dbConnection)
            : base(dbConnection)
        {
            Configuration.ProxyCreationEnabled = DynamicProxiesEnabled;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Configurations.Add(new OrderDetailConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public override void Seed()
        {
            System.Diagnostics.Debug.Print("Seeding db");

            var alex = CustomerSet.Add(new Customer
            {
                Name = "Alex",
                Id = Guid.NewGuid()
            });

            var jenny = CustomerSet.Add(new Customer
            {
                Name="Jenny",
                Id = Guid.NewGuid()
            });

            OrderSet.Add(new Order
            {
                Id = Guid.NewGuid(),
                Name = "Zalando Bestellung",
                OrderDate = new DateTime(2015, 01, 14),
                Price = 150d,
                Customer = jenny,
            });
            OrderSet.Add(new Order
            {
                Id = Guid.NewGuid(),
                Name = "Amazon Bestellung",
                OrderDate = new DateTime(2015, 02, 3),
                Price = 85d,
                Customer = alex,
            });
            OrderSet.Add(new Order
            {
                Id = Guid.NewGuid(),
                Name = "Universalversand",
                OrderDate = new DateTime(2015, 04, 20),
                Price = 33.9d,
                Customer = jenny
            });
            
            SaveChanges();
        }
    }
}