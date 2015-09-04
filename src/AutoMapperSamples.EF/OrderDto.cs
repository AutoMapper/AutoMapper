using System;

namespace AutoMapperSamples.EF
{
    public class OrderDto
    {
        public string FullName { get; set; }
        public Guid Id { get; set; }
        public DateTime Ordered { get; set; }
        public double Price { get; set; }
    }
}