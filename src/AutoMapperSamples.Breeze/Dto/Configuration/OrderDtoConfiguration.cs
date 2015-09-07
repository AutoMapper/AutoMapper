using AutoMapperSamples.EF.Dtos;

namespace AutoMapperSamples.Breeze.Dto.Configuration
{
    public class OrderDtoConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<EF.Dtos.OrderDto>
    {
        public OrderDtoConfiguration()
        {
            HasKey(e => e.Id);
            Property(e => e.FullName).IsRequired().HasMaxLength(250);
        }
    }
}