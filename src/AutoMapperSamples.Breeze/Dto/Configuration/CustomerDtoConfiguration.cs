using AutoMapperSamples.EF.Dtos;

namespace AutoMapperSamples.Breeze.Dto.Configuration
{
    public class CustomerDtoConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<EF.Dtos.CustomerDto>
    {
        public CustomerDtoConfiguration()
            :base()
        {
            HasKey(e => e.Id);
            Property(e => e.Name).IsRequired().HasMaxLength(60);
        }
    }
}