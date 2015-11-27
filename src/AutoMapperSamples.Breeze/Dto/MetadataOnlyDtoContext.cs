using System.Data.Entity;
using AutoMapperSamples.Breeze.Dto.Configuration;
using AutoMapperSamples.EF.Dtos;

namespace AutoMapperSamples.Breeze.Dto
{
    /// <summary>
    /// this context only serves as metadata-gatherer for BreezeJs
    /// see: http://breeze.github.io/doc-js/metadata-with-ef.html for a tutorial/explanation
    /// you could also use SummerBreeze (https://github.com/dotnetricardo/SummerBreeze) but I did not want to add another dependency...
    /// </summary>
    public class MetadataOnlyDtoContext : DbContext
    {
        static MetadataOnlyDtoContext()
        {
            // Prevent attempt to initialize a database for this context
            Database.SetInitializer<MetadataOnlyDtoContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new CustomerDtoConfiguration());
            modelBuilder.Configurations.Add(new OrderDtoConfiguration());
        }
        
        public DbSet<EF.Dtos.CustomerDto> Customers { get; set; }
        public DbSet<EF.Dtos.OrderDto> Orders { get; set; }
    }
}
