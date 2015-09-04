using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapperSamples.EF
{
    public class Order
    {
        public string Name { get; set; }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public DateTime Ordered { get; set; }
        public double Price { get; set; }
    }
}