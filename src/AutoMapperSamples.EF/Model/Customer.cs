using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapperSamples.EF.Model
{
    /// <summary>
    /// copied from BreezeJS samples: https://github.com/Breeze/breeze.js.samples/blob/master/net/NorthBreeze/Model_NorthwindIB_NH/Entities/
    /// </summary>
    public class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string ContactName { get; set; }
        public virtual string ContactTitle { get; set; }
        public virtual string Address { get; set; }
        public virtual string City { get; set; }
        public virtual string Region { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string Country { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Fax { get; set; }
        public virtual int RowVersion { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}