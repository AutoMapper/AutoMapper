using System;

namespace AutoMapperSamples.EF.Dtos
{
    public class OrderDto
    {
        public OrderDto()
        {
            
        }

        public string FullName { get; set; }
        public Guid Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public double Price { get; set; }
        public CustomerDto Customer { get; set; }
        public virtual Guid? CustomerID { get; set; }
        public virtual DateTime? RequiredDate { get; set; }
        public virtual DateTime? ShippedDate { get; set; }
        public virtual string ShipName { get; set; }
        public virtual string ShipAddress { get; set; }
        public virtual string ShipCity { get; set; }
        public virtual string ShipRegion { get; set; }
        public virtual string ShipPostalCode { get; set; }
        public virtual string ShipCountry { get; set; }
        public virtual int RowVersion { get; set; }
    }
}