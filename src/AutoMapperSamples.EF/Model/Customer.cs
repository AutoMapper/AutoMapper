using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapperSamples.EF.Model
{
    public class Customer
    {
        public Customer()
        {
            //Orders = new List<Order>();
        }

        public virtual string Name { get; set; }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }

        //public virtual ICollection<Order> Orders { get; set; }
    }
}