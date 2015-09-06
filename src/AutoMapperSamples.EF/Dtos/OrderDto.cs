using System;

namespace AutoMapperSamples.EF.Dtos
{
    public class OrderDto
    {
        public string FullName { get; set; }
        public Guid Id { get; set; }
        public DateTime Ordered { get; set; }
        public double Price { get; set; }
        //public CustomerDto Customer { get; set; }
    }
}