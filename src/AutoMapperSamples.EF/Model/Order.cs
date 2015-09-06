using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapperSamples.EF.Model
{
    public class Order
    {
        public virtual string Name { get; set; }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }
        public virtual DateTime Ordered { get; set; }
        public virtual double Price { get; set; }
        public virtual Customer Customer { get; set; }
    }
}