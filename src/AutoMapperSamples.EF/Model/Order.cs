using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapperSamples.EF.Model
{
    /// <summary>
    /// copied from BreezeJS samples: https://github.com/Breeze/breeze.js.samples/blob/master/net/NorthBreeze/Model_NorthwindIB_NH/Entities/
    /// </summary>
    public class Order
    {
        public Order()
        {
            //this.OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual Guid? CustomerID { get; set; }
        public virtual DateTime? OrderDate { get; set; }
        public virtual DateTime? RequiredDate { get; set; }
        public virtual DateTime? ShippedDate { get; set; }
        public virtual double Price { get; set; }
        public virtual string ShipName { get; set; }
        public virtual string ShipAddress { get; set; }
        public virtual string ShipCity { get; set; }
        public virtual string ShipRegion { get; set; }
        public virtual string ShipPostalCode { get; set; }
        public virtual string ShipCountry { get; set; }
        public virtual int RowVersion { get; set; }
        public virtual Customer Customer { get; set; }
        //public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}